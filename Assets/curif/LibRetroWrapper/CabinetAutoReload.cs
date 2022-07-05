using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CabinetAutoReload : MonoBehaviour
{
    private LibretroMameCore.Waiter SecsForCheckReload = new(2);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SecsForCheckReload.Finished()) {
            SecsForCheckReload.reset();
            reload();
        }
    }

    private void reload() {
        string testFile = ConfigManager.Cabinets + "/test.zip";
        if (File.Exists(testFile)) {
            ConfigManager.WriteConsole($"New cabinet to test: {testFile}");
                
            //new cabinet to test
            CabinetInformation cbInfo = null;
            try {
                CabinetDBAdmin.loadCabinetFromZip(testFile);

                cbInfo = CabinetInformation.fromYaml(ConfigManager.CabinetsDB + "/test"); //description.yaml
             
                CabinetInformation.showCabinetProblems(cbInfo);

                //cabinet inseption
                ConfigManager.WriteConsole($"Deploy test cabinet {cbInfo.name}");
                Cabinet cab = CabinetFactory.fromInformation(cbInfo, gameObject.transform.position, gameObject.transform.rotation);
                Object.Destroy(gameObject);
                cab.gameObject.AddComponent(typeof(CabinetAutoReload));

            }
            catch (System.Exception ex) {
                ConfigManager.WriteConsole($"ERROR loading cabinet from description {testFile}: {ex}");
                return;
            }

            ConfigManager.WriteConsole("New Tested Cabinet deployed ******");
        }
    }
}
