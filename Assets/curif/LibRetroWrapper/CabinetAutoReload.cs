using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class CabinetAutoReload : MonoBehaviour
{
  static string testCabinetDir = ConfigManager.CabinetsDB + "/test";
  static string testDescriptionCabinetFile = testCabinetDir + "/description.yaml";
  static string testFile = ConfigManager.Cabinets + "/test.zip";

  void Start()
  {
    //it's not possible to use filesystemwatcher

    ConfigManager.WriteConsole($"[CabinetAutoReload] start ");

    //this start() will be excecuted every time the component is loaded, do not excecute LoadCabinet() here.
    // LoadCabinet();

    StartCoroutine(reload());

  }

  IEnumerator reload()
  {
    while (true)
    {
      // ConfigManager.WriteConsole($"[CabinetAutoReload] test for file: {File.Exists(testFile)} {testFile}");
      if (File.Exists(testFile))
      {
        //also deletes the zip file
        ConfigManager.WriteConsole($"[CabinetAutoReload] load cabinet from {testFile}");
        CabinetDBAdmin.loadCabinetFromZip(testFile);
        LoadCabinet();
      }

      yield return new WaitForSeconds(2f);
    }
  }

  private void LoadCabinet()
  {

    if (!File.Exists(testDescriptionCabinetFile))
      return;

    ConfigManager.WriteConsole($"[CabinetAutoReload] New cabinet to test: {testDescriptionCabinetFile}");

    //new cabinet to test
    CabinetInformation cbInfo = null;
    try
    {

      cbInfo = CabinetInformation.fromYaml(testCabinetDir); //description.yaml

      CabinetInformation.showCabinetProblems(cbInfo);

      //cabinet inseption
      ConfigManager.WriteConsole($"[CabinetAutoReload] Deploy test cabinet {cbInfo.name}");
      Cabinet cab = CabinetFactory.fromInformation(cbInfo, "workshop", 0, transform.position, transform.rotation, transform.parent);
      UnityEngine.Object.Destroy(gameObject);
      cab.gameObject.AddComponent(typeof(CabinetAutoReload)); //this will excecute Start().

      ConfigManager.WriteConsole("[CabinetAutoReload] New Tested Cabinet deployed ******");
    }
    catch (System.Exception ex)
    {
      ConfigManager.WriteConsole($"[CabinetAutoReload] ERROR loading cabinet from description {testDescriptionCabinetFile}: {ex}");
    }
  }
}
