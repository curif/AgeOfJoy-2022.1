#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;
using System.Collections;

public class FileCopier : MonoBehaviour
{
    public string sourceFilePath;
    public string destinationFilePath;
    // public SceneReference[] ScenesToLoad;
    public Vector3 playerPosition;

    IEnumerator Start()
    {
        GameObject player = GameObject.Find("OVRPlayerControllerGalery");

        // Check if source file exists
        if (!File.Exists(sourceFilePath))
        {
            ConfigManager.WriteConsoleError($"Source file does not exist! {sourceFilePath} -> {destinationFilePath}");
            yield break;
        }

        // Copy the file
        try
        {
            File.Copy(sourceFilePath, destinationFilePath, true);
            ConfigManager.WriteConsole("[FileCopier] File copied successfully!");
        }
        catch (System.Exception ex)
        {
            throw new Exception("Error copying file: " + ex.Message);
        }

        // foreach(SceneReference scene in ScenesToLoad)
        // {
        //     AsyncOperation ascn = SceneManager.LoadSceneAsync(scene.Name, LoadSceneMode.Additive);
        //     while (!ascn.isDone) {
        //         yield return null;
        //     }
        // }
        if (playerPosition.x != 0 && playerPosition.z != 0)
            player.transform.position = playerPosition;
        
        yield break;
    }
}
#endif

