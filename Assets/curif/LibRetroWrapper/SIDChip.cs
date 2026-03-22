using System;
using UnityEngine;

/// <summary>
/// MOS 6581/8580 SID chip emulator: 3 voices, ADSR envelopes, waveforms, state-variable filter.
/// </summary>
public class SIDChip
{
    private const int SID_CLOCK = 985248; // PAL

    // Cycles per envelope step for each ADSR nibble value (at 1 MHz)
    private static readonly int[] RateTable =
    {
        9, 32, 63, 95, 149, 220, 267, 313,
        392, 977, 1954, 3126, 3907, 11720, 19532, 31251
    };

    // Exponential decay: how many rate-periods before each envelope decrement
    private static readonly int[] ExpTable = new int[256];

    static SIDChip()
    {
        for (int i = 0; i < 256; i++)
        {
            if      (i > 93) ExpTable[i] = 1;
            else if (i > 54) ExpTable[i] = 2;
            else if (i > 26) ExpTable[i] = 4;
            else if (i > 14) ExpTable[i] = 8;
            else if (i >  6) ExpTable[i] = 16;
            else              ExpTable[i] = 30;
        }
    }

    // Control register bit masks
    private const byte CTRL_GATE  = 0x01;
    private const byte CTRL_SYNC  = 0x02;
    private const byte CTRL_RING  = 0x04;
    private const byte CTRL_TEST  = 0x08;
    private const byte CTRL_TRI   = 0x10;
    private const byte CTRL_SAW   = 0x20;
    private const byte CTRL_PULSE = 0x40;
    private const byte CTRL_NOISE = 0x80;

    private class Voice
    {
        public ushort FreqReg;
        public ushort PulseWidth;   // 12-bit
        public byte   Control;
        public byte   Attack, Decay, Sustain, Release;

        public uint PhaseAcc;       // 24-bit oscillator accumulator
        public uint PrevPhaseAcc;
        public uint NoiseLFSR = 0x7FFFFF;

        public byte EnvValue;       // 0-255
        public int  EnvCounter;     // counts down cycles; when ≤ 0 → step
        public int  ExpCounter;     // exponential multiplier countdown

        public enum State { Idle, Attack, Decay, Sustain, Release }
        public State EnvState = State.Idle;
    }

    private readonly Voice[] voices = new Voice[3];
    private readonly int sampleRate;
    private float cyclesPerSample;
    private float accumCycles;

    // Filter registers
    private int   filterCutoff;     // 0-2047
    private byte  filterResonance;  // 0-15
    private byte  filterMode;       // bits: LP=1, BP=2, HP=4, 3OFF=8
    private byte  filterRouting;    // bits 0-2: voice 1,2,3 through filter
    private byte  masterVolume;     // 0-15

    // SVF filter state
    private float filterLP, filterBP;

    public SIDChip(int sampleRate)
    {
        this.sampleRate = sampleRate;
        cyclesPerSample = (float)SID_CLOCK / sampleRate;
        for (int i = 0; i < 3; i++) voices[i] = new Voice();
        Reset();
    }

    public void Reset()
    {
        filterLP = filterBP = 0;
        masterVolume = 0;
        filterCutoff = 0;
        filterResonance = 0;
        filterMode = 0;
        filterRouting = 0;
        foreach (var v in voices)
        {
            v.PhaseAcc = 0; v.PrevPhaseAcc = 0;
            v.NoiseLFSR = 0x7FFFFF;
            v.EnvValue = 0; v.EnvState = Voice.State.Idle;
            v.EnvCounter = int.MaxValue; v.ExpCounter = 1;
            v.FreqReg = 0; v.PulseWidth = 0; v.Control = 0;
        }
    }

    public void WriteRegister(int offset, byte value)
    {
        if (offset < 0 || offset > 24) return;

        int vi = offset / 7;
        int reg = offset % 7;

        if (vi < 3)
        {
            var v = voices[vi];
            switch (reg)
            {
                case 0: v.FreqReg  = (ushort)((v.FreqReg  & 0xFF00) | value); break;
                case 1: v.FreqReg  = (ushort)((v.FreqReg  & 0x00FF) | (value << 8)); break;
                case 2: v.PulseWidth = (ushort)((v.PulseWidth & 0x0F00) | value); break;
                case 3: v.PulseWidth = (ushort)((v.PulseWidth & 0x00FF) | ((value & 0x0F) << 8)); break;
                case 4:
                    bool gate     = (value & CTRL_GATE) != 0;
                    bool prevGate = (v.Control & CTRL_GATE) != 0;
                    v.Control = value;
                    if (gate && !prevGate)
                    {
                        v.EnvState   = Voice.State.Attack;
                        v.EnvCounter = RateTable[v.Attack];
                        v.ExpCounter = 1;
                        if ((value & CTRL_TEST) != 0) v.PhaseAcc = 0;
                    }
                    else if (!gate && prevGate)
                    {
                        v.EnvState   = Voice.State.Release;
                        v.EnvCounter = RateTable[v.Release];
                        v.ExpCounter = ExpTable[v.EnvValue];
                    }
                    break;
                case 5: v.Attack = (byte)((value >> 4) & 0xF); v.Decay   = (byte)(value & 0xF); break;
                case 6: v.Sustain = (byte)((value >> 4) & 0xF); v.Release = (byte)(value & 0xF); break;
            }
        }
        else if (offset == 21) filterCutoff = (filterCutoff & 0x7F8) | (value & 7);
        else if (offset == 22) filterCutoff = (filterCutoff & 0x007) | (value << 3);
        else if (offset == 23) { filterResonance = (byte)((value >> 4) & 0xF); filterRouting = (byte)(value & 0x07); }
        else if (offset == 24) { filterMode = (byte)((value >> 4) & 0x0F); masterVolume = (byte)(value & 0x0F); }
    }

    /// <summary>Generate one audio sample. Called once per output sample from the audio thread.</summary>
    public float GetSample()
    {
        accumCycles += cyclesPerSample;
        int cycles = (int)accumCycles;
        accumCycles -= cycles;

        bool voice3Off = (filterMode & 0x8) != 0;

        float filteredMix = 0f;
        float directMix   = 0f;

        for (int i = 0; i < 3; i++)
        {
            var v    = voices[i];
            var prev = voices[(i + 2) % 3];

            AdvanceOscillator(v, prev, cycles);
            StepEnvelope(v, cycles);

            float wave   = GetWaveform(v, prev);
            float sample = wave * v.EnvValue / 255f;

            if (i == 2 && voice3Off) continue;

            if ((filterRouting & (1 << i)) != 0)
                filteredMix += sample;
            else
                directMix += sample;
        }

        if (filterRouting != 0)
            filteredMix = ApplyFilter(filteredMix);

        float output = (filteredMix + directMix) / 3f * (masterVolume / 15f);
        return Mathf.Clamp(output, -1f, 1f);
    }

    private void AdvanceOscillator(Voice v, Voice prev, int cycles)
    {
        v.PrevPhaseAcc = v.PhaseAcc;
        if ((v.Control & CTRL_TEST) != 0) return;

        v.PhaseAcc = (v.PhaseAcc + (uint)(v.FreqReg * cycles)) & 0xFFFFFF;

        // Sync: reset this voice when the previous voice crosses zero
        if ((v.Control & CTRL_SYNC) != 0)
        {
            bool zeroCross = (prev.PrevPhaseAcc & 0x800000) != 0 && (prev.PhaseAcc & 0x800000) == 0;
            if (zeroCross) v.PhaseAcc = 0;
        }

        // Clock noise LFSR when bit 19 transitions 0→1
        bool noiseClk = (v.PrevPhaseAcc & 0x080000) == 0 && (v.PhaseAcc & 0x080000) != 0;
        if (noiseClk)
        {
            uint fb = ((v.NoiseLFSR >> 22) ^ (v.NoiseLFSR >> 17)) & 1;
            v.NoiseLFSR = ((v.NoiseLFSR << 1) | fb) & 0x7FFFFF;
        }
    }

    private void StepEnvelope(Voice v, int cycles)
    {
        if (v.EnvState == Voice.State.Idle) return;

        v.EnvCounter -= cycles;
        while (v.EnvCounter <= 0)
        {
            switch (v.EnvState)
            {
                case Voice.State.Attack:
                    v.EnvCounter += RateTable[v.Attack];
                    v.EnvValue++;
                    if (v.EnvValue == 255)
                    {
                        v.EnvState   = Voice.State.Decay;
                        v.EnvCounter = RateTable[v.Decay];
                        v.ExpCounter = 1;
                    }
                    break;

                case Voice.State.Decay:
                    v.EnvCounter += RateTable[v.Decay];
                    v.ExpCounter--;
                    if (v.ExpCounter <= 0)
                    {
                        int sl = v.Sustain * 17;
                        if (v.EnvValue > sl)
                        {
                            v.EnvValue--;
                            v.ExpCounter = ExpTable[v.EnvValue];
                        }
                        else
                        {
                            v.EnvState   = Voice.State.Sustain;
                            v.EnvCounter = int.MaxValue;
                        }
                    }
                    break;

                case Voice.State.Sustain:
                    v.EnvValue   = (byte)(v.Sustain * 17);
                    v.EnvCounter = int.MaxValue;
                    break;

                case Voice.State.Release:
                    v.EnvCounter += RateTable[v.Release];
                    v.ExpCounter--;
                    if (v.ExpCounter <= 0)
                    {
                        if (v.EnvValue > 0)
                        {
                            v.EnvValue--;
                            v.ExpCounter = ExpTable[v.EnvValue];
                        }
                        else
                        {
                            v.EnvState   = Voice.State.Idle;
                            v.EnvCounter = int.MaxValue;
                        }
                    }
                    break;

                default:
                    v.EnvCounter = int.MaxValue;
                    break;
            }
        }
    }

    private float GetWaveform(Voice v, Voice prev)
    {
        byte ctrl = v.Control;
        if ((ctrl & (CTRL_NOISE | CTRL_PULSE | CTRL_SAW | CTRL_TRI)) == 0) return 0f;

        uint acc = v.PhaseAcc;
        float result = 0f;
        int count = 0;

        if ((ctrl & CTRL_SAW) != 0)
        {
            result += (acc >> 12) & 0xFFF; // 0-4095
            count++;
        }

        if ((ctrl & CTRL_TRI) != 0)
        {
            uint triAcc = acc;
            if ((ctrl & CTRL_RING) != 0 && (prev.PhaseAcc & 0x800000) != 0)
                triAcc ^= 0x800000;
            uint tri = (triAcc & 0x800000) != 0
                ? (~triAcc >> 11) & 0xFFE
                :  (triAcc >> 11) & 0xFFE;
            result += tri; // 0-4094
            count++;
        }

        if ((ctrl & CTRL_PULSE) != 0)
        {
            uint pw = (uint)(v.PulseWidth & 0xFFF);
            result += ((acc >> 12) & 0xFFF) >= pw ? 4095f : 0f;
            count++;
        }

        if ((ctrl & CTRL_NOISE) != 0)
        {
            uint lfsr = v.NoiseLFSR;
            uint noise =
                ((lfsr >> 22) & 1) << 11 |
                ((lfsr >> 20) & 1) << 10 |
                ((lfsr >> 16) & 1) <<  9 |
                ((lfsr >> 13) & 1) <<  8 |
                ((lfsr >> 11) & 1) <<  7 |
                ((lfsr >>  7) & 1) <<  6 |
                ((lfsr >>  4) & 1) <<  5 |
                ((lfsr >>  2) & 1) <<  4;
            result += noise; // 0-4080
            count++;
        }

        if (count == 0) return 0f;
        return (result / count - 2048f) / 2048f; // center at 0, normalise to ~[-1,1]
    }

    private float ApplyFilter(float input)
    {
        if ((filterMode & 0x7) == 0) return input;

        // Chamberlin state-variable filter
        float fc = 30f + filterCutoff / 2047f * 11970f; // 30 Hz – 12 kHz
        float q  = 0.1f + (1f - filterResonance / 15f) * 1.3f; // damping: 0.1 (high res) – 1.4 (low res)
        float f  = 2f * Mathf.Sin(Mathf.PI * fc / sampleRate);
        // Chamberlin SVF is stable only when f < 2 - q.
        // At 22050 Hz, high-cutoff values produce f ≈ 1.98 which violates this for any resonance,
        // blowing the state variables to Infinity/NaN and silencing all output permanently.
        f = Mathf.Min(f, 2f - q - 0.05f);

        // Recover from any NaN/Inf accumulated before this clamp was in place
        if (!float.IsFinite(filterLP)) filterLP = 0f;
        if (!float.IsFinite(filterBP)) filterBP = 0f;

        filterLP += f * filterBP;
        float hp  = input - filterLP - q * filterBP;
        filterBP += f * hp;

        float output = 0f;
        if ((filterMode & 0x1) != 0) output += filterLP;
        if ((filterMode & 0x2) != 0) output += filterBP;
        if ((filterMode & 0x4) != 0) output += hp;

        return Mathf.Clamp(output, -1f, 1f);
    }
}
