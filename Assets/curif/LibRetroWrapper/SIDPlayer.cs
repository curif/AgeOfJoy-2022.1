using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Drives SID music playback via OnAudioFilterRead.
/// Attached dynamically to the same GameObject as the cabinet's existing AudioSource
/// by basicAGE.InitComponents(). Multiple named SID instances can play simultaneously.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SIDPlayer : MonoBehaviour
{
    private class SIDInstance
    {
        public SIDFile  File;
        public SID6502  Cpu;
        public SIDChip  Chip;
        public int      CurrentSong;      // 1-based
        public float    Volume = 1f;      // 0.0 – 1.0
        public bool     Playing;
        public bool     Paused;
        public float    SamplesUntilPlay; // countdown to next 50 Hz play call
        public readonly object Lock = new object();

        public int Status => Playing ? (Paused ? 3 : 2) : 1;
    }

    private const float PAL_PLAY_HZ = 50f;

    private int sampleRate;
    private AudioSource audioSource;
    private AudioClip   silentClip;

    // Main-thread dictionary; enumerated under lock(instances) in audio thread
    private readonly Dictionary<string, SIDInstance> instances = new Dictionary<string, SIDInstance>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sampleRate  = AudioSettings.outputSampleRate;

        // SID output must be dry/clean — disable OVR spatializer and all spatial effects
        audioSource.spatialize        = false; // disable OVR Audio spatializer plugin (source of reverb)
        audioSource.bypassReverbZones = true;
        audioSource.reverbZoneMix     = 0f;
        audioSource.spatialBlend      = 0f;   // pure 2D, no 3D rolloff
    }

    void Start()
    {
        EnsureAudioRunning();
    }

    void Update()
    {
        bool anyPlaying = false;
        lock (instances)
        {
            foreach (var kvp in instances)
                if (kvp.Value.Playing && !kvp.Value.Paused) { anyPlaying = true; break; }
        }
        if (anyPlaying)
            EnsureAudioRunning();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API (call from main thread / AGEBasic coroutine)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Load a SID file from disk. Synchronous – call from init code, not the hot loop.</summary>
    public void Load(string name, string path)
    {
        if (!File.Exists(path))
            throw new Exception($"SID file not found: {path}");

        SIDFile file = SIDFile.Load(path);
        if (!file.IsValid)
            throw new Exception($"Invalid or unrecognised SID file: {path}");

        CreateInstance(name, file);
    }

    /// <summary>Load a SID from raw bytes stored in a DATA list.</summary>
    public void LoadFromBytes(string name, byte[] bytes)
    {
        SIDFile file = SIDFile.FromBytes(bytes);
        if (!file.IsValid)
            throw new Exception($"Invalid SID data for '{name}'");

        CreateInstance(name, file);
    }

    /// <summary>Start playback. song is 1-based; 0 uses the file's default song.</summary>
    public void Play(string name, int song = 0)
    {
        if (!instances.TryGetValue(name, out var inst))
            throw new Exception($"SID '{name}' is not loaded");

        lock (inst.Lock)
        {
            int s = song > 0 ? song : inst.File.DefaultSong;
            s = Mathf.Clamp(s, 1, inst.File.SongCount);

            if (!inst.Playing || inst.CurrentSong != s)
            {
                inst.Chip.Reset();
                inst.Cpu.LoadProgram(inst.File.Data, inst.File.ActualLoadAddress);
                inst.Cpu.Init(inst.File.InitAddress, s);
                inst.CurrentSong     = s;
                inst.SamplesUntilPlay = 0;
            }

            inst.Playing = true;
            inst.Paused  = false;
        }

        EnsureAudioRunning();
    }

    public void Stop(string name)
    {
        if (instances.TryGetValue(name, out var inst))
            lock (inst.Lock) { inst.Playing = false; inst.Paused = false; }
    }

    public void Pause(string name)
    {
        if (instances.TryGetValue(name, out var inst))
            lock (inst.Lock) { inst.Paused = true; }
    }

    public void Resume(string name)
    {
        if (instances.TryGetValue(name, out var inst))
        {
            lock (inst.Lock) { inst.Paused = false; }
            EnsureAudioRunning();
        }
    }

    public void Unload(string name)
    {
        lock (instances)
        {
            if (instances.TryGetValue(name, out var inst))
                lock (inst.Lock) { inst.Playing = false; }
            instances.Remove(name);
        }
    }

    /// <param name="volume">0 – 100</param>
    public void SetVolume(string name, float volume)
    {
        if (instances.TryGetValue(name, out var inst))
            lock (inst.Lock) { inst.Volume = Mathf.Clamp01(volume); }
    }

    /// <returns>0 = not loaded · 1 = ready · 2 = playing · 3 = paused</returns>
    public int GetStatus(string name)
        => instances.TryGetValue(name, out var inst) ? inst.Status : 0;

    public string GetTitle(string name)
        => instances.TryGetValue(name, out var inst) ? inst.File.Title : "";

    public string GetAuthor(string name)
        => instances.TryGetValue(name, out var inst) ? inst.File.Author : "";

    public string GetReleased(string name)
        => instances.TryGetValue(name, out var inst) ? inst.File.Released : "";

    public int GetSongCount(string name)
        => instances.TryGetValue(name, out var inst) ? inst.File.SongCount : 0;

    public int GetDefaultSong(string name)
        => instances.TryGetValue(name, out var inst) ? inst.File.DefaultSong : 0;

    // ──────────────────────────────────────────────────────────────────────────
    // Audio DSP callback – runs on Unity's audio thread
    // ──────────────────────────────────────────────────────────────────────────

    void OnAudioFilterRead(float[] data, int channels)
    {
        try
        {
            float samplesPerPlay = sampleRate / PAL_PLAY_HZ;
            int   frames         = data.Length / channels;

            // Snapshot playing instances without holding the lock during heavy work
            List<SIDInstance> playing = null;
            lock (instances)
            {
                foreach (var kvp in instances)
                {
                    var inst = kvp.Value;
                    if (inst.Playing && !inst.Paused)
                    {
                        if (playing == null) playing = new List<SIDInstance>(4);
                        playing.Add(inst);
                    }
                }
            }

            if (playing == null) return;

            for (int frame = 0; frame < frames; frame++)
            {
                float mix = 0f;

                foreach (var inst in playing)
                {
                    lock (inst.Lock)
                    {
                        if (!inst.Playing || inst.Paused) continue;

                        // Trigger the SID play routine at ~50 Hz
                        inst.SamplesUntilPlay -= 1f;
                        if (inst.SamplesUntilPlay <= 0f)
                        {
                            // PlayAddress == 0 means the init routine installed an IRQ handler.
                            // Read the C64 user IRQ vector ($0314-$0315) it wrote during Init().
                            int playAddr = inst.File.PlayAddress;
                            if (playAddr == 0)
                                playAddr = inst.Cpu.Memory[0x0314] | (inst.Cpu.Memory[0x0315] << 8);
                            inst.Cpu.Play(playAddr);
                            inst.SamplesUntilPlay += samplesPerPlay;
                        }

                        mix += inst.Chip.GetSample() * inst.Volume;
                    }
                }

                // Add SID audio on top of any existing audio in all channels
                for (int ch = 0; ch < channels; ch++)
                    data[frame * channels + ch] += mix;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[SIDPlayer] OnAudioFilterRead: {e.Message}");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Internals
    // ──────────────────────────────────────────────────────────────────────────

    private void CreateInstance(string name, SIDFile file)
    {
        lock (instances)
        {
            if (instances.ContainsKey(name))
            {
                lock (instances[name].Lock) instances[name].Playing = false;
                instances.Remove(name);
            }

            var chip = new SIDChip(sampleRate);
            var cpu  = new SID6502(chip);
            cpu.LoadProgram(file.Data, file.ActualLoadAddress);

            instances[name] = new SIDInstance
            {
                File = file,
                Cpu  = cpu,
                Chip = chip,
            };
        }

        EnsureAudioRunning();
    }

    /// <summary>
    /// Guarantees OnAudioFilterRead fires by ensuring the AudioSource is in a playing state.
    /// If no clip is assigned, installs a 1-second silent looping clip.
    /// </summary>
    private void EnsureAudioRunning()
    {
        if (audioSource == null || audioSource.isPlaying) return;

        // Re-apply every time: Meta XR spatializer can override these after Awake()
        audioSource.spatialize        = false;
        audioSource.bypassReverbZones = true;
        audioSource.reverbZoneMix     = 0f;
        audioSource.spatialBlend      = 0f;

        if (audioSource.clip == null)
        {
            int sr  = AudioSettings.outputSampleRate;
            silentClip       = AudioClip.Create("_SIDActive", sr, 1, sr, false);
            silentClip.SetData(new float[sr], 0);
            audioSource.clip = silentClip;
            audioSource.loop = true;
        }

        audioSource.Play();
    }

    void OnDestroy()
    {
        lock (instances)
        {
            foreach (var kvp in instances)
                lock (kvp.Value.Lock) { kvp.Value.Playing = false; }
            instances.Clear();
        }

        if (silentClip != null)
        {
            if (audioSource != null) audioSource.Stop();
            Destroy(silentClip);
            silentClip = null;
        }
    }
}
