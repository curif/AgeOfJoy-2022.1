using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

/// <summary>
/// Parses a Pugsy's Mamecheat XML file and builds a lookup table mapping
/// cheat description → (region, offset, byteSize) for use with on-memory-change events.
/// Only cheats with a state="run" action that writes directly to a CPU memory address are included.
/// </summary>
public class CheatAddress
{
    public uint Region;    // LibRetro region: 0=SAVE_RAM, 2=SYSTEM_RAM (maincpu)
    public uint Offset;    // byte offset into the region
    public int ByteSize;   // 1=byte (pb/mb), 2=word (pw/mw), 4=dword (pd/md)
}

public static class MameCheatXmlParser
{
    // Matches the left-hand side of a cheat action: chip.size@hexoffset
    // e.g. "maincpu.pb@8880" or "maincpu.mw@B580"
    static readonly Regex AddressPattern = new Regex(
        @"^([A-Za-z0-9]+)\.(p[bwd]|m[bwd])@([0-9A-Fa-f]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses the XML file and returns a case-insensitive dictionary of
    /// cheat description → CheatAddress for all cheats that have a readable address.
    /// </summary>
    public static Dictionary<string, CheatAddress> Parse(string xmlPath)
    {
        var result = new Dictionary<string, CheatAddress>(StringComparer.OrdinalIgnoreCase);

        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);

        XmlNodeList cheats = doc.GetElementsByTagName("cheat");
        foreach (XmlNode cheat in cheats)
        {
            string desc = cheat.Attributes?["desc"]?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(desc))
                continue;

            CheatAddress addr = ExtractFirstAddress(cheat);
            if (addr != null)
                result[desc] = addr;
        }

        return result;
    }

    static CheatAddress ExtractFirstAddress(XmlNode cheat)
    {
        foreach (XmlNode script in cheat.ChildNodes)
        {
            if (script.Name != "script")
                continue;
            if (script.Attributes?["state"]?.Value != "run")
                continue;

            foreach (XmlNode action in script.ChildNodes)
            {
                if (action.Name != "action")
                    continue;

                string text = action.InnerText?.Trim();
                if (string.IsNullOrEmpty(text))
                    continue;

                // Skip temp variable reads/writes
                if (text.StartsWith("temp", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extract left-hand side of the assignment
                int eqIdx = text.IndexOf('=');
                if (eqIdx <= 0)
                    continue;

                string lhs = text.Substring(0, eqIdx).Trim();
                Match m = AddressPattern.Match(lhs);
                if (!m.Success)
                    continue;

                return new CheatAddress
                {
                    Region = ChipToRegion(m.Groups[1].Value),
                    Offset = Convert.ToUInt32(m.Groups[3].Value, 16),
                    ByteSize = SizeToBytes(m.Groups[2].Value)
                };
            }
        }
        return null;
    }

    static uint ChipToRegion(string chip)
    {
        // All CPU chips (maincpu, soundcpu, etc.) map to SYSTEM_RAM in LibRetro
        return 2;
    }

    static int SizeToBytes(string size)
    {
        switch (size.ToLower())
        {
            case "pw":
            case "mw": return 2;
            case "pd":
            case "md": return 4;
            default:   return 1; // pb, mb
        }
    }
}
