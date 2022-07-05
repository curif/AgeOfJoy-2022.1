using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AddComponentMenu("curif/LibRetroWrapper/MameScreenController")]
public class MameScreenController : MonoBehaviour
{
    private MameCore _mame = MameCore.Instance;

    public string gameFile = "pacman.zip";
    private string path = "/storage/emulated/0/RetroArch/downloads/";

    // Start is called before the first frame update
    void Start() {
        Debug.Log("++++++++ ============== +++++++++++\n++++++++ Init Mame Core +++++++++++\n++++++++ ============== +++++++++++");

        //the library must to be registered in Unity, put the xxx.so library in assets folder, 
        //set properties to "select platform" -> android and CPU -> ARM64
        _mame.Init("mame2000_libretro_android.so");

        Debug.Log(string.Format("LibRetro API Version: {0}", _mame.retro_api_version()));

        Debug.Log("retro_set_environment");
        _mame.retro_set_environment(environmentCB); //set environment handler callback

        //retro_system_info info = new retro_system_info(); 
        //_mame.retro_get_system_info(info);
        //Debug.Log(string.Format("Mame Info ---------\n {}", info.ToString()));
        Debug.Log("Started");
    }

    public bool environmentCB(uint cmd, IntPtr data) {
     
        Debug.Log("===> Environment cmd " + cmd);
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
