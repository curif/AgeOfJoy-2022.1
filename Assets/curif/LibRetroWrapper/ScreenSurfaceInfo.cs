/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;

// Describes where the "screen surface" lives on a CRT GameObject:
//   materialSlot  — index into Renderer.materials that the active shader (crt/clean/projector…) writes to.
//   submeshIndex  — index into the Mesh submeshes that LightGunTarget ray-traces for UV hit detection.
//
// The built-in screen prefabs (screen19i, screen32i, …) carry two slots/submeshes
// (0 = bezel, 1 = screen surface) and DON'T need this component: every consumer defaults
// to 1 when it's absent, so their behavior is unchanged.
//
// Custom screen meshes (crt.type: custom) supply a single-submesh mesh from the cabinet .glb;
// Cabinet.addCRT attaches this component with materialSlot = 0 / submeshIndex = 0.
public class ScreenSurfaceInfo : MonoBehaviour
{
    public int materialSlot = 1;
    public int submeshIndex = 1;
}
