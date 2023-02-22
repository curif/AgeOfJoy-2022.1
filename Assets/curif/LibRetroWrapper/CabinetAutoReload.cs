/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CabinetAutoReload : MonoBehaviour
{
  [Tooltip("Positions where the player can stay to load the cabinet")]
  public List<GameObject> AgentPlayerPositions;

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

      ConfigManager.WriteConsole($"[CabinetAutoReload] new cabinet from yaml: {testCabinetDir}");

      cbInfo = CabinetInformation.fromYaml(testCabinetDir); //description.yaml
      if (cbInfo == null)
      {
        ConfigManager.WriteConsole($"[CabinetAutoReload] ERROR NULL cabinet - new cabinet from yaml: {testCabinetDir}");
        throw new IOException();
      }

      ConfigManager.WriteConsole($"[CabinetAutoReload] cabinet problems (if any):..."); 
      CabinetInformation.showCabinetProblems(cbInfo);

      //cabinet inseption
      ConfigManager.WriteConsole($"[CabinetAutoReload] Deploy test cabinet {cbInfo.name}");
      Cabinet cab = CabinetFactory.fromInformation(cbInfo, "workshop", 0, transform.position, transform.rotation, transform.parent, AgentPlayerPositions);
      CabinetFactory.skinFromInformation(cab, cbInfo);
      UnityEngine.Object.Destroy(gameObject);
      CabinetAutoReload cba = (CabinetAutoReload)cab.gameObject.AddComponent(typeof(CabinetAutoReload)); //this will excecute Start().
      cba.AgentPlayerPositions = AgentPlayerPositions;

      ConfigManager.WriteConsole("[CabinetAutoReload] New Tested Cabinet deployed ******");
    }
    catch (System.Exception ex)
    {
      ConfigManager.WriteConsole($"[CabinetAutoReload] ERROR loading cabinet from description {testDescriptionCabinetFile}: {ex}");
    }
  }
}
