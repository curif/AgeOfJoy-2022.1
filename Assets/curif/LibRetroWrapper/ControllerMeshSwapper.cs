using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMeshSwapper : MonoBehaviour
{
    public GameObject Q2Mesh;
    public GameObject Q3Mesh;

    void Start()
    {
        ConfigManager.WriteConsole("[ControllerMeshSwapper.Start]");
        Q2Mesh.SetActive(!DeviceController.IsQ3);
        Q3Mesh.SetActive(DeviceController.IsQ3);
    }
}
