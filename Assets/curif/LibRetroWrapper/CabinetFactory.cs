/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;
using System.Collections.Generic;
//store Cabinets resources
public static class CabinetFactory
{
  public static Dictionary<string, GameObject> CabinetStyles = new();

  static CabinetFactory()
  {
    CabinetStyles.Add("generic", Resources.Load<GameObject>($"Cabinets/PreFab/Generic"));
    CabinetStyles.Add("timeplt", Resources.Load<GameObject>($"Cabinets/PreFab/TimePilot"));
    CabinetStyles.Add("galaga", Resources.Load<GameObject>($"Cabinets/PreFab/Galaga"));
    CabinetStyles.Add("pacmancabaret", Resources.Load<GameObject>($"Cabinets/PreFab/PacManCabaret"));
    CabinetStyles.Add("frogger", Resources.Load<GameObject>($"Cabinets/PreFab/Frogger"));
    CabinetStyles.Add("defender", Resources.Load<GameObject>($"Cabinets/PreFab/Defender"));
    CabinetStyles.Add("donkeykong", Resources.Load<GameObject>($"Cabinets/PreFab/DonkeyKong"));
    CabinetStyles.Add("xevious", Resources.Load<GameObject>($"Cabinets/PreFab/Xevious"));
    CabinetStyles.Add("1942", Resources.Load<GameObject>($"Cabinets/PreFab/1942"));
  }

  public static Cabinet Factory(string style, string name, int number, string room, Vector3 position, Quaternion rotation, Transform parent)
  {
    if (!CabinetStyles.ContainsKey(style) || CabinetStyles[style] == null)
    {
      Debug.LogError($"[Cabinet.Factory]: style {style} unknown or not loaded, falls to 'generic' cabinet");
      style = "generic";
    }
    string cabinetName = $"cabinet-{name}-{room}-{number}";
    return new Cabinet(cabinetName, position, rotation, parent, go: CabinetStyles[style]);
  }
  
  public static Cabinet fromInformation(CabinetInformation cbinfo, string room, int number, Vector3 position, Quaternion rotation, Transform parent)
  {
    Cabinet cabinet = CabinetFactory.Factory(cbinfo.style, cbinfo.name, number, room, position, rotation, parent);
    if (cbinfo.material != null)
    {
      cabinet.SetMaterial(CabinetMaterials.fromName(cbinfo.material));
    }
    else if (cbinfo.color != null)
    {
      Material mat = new Material(CabinetMaterials.Base);
      mat.SetColor("_Color", cbinfo.color.getColor());
      cabinet.SetMaterial(mat);
    }

    //process each part
    if (cbinfo.Parts != null) {
      ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} texture each part");
      foreach (CabinetInformation.Part p in cbinfo.Parts)
      {
        switch (p.type)
        {
          case "bezel" : {
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} bezel {p.art.file}");
            cabinet.SetBezel(p.name, cbinfo.getPath(p.art.file));
          }
          break;
          
          case "marquee" : {
            ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} marquee {p.art.file}");
            cabinet.SetMarquee(p.name, cbinfo.getPath(p.art.file));
            if (p.color != null) 
              cabinet.SetMarqueeEmissionColor(p.name, p.color.getColor());
          }
          break;

          default:
          {
            if (p.material != null)
              cabinet.SetMaterial(p.name, CabinetMaterials.fromName(p.material));
            else if (p.art != null)
              cabinet.SetTextureTo(p.name, cbinfo.getPath(p.art.file), CabinetMaterials.Base, invertX: p.art.invertx, invertY: p.art.inverty);
            else if (p.color != null)
            {
              Material matColor = new Material(CabinetMaterials.Base);
              matColor.SetColor("_Color", p.color.getColor());
              cabinet.SetMaterial(p.name, matColor);
            }
            else
              cabinet.SetMaterial(p.name, CabinetMaterials.Black);
          }
          break;
        }
      }
      
    }
    /*
    if (cbinfo.bezel != null)
    {
      ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} bezel {cbinfo.bezel.art.file}");
      cabinet.SetBezel(cbinfo.getPath(cbinfo.bezel.art.file));
    }

    if (cbinfo.marquee != null)
    {
      ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} marquee {cbinfo.marquee.art.file}");
      cabinet.SetMarquee(cbinfo.getPath(cbinfo.marquee.art.file), cbinfo.marquee.lightcolor.getColor());
    }
    else
      cabinet.SetMarquee("", Color.white);
    */

    if (!string.IsNullOrEmpty(cbinfo.coinslot))
    {
      ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} coinslot {cbinfo.coinslot}");
      cabinet.AddCoinSlot(cbinfo.coinslot);
    }

    cabinet.addCRT(cbinfo.crt.type, cbinfo.crt.orientation, cbinfo.rom, cbinfo.getPath(cbinfo.video.file), cbinfo.timetoload,
                    invertX: cbinfo.crt.screen.invertx, invertY: cbinfo.crt.screen.inverty,
                    GameVideoFileInvertX: cbinfo.video.invertx, GameVideoFileInvertY: cbinfo.video.inverty
                    );
    ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} CRT added");

    return cabinet;
  }
}
