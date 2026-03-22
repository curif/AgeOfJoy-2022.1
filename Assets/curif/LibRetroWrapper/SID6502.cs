using System;

/// <summary>
/// Minimal 6502 CPU emulator for SID music playback.
/// SID registers at $D400-$D41C are forwarded to SIDChip on write.
/// </summary>
public class SID6502
{
    public byte A, X, Y, S;
    public ushort PC;
    public byte P;

    const byte FLAG_C = 0x01;
    const byte FLAG_Z = 0x02;
    const byte FLAG_I = 0x04;
    const byte FLAG_D = 0x08;
    const byte FLAG_B = 0x10;
    const byte FLAG_U = 0x20;
    const byte FLAG_V = 0x40;
    const byte FLAG_N = 0x80;

    public byte[] Memory = new byte[65536];
    private SIDChip sidChip;

    public SID6502(SIDChip chip)
    {
        sidChip = chip;
        S = 0xFF;
        P = FLAG_U | FLAG_I;
    }

    public void LoadProgram(byte[] data, int loadAddress)
    {
        int len = Math.Min(data.Length, 65536 - loadAddress);
        Array.Copy(data, 0, Memory, loadAddress, len);
    }

    /// <summary>Call the SID init routine for the given song (1-based).</summary>
    public void Init(int initAddress, int song)
    {
        S = 0xFF;
        A = (byte)(song - 1);
        X = 0;
        Y = 0;
        P = FLAG_U | FLAG_I;
        PushWord(0xFFFE);   // fake JSR: return to $FFFF
        PC = (ushort)initAddress;
        int safety = 200000;
        while (PC != 0xFFFF && safety-- > 0)
            Step();
    }

    /// <summary>Execute the SID play routine (called at ~50 Hz).</summary>
    public void Play(int playAddress)
    {
        if (playAddress == 0) return;

        // Save stack pointer before pushing sentinel so we can restore it afterward.
        // Many SID tunes call Kernal ROM addresses (e.g. $FFD2). Our RAM there is 0x00 = BRK,
        // which sets PC=0xFFFF and exits the loop — but the JSR return address is never popped.
        // Restoring S prevents that leak from accumulating across 50 Hz calls and corrupting
        // the stack page, which would eventually silence all voices.
        byte savedS = S;
        PushWord(0xFFFE);
        PC = (ushort)playAddress;
        int safety = 50000;
        while (PC != 0xFFFF && safety-- > 0)
            Step();
        S = savedS; // restore: cancels sentinel + any leaked JSR frames
    }

    private void PushWord(int value) { Push((byte)(value >> 8)); Push((byte)(value & 0xFF)); }
    private void Push(byte v) { Memory[0x0100 + S] = v; S--; }
    private byte Pop() { S++; return Memory[0x0100 + S]; }
    private ushort PopWord() { byte lo = Pop(); byte hi = Pop(); return (ushort)((hi << 8) | lo); }

    private byte Read(int addr) => Memory[addr & 0xFFFF];
    private void Write(int addr, byte val)
    {
        addr &= 0xFFFF;
        Memory[addr] = val;
        if (addr >= 0xD400 && addr <= 0xD41C)
            sidChip.WriteRegister(addr - 0xD400, val);
    }

    private void SetNZ(byte v) { SetF(FLAG_N, (v & 0x80) != 0); SetF(FLAG_Z, v == 0); }
    private void SetF(byte f, bool c) { if (c) P |= f; else P &= (byte)~f; }
    private bool GetF(byte f) => (P & f) != 0;

    private byte Fetch() => Read(PC++);
    private ushort FetchWord() { byte lo = Fetch(); byte hi = Fetch(); return (ushort)((hi << 8) | lo); }

    private int Zp()  => Fetch();
    private int ZpX() => (Fetch() + X) & 0xFF;
    private int ZpY() => (Fetch() + Y) & 0xFF;
    private int Abs() => FetchWord();
    private int AbsX() => (FetchWord() + X) & 0xFFFF;
    private int AbsY() => (FetchWord() + Y) & 0xFFFF;
    private int IndX() { int z = (Fetch() + X) & 0xFF; return Read(z) | (Read((z + 1) & 0xFF) << 8); }
    private int IndY() { int z = Fetch(); int b = Read(z) | (Read((z + 1) & 0xFF) << 8); return (b + Y) & 0xFFFF; }
    private int Ind()  { int a = FetchWord(); return Read(a) | (Read((a & 0xFF00) | ((a + 1) & 0xFF)) << 8); }

    public void Step()
    {
        byte op = Fetch();
        int addr; byte val;

        switch (op)
        {
            // LDA
            case 0xA9: A = Fetch(); SetNZ(A); break;
            case 0xA5: A = Read(Zp()); SetNZ(A); break;
            case 0xB5: A = Read(ZpX()); SetNZ(A); break;
            case 0xAD: A = Read(Abs()); SetNZ(A); break;
            case 0xBD: A = Read(AbsX()); SetNZ(A); break;
            case 0xB9: A = Read(AbsY()); SetNZ(A); break;
            case 0xA1: A = Read(IndX()); SetNZ(A); break;
            case 0xB1: A = Read(IndY()); SetNZ(A); break;
            // LDX
            case 0xA2: X = Fetch(); SetNZ(X); break;
            case 0xA6: X = Read(Zp()); SetNZ(X); break;
            case 0xB6: X = Read(ZpY()); SetNZ(X); break;
            case 0xAE: X = Read(Abs()); SetNZ(X); break;
            case 0xBE: X = Read(AbsY()); SetNZ(X); break;
            // LDY
            case 0xA0: Y = Fetch(); SetNZ(Y); break;
            case 0xA4: Y = Read(Zp()); SetNZ(Y); break;
            case 0xB4: Y = Read(ZpX()); SetNZ(Y); break;
            case 0xAC: Y = Read(Abs()); SetNZ(Y); break;
            case 0xBC: Y = Read(AbsX()); SetNZ(Y); break;
            // STA
            case 0x85: Write(Zp(), A); break;
            case 0x95: Write(ZpX(), A); break;
            case 0x8D: Write(Abs(), A); break;
            case 0x9D: Write(AbsX(), A); break;
            case 0x99: Write(AbsY(), A); break;
            case 0x81: Write(IndX(), A); break;
            case 0x91: Write(IndY(), A); break;
            // STX
            case 0x86: Write(Zp(), X); break;
            case 0x96: Write(ZpY(), X); break;
            case 0x8E: Write(Abs(), X); break;
            // STY
            case 0x84: Write(Zp(), Y); break;
            case 0x94: Write(ZpX(), Y); break;
            case 0x8C: Write(Abs(), Y); break;
            // ADC
            case 0x69: ADC(Fetch()); break;
            case 0x65: ADC(Read(Zp())); break;
            case 0x75: ADC(Read(ZpX())); break;
            case 0x6D: ADC(Read(Abs())); break;
            case 0x7D: ADC(Read(AbsX())); break;
            case 0x79: ADC(Read(AbsY())); break;
            case 0x61: ADC(Read(IndX())); break;
            case 0x71: ADC(Read(IndY())); break;
            // SBC
            case 0xE9: SBC(Fetch()); break;
            case 0xE5: SBC(Read(Zp())); break;
            case 0xF5: SBC(Read(ZpX())); break;
            case 0xED: SBC(Read(Abs())); break;
            case 0xFD: SBC(Read(AbsX())); break;
            case 0xF9: SBC(Read(AbsY())); break;
            case 0xE1: SBC(Read(IndX())); break;
            case 0xF1: SBC(Read(IndY())); break;
            // AND
            case 0x29: A &= Fetch(); SetNZ(A); break;
            case 0x25: A &= Read(Zp()); SetNZ(A); break;
            case 0x35: A &= Read(ZpX()); SetNZ(A); break;
            case 0x2D: A &= Read(Abs()); SetNZ(A); break;
            case 0x3D: A &= Read(AbsX()); SetNZ(A); break;
            case 0x39: A &= Read(AbsY()); SetNZ(A); break;
            case 0x21: A &= Read(IndX()); SetNZ(A); break;
            case 0x31: A &= Read(IndY()); SetNZ(A); break;
            // ORA
            case 0x09: A |= Fetch(); SetNZ(A); break;
            case 0x05: A |= Read(Zp()); SetNZ(A); break;
            case 0x15: A |= Read(ZpX()); SetNZ(A); break;
            case 0x0D: A |= Read(Abs()); SetNZ(A); break;
            case 0x1D: A |= Read(AbsX()); SetNZ(A); break;
            case 0x19: A |= Read(AbsY()); SetNZ(A); break;
            case 0x01: A |= Read(IndX()); SetNZ(A); break;
            case 0x11: A |= Read(IndY()); SetNZ(A); break;
            // EOR
            case 0x49: A ^= Fetch(); SetNZ(A); break;
            case 0x45: A ^= Read(Zp()); SetNZ(A); break;
            case 0x55: A ^= Read(ZpX()); SetNZ(A); break;
            case 0x4D: A ^= Read(Abs()); SetNZ(A); break;
            case 0x5D: A ^= Read(AbsX()); SetNZ(A); break;
            case 0x59: A ^= Read(AbsY()); SetNZ(A); break;
            case 0x41: A ^= Read(IndX()); SetNZ(A); break;
            case 0x51: A ^= Read(IndY()); SetNZ(A); break;
            // INC/DEC memory
            case 0xE6: addr = Zp();   Write(addr, INC(Read(addr))); break;
            case 0xF6: addr = ZpX();  Write(addr, INC(Read(addr))); break;
            case 0xEE: addr = Abs();  Write(addr, INC(Read(addr))); break;
            case 0xFE: addr = AbsX(); Write(addr, INC(Read(addr))); break;
            case 0xC6: addr = Zp();   Write(addr, DEC(Read(addr))); break;
            case 0xD6: addr = ZpX();  Write(addr, DEC(Read(addr))); break;
            case 0xCE: addr = Abs();  Write(addr, DEC(Read(addr))); break;
            case 0xDE: addr = AbsX(); Write(addr, DEC(Read(addr))); break;
            case 0xE8: X++; SetNZ(X); break;
            case 0xC8: Y++; SetNZ(Y); break;
            case 0xCA: X--; SetNZ(X); break;
            case 0x88: Y--; SetNZ(Y); break;
            // ASL
            case 0x0A: SetF(FLAG_C, (A & 0x80) != 0); A <<= 1; SetNZ(A); break;
            case 0x06: addr = Zp();   val = ASL(Read(addr)); Write(addr, val); break;
            case 0x16: addr = ZpX();  val = ASL(Read(addr)); Write(addr, val); break;
            case 0x0E: addr = Abs();  val = ASL(Read(addr)); Write(addr, val); break;
            case 0x1E: addr = AbsX(); val = ASL(Read(addr)); Write(addr, val); break;
            // LSR
            case 0x4A: SetF(FLAG_C, (A & 0x01) != 0); A >>= 1; SetNZ(A); break;
            case 0x46: addr = Zp();   val = LSR(Read(addr)); Write(addr, val); break;
            case 0x56: addr = ZpX();  val = LSR(Read(addr)); Write(addr, val); break;
            case 0x4E: addr = Abs();  val = LSR(Read(addr)); Write(addr, val); break;
            case 0x5E: addr = AbsX(); val = LSR(Read(addr)); Write(addr, val); break;
            // ROL
            case 0x2A: { byte c = (byte)(GetF(FLAG_C) ? 1 : 0); SetF(FLAG_C, (A & 0x80) != 0); A = (byte)((A << 1) | c); SetNZ(A); } break;
            case 0x26: addr = Zp();   val = ROL(Read(addr)); Write(addr, val); break;
            case 0x36: addr = ZpX();  val = ROL(Read(addr)); Write(addr, val); break;
            case 0x2E: addr = Abs();  val = ROL(Read(addr)); Write(addr, val); break;
            case 0x3E: addr = AbsX(); val = ROL(Read(addr)); Write(addr, val); break;
            // ROR
            case 0x6A: { byte c = (byte)(GetF(FLAG_C) ? 0x80 : 0); SetF(FLAG_C, (A & 0x01) != 0); A = (byte)((A >> 1) | c); SetNZ(A); } break;
            case 0x66: addr = Zp();   val = ROR(Read(addr)); Write(addr, val); break;
            case 0x76: addr = ZpX();  val = ROR(Read(addr)); Write(addr, val); break;
            case 0x6E: addr = Abs();  val = ROR(Read(addr)); Write(addr, val); break;
            case 0x7E: addr = AbsX(); val = ROR(Read(addr)); Write(addr, val); break;
            // CMP / CPX / CPY
            case 0xC9: CMP(A, Fetch()); break;
            case 0xC5: CMP(A, Read(Zp())); break;
            case 0xD5: CMP(A, Read(ZpX())); break;
            case 0xCD: CMP(A, Read(Abs())); break;
            case 0xDD: CMP(A, Read(AbsX())); break;
            case 0xD9: CMP(A, Read(AbsY())); break;
            case 0xC1: CMP(A, Read(IndX())); break;
            case 0xD1: CMP(A, Read(IndY())); break;
            case 0xE0: CMP(X, Fetch()); break;
            case 0xE4: CMP(X, Read(Zp())); break;
            case 0xEC: CMP(X, Read(Abs())); break;
            case 0xC0: CMP(Y, Fetch()); break;
            case 0xC4: CMP(Y, Read(Zp())); break;
            case 0xCC: CMP(Y, Read(Abs())); break;
            // BIT
            case 0x24: BIT(Read(Zp())); break;
            case 0x2C: BIT(Read(Abs())); break;
            // JMP / JSR / RTS / RTI
            case 0x4C: PC = (ushort)Abs(); break;
            case 0x6C: PC = (ushort)Ind(); break;
            case 0x20: { ushort t = FetchWord(); PushWord(PC - 1); PC = t; } break;
            case 0x60: PC = (ushort)(PopWord() + 1); break;
            case 0x40: P = (byte)((Pop() & ~FLAG_B) | FLAG_U); PC = PopWord(); break;
            // Branches
            case 0x90: Branch(!GetF(FLAG_C)); break;
            case 0xB0: Branch(GetF(FLAG_C)); break;
            case 0xF0: Branch(GetF(FLAG_Z)); break;
            case 0xD0: Branch(!GetF(FLAG_Z)); break;
            case 0x30: Branch(GetF(FLAG_N)); break;
            case 0x10: Branch(!GetF(FLAG_N)); break;
            case 0x70: Branch(GetF(FLAG_V)); break;
            case 0x50: Branch(!GetF(FLAG_V)); break;
            // Stack
            case 0x48: Push(A); break;
            case 0x68: A = Pop(); SetNZ(A); break;
            case 0x08: Push((byte)(P | FLAG_B | FLAG_U)); break;
            case 0x28: P = (byte)((Pop() & ~FLAG_B) | FLAG_U); break;
            // Transfer
            case 0xAA: X = A; SetNZ(X); break;
            case 0x8A: A = X; SetNZ(A); break;
            case 0xA8: Y = A; SetNZ(Y); break;
            case 0x98: A = Y; SetNZ(A); break;
            case 0xBA: X = S; SetNZ(X); break;
            case 0x9A: S = X; break;
            // Flags
            case 0x18: SetF(FLAG_C, false); break;
            case 0x38: SetF(FLAG_C, true); break;
            case 0x58: SetF(FLAG_I, false); break;
            case 0x78: SetF(FLAG_I, true); break;
            case 0xB8: SetF(FLAG_V, false); break;
            case 0xD8: SetF(FLAG_D, false); break;
            case 0xF8: SetF(FLAG_D, true); break;
            // NOP
            case 0xEA: break;
            // BRK – treat as end-of-routine sentinel
            case 0x00: PC = 0xFFFF; break;
            // Unofficial NOPs (common in SID player stubs)
            case 0x1A: case 0x3A: case 0x5A: case 0x7A: case 0xDA: case 0xFA: break;
            case 0x80: case 0x82: case 0x89: case 0xC2: case 0xE2: Fetch(); break;
            case 0x04: case 0x44: case 0x64: Zp(); break;
            case 0x14: case 0x34: case 0x54: case 0x74: case 0xD4: case 0xF4: ZpX(); break;
            case 0x0C: Abs(); break;
            case 0x1C: case 0x3C: case 0x5C: case 0x7C: case 0xDC: case 0xFC: AbsX(); break;
            default: break; // unknown = NOP
        }
    }

    private void ADC(byte v)
    {
        int c = GetF(FLAG_C) ? 1 : 0;
        int r = A + v + c;
        SetF(FLAG_V, (~(A ^ v) & (A ^ r) & 0x80) != 0);
        SetF(FLAG_C, r > 0xFF);
        A = (byte)r;
        SetNZ(A);
    }
    private void SBC(byte v) => ADC((byte)~v);

    private byte INC(byte v) { v++; SetNZ(v); return v; }
    private byte DEC(byte v) { v--; SetNZ(v); return v; }
    private byte ASL(byte v) { SetF(FLAG_C, (v & 0x80) != 0); v <<= 1; SetNZ(v); return v; }
    private byte LSR(byte v) { SetF(FLAG_C, (v & 0x01) != 0); v >>= 1; SetNZ(v); return v; }
    private byte ROL(byte v) { byte c = (byte)(GetF(FLAG_C) ? 1 : 0); SetF(FLAG_C, (v & 0x80) != 0); v = (byte)((v << 1) | c); SetNZ(v); return v; }
    private byte ROR(byte v) { byte c = (byte)(GetF(FLAG_C) ? 0x80 : 0); SetF(FLAG_C, (v & 0x01) != 0); v = (byte)((v >> 1) | c); SetNZ(v); return v; }

    private void CMP(byte reg, byte v)
    {
        int r = reg - v;
        SetF(FLAG_C, reg >= v);
        SetF(FLAG_Z, reg == v);
        SetF(FLAG_N, (r & 0x80) != 0);
    }
    private void BIT(byte v) { SetF(FLAG_N, (v & 0x80) != 0); SetF(FLAG_V, (v & 0x40) != 0); SetF(FLAG_Z, (A & v) == 0); }
    private void Branch(bool cond) { sbyte off = (sbyte)Fetch(); if (cond) PC = (ushort)(PC + off); }
}
