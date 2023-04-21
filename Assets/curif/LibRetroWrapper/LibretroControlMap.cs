using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class LibretroControlMap : MonoBehaviour
{
  public InputActionMap actionMap;

  // Start is called before the first frame update
  void Start()
  {
    DefaultControlMap conf = DefaultControlMap.Instance;

    ConfigManager.WriteConsole($"[LibretroControlMap] load default config {conf}");
    //ConfigManager.WriteConsole(conf.asMarkdown());
    Debug.Log(conf.asMarkdown());

    actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);
  }

  // Update is called once per frame
  void Update()
  {
  }
}
