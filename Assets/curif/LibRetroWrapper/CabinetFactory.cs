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

    foreach (KeyValuePair<string, GameObject> cab in CabinetStyles)
    {
      cab.Value.AddComponent<MeshCollider>();
    }
  }

  public static Cabinet Factory(string style, string name, Vector3 position, Quaternion rotation)
  {
    if (!CabinetStyles.ContainsKey(style) || CabinetStyles[style] == null)
    {
      Debug.LogError($"[Cabinet.Factory]: style {style} unknown or not loaded, falls to 'generic' cabinet");
      style = "generic";
    }

    return new Cabinet(name, position, rotation, CabinetStyles[style]);
  }

  public static Cabinet fromInformation(CabinetInformation cbinfo, Vector3 position, Quaternion rotation)
  {
    Cabinet cabinet = CabinetFactory.Factory(cbinfo.style, cbinfo.name, position, rotation);
    cabinet.SetMaterial(CabinetMaterials.fromName(cbinfo.material));

    //process each part
    ConfigManager.WriteConsole($"[CabinetFactory.fromInformation] {cbinfo.name} texture each part");
    foreach (CabinetInformation.Part p in cbinfo.Parts)
    {
      Material mat = CabinetMaterials.Base;
      if (p.material != null)
      {
        mat = CabinetMaterials.fromName(p.material);
      }

      if (p.art != null)
      {
        cabinet.SetTextureTo(p.name, cbinfo.getPath(p.art.file), mat, invertX: p.art.invertx, invertY: p.art.inverty);
      }
      else
      {
        cabinet.SetMaterial(p.name, mat);
      }
    }

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
