using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CabinetAutoReload : MonoBehaviour
{
  string testFile = ConfigManager.Cabinets + "/test.zip";

  void Start()
  {
    StartCoroutine(reload());

  }

  IEnumerator reload()
  {
    if (!File.Exists(testFile))
      yield return new WaitForSeconds(1f);


    ConfigManager.WriteConsole($"New cabinet to test: {testFile}");

    //new cabinet to test
    CabinetInformation cbInfo = null;
    try
    {
      CabinetDBAdmin.loadCabinetFromZip(testFile);

      cbInfo = CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/test"); //description.yaml

      CabinetInformation.showCabinetProblems(cbInfo);

      //cabinet inseption
      ConfigManager.WriteConsole($"Deploy test cabinet {cbInfo.name}");
      Cabinet cab = CabinetFactory.fromInformation(cbInfo, gameObject.transform.position, gameObject.transform.rotation);
      Object.Destroy(gameObject);
      cab.gameObject.AddComponent(typeof(CabinetAutoReload));

      ConfigManager.WriteConsole("New Tested Cabinet deployed ******");
    }
    catch (System.Exception ex)
    {
      ConfigManager.WriteConsole($"ERROR loading cabinet from description {testFile}: {ex}");
    }

  }
}
