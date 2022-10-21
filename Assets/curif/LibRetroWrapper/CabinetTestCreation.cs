using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class CabinetTestCreation : MonoBehaviour
{
    public string RomsPath;
    public string PathDest;
    public string rom;
    public bool invertx = false;
    public bool inverty = false;
    public string orientation = "vertical";

    public static int fileCounter = 0;

    public Camera cam;

    public int Width = 800, Height = 400;
    [SerializeField] TextMeshProUGUI txtMesh;

    void Save(Texture2D texture, string path)
    {
        var bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    // Take a "screenshot" of a camera's Render Texture.
    Texture2D RTImage()
    {
        Texture2D image = new Texture2D(Width, Height);
        RenderTexture rt = new RenderTexture(Width, Height, 16, RenderTextureFormat.ARGB32);

        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        // https://docs.unity3d.com/ScriptReference/RenderTexture-active.html
        var currentRt = RenderTexture.active;
        RenderTexture.active = rt;

        // Render the camera's view.
        // https://docs.unity3d.com/ScriptReference/Camera.Render.html
        cam.targetTexture = rt;
        cam.Render();

        // Make a new texture and read the active Render Texture into it.
        image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRt;
        cam.targetTexture = null;

        return image;
    }

    void CreateCabinet(string romWithPath)
    {
        string img;
        ConfigManager.WriteConsole($"Create cabinet for rom {romWithPath}");

        if (!File.Exists(romWithPath))
            throw new System.Exception($"{romWithPath} not found");

        string cabName = CabinetDBAdmin.CreateGenericForUnnasignedRom(romWithPath, PathDest, invertx: invertx,
            inverty: inverty, orientation: orientation);
        ConfigManager.WriteConsole($"Generated cabinet {cabName} from {romWithPath}");

        //create marquee
        img = $"{PathDest}/{cabName}/marquee.png";
        txtMesh.text = cabName;
        Texture2D t = RTImage();
        Save(t, img);
    }

    void OnGUI()
    {
        if (GUILayout.Button("Generate Cabinets"))
        {
            ConfigManager.WriteConsole($"Generate cabinets in dir {PathDest} from path {RomsPath} specific rom: {rom}");

            if (!Directory.Exists(PathDest))
                Directory.CreateDirectory(PathDest);

            if (!String.IsNullOrEmpty(rom))
            {
                CreateCabinet(Path.Combine(RomsPath, rom));
            }
            else
            {
                string[] roms = Directory.GetFiles(RomsPath, "*.zip");
                foreach (string romWithPath in roms)
                {
                    CreateCabinet(romWithPath);
                }
            }
        }
    }
}
