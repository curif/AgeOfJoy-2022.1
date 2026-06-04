/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[Serializable]
public class MRVector3
{
    public float X;
    public float Y;
    public float Z;

    public Vector3 ToVector3() => new Vector3(X, Y, Z);

    public static MRVector3 From(Vector3 v) => new MRVector3 { X = v.x, Y = v.y, Z = v.z };
}

[Serializable]
public class MRQuaternion
{
    public float X;
    public float Y;
    public float Z;
    public float W = 1f;

    public Quaternion ToQuaternion() => new Quaternion(X, Y, Z, W);

    public static MRQuaternion From(Quaternion q) => new MRQuaternion { X = q.x, Y = q.y, Z = q.z, W = q.w };
}

[Serializable]
public enum PlacementSurfaceType
{
    Floor = 0,
    Wall = 1,
    Ceiling = 2,
    Free3D = 3
}

[Serializable]
public class MRCabinetPlacement
{
    public string Id;
    public string CabinetDBName;
    public string Rom;
    public MRVector3 Position;
    public MRQuaternion Rotation;
    public float Scale = 1f;
    /// <summary>OVRAnchor UUID when Position/Rotation are local to that MRUK anchor (layout v3+).</summary>
    public string AnchorUuid;
    /// <summary>Last known world pose — used when anchor UUID cannot be resolved on MR re-entry.</summary>
    public MRVector3 WorldPosition;
    public MRQuaternion WorldRotation;
    public PlacementSurfaceType SurfaceType = PlacementSurfaceType.Floor;
    /// <summary>Local axis that points toward the viewer when placed (NegativeZ = Unity default mesh with Z as back).</summary>
    public PlacementFacingAxis FacingAxis = PlacementFacingAxis.PositiveZ;

    public string DisplayLabel =>
        string.IsNullOrEmpty(CabinetDBName) ? Id : CabinetDBName;
}

[Serializable]
public class MRLayout
{
    public int Version = 3;
    public List<MRCabinetPlacement> Cabinets = new();

    readonly object cabinetsLock = new object();
    bool dirty;

    public IReadOnlyList<MRCabinetPlacement> GetCabinets()
    {
        lock (cabinetsLock)
            return Cabinets.AsReadOnly();
    }

    public MRCabinetPlacement FindById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        lock (cabinetsLock)
        {
            foreach (MRCabinetPlacement placement in Cabinets)
            {
                if (placement != null && placement.Id == id)
                    return placement;
            }
        }

        return null;
    }

    public bool RemoveById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        lock (cabinetsLock)
        {
            for (int i = Cabinets.Count - 1; i >= 0; i--)
            {
                if (Cabinets[i]?.Id == id)
                {
                    Cabinets.RemoveAt(i);
                    dirty = true;
                    return true;
                }
            }
        }

        return false;
    }

    public MRCabinetPlacement FindByCabinetDBName(string cabinetDBName)
    {
        if (string.IsNullOrEmpty(cabinetDBName))
            return null;

        lock (cabinetsLock)
        {
            foreach (MRCabinetPlacement placement in Cabinets)
            {
                if (placement != null
                    && string.Equals(placement.CabinetDBName, cabinetDBName, StringComparison.OrdinalIgnoreCase))
                    return placement;
            }
        }

        return null;
    }

    public MRCabinetPlacement AddPlacement(MRCabinetPlacement placement)
    {
        if (placement == null || string.IsNullOrEmpty(placement.CabinetDBName))
            return null;

        lock (cabinetsLock)
        {
            Cabinets.Add(placement);
            dirty = true;
        }

        return placement;
    }

    public void MarkSaved() => dirty = false;

    public bool NeedsSave() => dirty;

    public static MRLayout LoadOrCreate(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var empty = new MRLayout();
            empty.Save(filePath);
            return empty;
        }

        return LoadFromYaml(filePath);
    }

    public static MRLayout LoadFromYaml(string filePath)
    {
        ConfigManager.WriteConsole($"[MRLayout] loading {filePath}");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        string yaml = File.ReadAllText(filePath);
        MRLayout layout = deserializer.Deserialize<MRLayout>(yaml);
        if (layout == null)
            layout = new MRLayout();
        if (layout.Cabinets == null)
            layout.Cabinets = new List<MRCabinetPlacement>();
        layout.MarkSaved();
        return layout;
    }

    public void Save(string filePath)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml;
        lock (cabinetsLock)
            yaml = serializer.Serialize(this);

        File.WriteAllText(filePath, yaml);
        MarkSaved();
        ConfigManager.WriteConsole($"[MRLayout] saved {filePath} ({Cabinets.Count} cabinets)");
    }
}

[Serializable]
public class MREnvironmentPlacement
{
    public string Id;
    public string PrefabName;
    public MRVector3 Position;
    public MRQuaternion Rotation;
    public float Scale = 1f;
    public string AnchorUuid;
    public MRVector3 WorldPosition;
    public MRQuaternion WorldRotation;
    public PlacementSurfaceType SurfaceType = PlacementSurfaceType.Floor;
    public PlacementFacingAxis FacingAxis = PlacementFacingAxis.PositiveZ;

    public string DisplayLabel
    {
        get
        {
            if (!string.IsNullOrEmpty(PrefabName))
                return PrefabName;
            return string.IsNullOrEmpty(Id) ? "(prop)" : Id;
        }
    }
}

[Serializable]
public class MREnvironmentLayout
{
    public int Version = 1;
    public List<MREnvironmentPlacement> Props = new();

    readonly object propsLock = new object();

    public IReadOnlyList<MREnvironmentPlacement> GetProps()
    {
        lock (propsLock)
            return Props.AsReadOnly();
    }

    public MREnvironmentPlacement FindById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        lock (propsLock)
        {
            foreach (MREnvironmentPlacement placement in Props)
            {
                if (placement != null && placement.Id == id)
                    return placement;
            }
        }

        return null;
    }

    public MREnvironmentPlacement FindByPrefabName(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return null;

        lock (propsLock)
        {
            foreach (MREnvironmentPlacement placement in Props)
            {
                if (placement != null
                    && string.Equals(placement.PrefabName, prefabName, StringComparison.OrdinalIgnoreCase))
                    return placement;
            }
        }

        return null;
    }

    public bool RemoveById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        lock (propsLock)
        {
            for (int i = Props.Count - 1; i >= 0; i--)
            {
                if (Props[i]?.Id == id)
                {
                    Props.RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }

    public MREnvironmentPlacement AddPlacement(MREnvironmentPlacement placement)
    {
        if (placement == null || string.IsNullOrEmpty(placement.PrefabName))
            return null;

        lock (propsLock)
            Props.Add(placement);

        return placement;
    }

    public static MREnvironmentLayout LoadOrCreate(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var empty = new MREnvironmentLayout();
            empty.Save(filePath);
            return empty;
        }

        return LoadFromYaml(filePath);
    }

    public static MREnvironmentLayout LoadFromYaml(string filePath)
    {
        ConfigManager.WriteConsole($"[MREnvironmentLayout] loading {filePath}");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        string yaml = File.ReadAllText(filePath);
        MREnvironmentLayout layout = deserializer.Deserialize<MREnvironmentLayout>(yaml);
        if (layout == null)
            layout = new MREnvironmentLayout();
        if (layout.Props == null)
            layout.Props = new List<MREnvironmentPlacement>();
        return layout;
    }

    public void Save(string filePath)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml;
        lock (propsLock)
            yaml = serializer.Serialize(this);

        File.WriteAllText(filePath, yaml);
        ConfigManager.WriteConsole($"[MREnvironmentLayout] saved {filePath} ({Props.Count} props)");
    }
}
