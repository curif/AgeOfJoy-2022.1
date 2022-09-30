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
    public static int fileCounter = 0;

    public Camera cam;

    public int width=800, height=400;
    [SerializeField] TextMeshProUGUI txtMesh;
    
    void Save(Texture2D texture, string path)
    {
        var bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    // Take a "screenshot" of a camera's Render Texture.
    Texture2D RTImage()
    {
        Texture2D image = new Texture2D(width, height);
        RenderTexture rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);

        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        // https://docs.unity3d.com/ScriptReference/RenderTexture-active.html
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        // Render the camera's view.
        // https://docs.unity3d.com/ScriptReference/Camera.Render.html
        cam.targetTexture = rt;
        cam.Render();

        // Make a new texture and read the active Render Texture into it.
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        cam.targetTexture = null;

        return image;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Generate Cabinets"))
        {
            string img;

            ConfigManager.WriteConsole($"Generate cabinets in dir {PathDest} from {RomsPath}");

            if (!Directory.Exists(PathDest))
                 Directory.CreateDirectory(PathDest);
            
            string[] roms = Directory.GetFiles(RomsPath, "*.zip");
            foreach (string rom in roms)
            {
                if (File.Exists(rom)) {
                    string cabName = CabinetDBAdmin.CreateGenericForUnnasignedRom(rom, PathDest);
                    //create marquee
                    img = $"{PathDest}/{cabName}/marquee.png";
                    txtMesh.text = cabName;
                    Texture2D t = RTImage();
                    Save(t, img);
                }
            }   
            
        }

    }

}
