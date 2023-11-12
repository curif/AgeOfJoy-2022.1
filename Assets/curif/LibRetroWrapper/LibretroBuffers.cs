
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Buffers2
{
    public byte[][] buffers = new byte[][]
    {
    new byte[] { },
    new byte[] { }
    };
    public object[] locked = new object[2] { new object(), new object() };
    public bool[] loaded = new bool[2] { false, false };
    public int toLoad = 0;

    public int next()
    {
        toLoad = (toLoad + 1) % 2;
        return toLoad;
    }
    public void createBuffers(int size)
    {
        if (buffers[0].Length == size)
            return;
        ConfigManager.WriteConsole($"[createBuffers] create both buffers {size} bytes");
        buffers[0] = new byte[size];
        buffers[1] = new byte[size];
    }
    public void load(IntPtr data, int size)
    {
        lock (locked[toLoad])
        {
            if (!loaded[toLoad])
            {
                ConfigManager.WriteConsole($"[load] size: {size} buffer: {toLoad}");
                createBuffers(size);
                Marshal.Copy(data, buffers[toLoad], 0, size);
                loaded[toLoad] = true;
                next();
            }
        }
    }
    public void texturize(Texture2D texture)
    {
        int toTexturize = (toLoad + 1) % 2;
        lock (locked[toTexturize])
        {
            if (loaded[toTexturize])
            {
                ConfigManager.WriteConsole($"[texturize] buffer: {toTexturize}");
                texture.LoadRawTextureData(buffers[toTexturize]);
                texture.Apply(false, false);
                loaded[toTexturize] = false;
            }
        }

    }
}


public class Buffers
{
    public byte[][] buffers = new byte[][]
    {
    new byte[] { },
    new byte[] { }
    };
    public byte[] buffer;

    public object locked = new object();
    public bool ready = false;
    public int toLoad = 0;

    public void createBuffers(int size)
    {
        if (buffers[0].Length == size)
            return;
        // ConfigManager.WriteConsole($"[createBuffers] create both buffers {size} bytes");
        buffers[0] = new byte[size];
        buffers[1] = new byte[size];
    }
    public void load(IntPtr data, int size)
    {
        createBuffers(size);
        Marshal.Copy(data, buffers[toLoad], 0, size);
        lock (locked)
        {
            if (!ready)
            {
                // ConfigManager.WriteConsole($"[load] size: {size} buffer: {toLoad}");
                buffer = buffers[toLoad];
                ready = true;
                toLoad = (toLoad + 1) % 2;
            }
        }
    }
    public void texturize(Texture2D texture)
    {
        lock (locked)
        {
            if (ready)
            {
                // ConfigManager.WriteConsole($"[texturize] buffer");
                texture.LoadRawTextureData(buffer);
                texture.Apply(false, false);
                ready = false;
            }
        }
    }
}