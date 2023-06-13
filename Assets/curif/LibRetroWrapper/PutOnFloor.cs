/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutOnFloor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float floorHeight = 0.1f;

        Transform floor = DetectFloor();
        if (floor == null)
            return;

        //ConfigManager.WriteConsole($"[PutOnFloor] Cabinet {name} floor found");

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, floor.position.y + floorHeight/2, gameObject.transform.position.z);
        //gameObject.transform.position.y = floor.position.y + 0.01f;
    }

    //https://forum.unity.com/threads/here-is-an-editor-script-to-help-place-objects-on-ground.38186/
    public Transform DetectFloor() {
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(gameObject.transform.position, Vector3.down);
        //https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        if (Physics.Raycast(ray, out hit, 1000))
            return hit.transform;

        //If no hit then you have attempted to measure the height somewhere off the mesh
        return null;
    }

}