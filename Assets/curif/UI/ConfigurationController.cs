using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationController : MonoBehaviour
{
    public ScreenGenerator scr;

    private GenericMenu mainMenu;

    // Start is called before the first frame update
    void Start()
    {
      mainMenu = new(scr, "AGE of Joy - Main configuration", new string[] {"Sound configuration", "NPC configuration", "Controllers"});
    }

    // Update is called once per frame
    void Update()
    {
      mainMenu.DrawMenu();
    }
}
