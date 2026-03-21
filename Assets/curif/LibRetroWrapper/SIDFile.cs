using System;
using System.IO;
using System.Text;

/// <summary>
/// Parses PSID/RSID v1-v4 file headers. Exposes metadata and raw 6502 code bytes.
/// Format: https://www.hvsc.c64.org/download/C64Music/DOCUMENTS/SID_file_format.txt
/// </summary>
public class SIDFile
{
    public bool IsValid { get; private set; }
    public string Magic { get; private set; }
    public int Version { get; private set; }
    public int DataOffset { get; private set; }
    public int LoadAddress { get; private set; }
    public int InitAddress { get; private set; }
    public int PlayAddress { get; private set; }
    public int SongCount { get; private set; }
    public int DefaultSong { get; private set; }   // 1-based
    public string Title { get; private set; }
    public string Author { get; private set; }
    public string Released { get; private set; }
    public byte[] Data { get; private set; }        // raw 6502 machine code
    public int ActualLoadAddress { get; private set; }

    public static SIDFile Load(string path)
    {
        return Parse(File.ReadAllBytes(path));
    }

    public static SIDFile FromBytes(byte[] bytes)
    {
        return Parse(bytes);
    }

    private static SIDFile Parse(byte[] raw)
    {
        var sid = new SIDFile();
        if (raw == null || raw.Length < 0x76)
        {
            sid.IsValid = false;
            return sid;
        }

        sid.Magic = Encoding.ASCII.GetString(raw, 0, 4);
        if (sid.Magic != "PSID" && sid.Magic != "RSID")
        {
            sid.IsValid = false;
            return sid;
        }

        sid.Version = ReadBE16(raw, 4);
        sid.DataOffset = ReadBE16(raw, 6);
        sid.LoadAddress = ReadBE16(raw, 8);
        sid.InitAddress = ReadBE16(raw, 10);
        sid.PlayAddress = ReadBE16(raw, 12);
        sid.SongCount = ReadBE16(raw, 14);
        sid.DefaultSong = ReadBE16(raw, 16);
        sid.Title = ReadNullString(raw, 22, 32);
        sid.Author = ReadNullString(raw, 54, 32);
        sid.Released = ReadNullString(raw, 86, 32);

        int dataStart = sid.DataOffset;
        int dataLen = raw.Length - dataStart;
        if (dataLen <= 0) { sid.IsValid = false; return sid; }

        sid.Data = new byte[dataLen];
        Array.Copy(raw, dataStart, sid.Data, 0, dataLen);

        if (sid.LoadAddress == 0)
        {
            if (dataLen < 2) { sid.IsValid = false; return sid; }
            sid.ActualLoadAddress = sid.Data[0] | (sid.Data[1] << 8);
            byte[] trimmed = new byte[dataLen - 2];
            Array.Copy(sid.Data, 2, trimmed, 0, dataLen - 2);
            sid.Data = trimmed;
        }
        else
        {
            sid.ActualLoadAddress = sid.LoadAddress;
        }

        sid.SongCount = Math.Max(1, sid.SongCount);
        sid.DefaultSong = Math.Max(1, sid.DefaultSong);
        sid.IsValid = true;
        return sid;
    }

    private static int ReadBE16(byte[] d, int o) => (d[o] << 8) | d[o + 1];

    private static string ReadNullString(byte[] d, int offset, int maxLen)
    {
        int end = offset;
        int limit = Math.Min(offset + maxLen, d.Length);
        while (end < limit && d[end] != 0) end++;
        return Encoding.ASCII.GetString(d, offset, end - offset).Trim();
    }
}
