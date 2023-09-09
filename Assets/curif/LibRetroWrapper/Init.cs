/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections;
using UnityEngine;
using System.IO;

public class Init
{

    // static string testedCabinetName = "TestedCabinet";

    //https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute-ctor.html
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        // bool isWorkshop = true; //look for a process to detect if this is a workshop or not.
        ConfigManager.WriteConsole("[Init.OnRuntimeMethodLoad] +++++++++++++++++++++  Initialize  +++++++++++++++++++++");
        ConfigManager.WriteConsole("[Init.OnRuntimeMethodLoad] Loading cabinets");
        CabinetDBAdmin.loadCabinets();
        /*
        ConfigManager.WriteConsole("[Init.OnRuntimeMethodLoad] Loading cores");
        string AppCoresPath = $"{Application.dataPath}/cores";
        if (!Directory.Exists(AppCoresPath))
        {
            ConfigManager.WriteConsole($"[Init.OnRuntimeMethodLoad] create cores dir: {AppCoresPath}");
            Directory.CreateDirectory(AppCoresPath);
        }
        string[] files = Directory.GetFiles(ConfigManager.Cores, "*.so");
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destinationPath = Path.Combine(AppCoresPath, fileName);

            // Copy the file to the destination directory
            File.Copy(file, destinationPath, true);
            File.SetAttributes(destinationPath, FileAttributes.Normal);
            File.Delete(file);
            ConfigManager.WriteConsole($"[Init.OnRuntimeMethodLoad]      Installed core {fileName}");
        }
        */
        /*
        string[] files = Directory.GetFiles(ConfigManager.Cores, "*.so");
        foreach (string libraryPath in files)
        {
            // Change the file permissions
            AndroidJavaClass fileClass = new AndroidJavaClass("java.io.File");
            AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", libraryPath);

            bool readable = fileObject.Call<bool>("setReadable", true, false);
            bool executable = fileObject.Call<bool>("setExecutable", true, false);

            ConfigManager.WriteConsole($"[Init.OnRuntimeMethodLoad]      Installed core {libraryPath} readable: {readable} executable: {executable}");

        }
        ConfigManager.WriteConsole($"[Init.OnRuntimeMethodLoad] END +++++++++++++++++");
*/
        /*
                GameObject[] cabinetSpots = GameObject.FindGameObjectsWithTag("spot");
                int cabinetFoundIndex = 0;
                ConfigManager.WriteConsole($"{cabinetSpots.Length} spots to fill in the workshop space.");
                if (cabinetSpots.Length == 0) {
                    ConfigManager.WriteConsole($"No cabinet spots present in the scene, do nothing.");
                    return;
                }

                ConfigManager.WriteConsole($"processing database: {ConfigManager.CabinetsDB}");
                string[] files = Directory.GetDirectories(ConfigManager.CabinetsDB);
                ConfigManager.WriteConsole($"{files.Length} directories found in database {ConfigManager.CabinetsDB}");
                foreach (string dir in files) {
                    if (isWorkshop) {
                        ConfigManager.WriteConsole($"processing entry: {dir}");
                        if (Directory.Exists(dir)) {
                            CabinetInformation cbInfo = null;
                            try {
                                cbInfo = CabinetInformation.fromYaml(dir); //description.yaml
                                ConfigManager.WriteConsole($"** YAML loaded cabinet {cbInfo.name} rom: {cbInfo.rom}");

                                //all the errors are not a problem because there are defaults for each ones and the cabinet have to be made, exist or not an error.
                                CabinetInformation.showCabinetProblems(cbInfo);
                            }
                            catch (System.Exception e) {
                                ConfigManager.WriteConsole($"ERROR ** YAML cabinet not loaded {dir}: {e}");
                                cbInfo = null;                    
                            }

                            if (cbInfo != null) {
                                GameObject cabSpot = null;
                                string name = "";
                                Cabinet cab;

                                //spot selection
                                if (dir.Contains("/test")) {
                                    cabSpot = GameObject.Find("CabSpot");
                                    name = testedCabinetName;
                                }
                                else {
                                    cabSpot = cabinetSpots[cabinetFoundIndex];
                                    cabinetFoundIndex++;
                                    name = $"Cabinet-{cabinetFoundIndex}";
                                }

                                if (cabSpot) {

                                    //invoque and deploy the new cabinet
                                    try {
                                        cab = CabinetFactory.fromInformation(cbInfo, cabSpot.transform.position, cabSpot.transform.rotation);
                                        Object.Destroy(cabSpot);
                                        cab.gameObject.name = name;

                                        if (cab.gameObject.name == testedCabinetName) {
                                            //cabinet auto reload
                                            cab.gameObject.AddComponent(typeof(CabinetAutoReload));
                                        }
                                    }
                                    catch (System.Exception e) {
                                        ConfigManager.WriteConsole($"ERROR ** cabinet not deployed {dir}: {e}");
                                    }
                                }

                                if (cabinetFoundIndex >= cabinetSpots.Length) {
                                    break;
                                }
                            }
                        }
                    }

                }

                // if (isWorkshop) {
                //     System.Timers.Timer timer = new System.Timers.Timer(2000);
                //     timer.Start();
                //     timer.Elapsed += ReloadTestCabinet;
                // }
        */
        Debug.Log("+++++++++++++++++++++ Initialized");

    }
    /*
        private static void ReloadTestCabinet(object sender,  System.Timers.ElapsedEventArgs e) {
            string testFile = ConfigManager.Cabinets + "/test.zip";
            if (File.Exists(testFile)) {
                ConfigManager.WriteConsole($"New cabinet to test: {testFile}");

                //new cabinet to test
                CabinetInformation cbInfo = null;       
                try {
                    CabinetDBAdmin.loadCabinetFromZip(testFile);

                    cbInfo = CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/test"); //description.yaml

                    CabinetInformation.showCabinetProblems(cbInfo);

                    GameObject cabSpot = GameObject.Find(testedCabinetName);
                    if (cabSpot == null) {
                        ConfigManager.WriteConsole("ERROR ** there is no place where to deploy the cabinet");
                        return;
                    }

                    ConfigManager.WriteConsole($"Deploy test cabinet {cbInfo.name}");
                    Cabinet cab = CabinetFactory.fromInformation(cbInfo, cabSpot.transform.position, cabSpot.transform.rotation);
                    cab.gameObject.name = testedCabinetName;

                    ConfigManager.WriteConsole($"destroy previous tested cabinet {cabSpot.name}");
                    Object.Destroy(cabSpot);
                }
                catch (System.Exception ex) {
                    ConfigManager.WriteConsole($"ERROR loading cabinet from description {testFile}: {ex}");
                    return;
                }

                ConfigManager.WriteConsole("New Tested Cabinet deployed ******");
            }
        }
    */
}
