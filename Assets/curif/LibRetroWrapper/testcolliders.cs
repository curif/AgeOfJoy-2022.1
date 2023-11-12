/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Eflatun.SceneReference;
using UnityEngine.XR.Interaction.Toolkit;
//[RequireComponent(typeof(BoxCollider))]
//[RequireComponent(typeof(MeshCollider))]
//[RequireComponent(typeof(TeleportationArea))]
public class testcolliders : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Code to handle collision goes here
        ConfigManager.WriteConsole($"[testcolliders.OnCollisionEnter] collision {collision.gameObject} hit to {name}");
    }
    private void OnCollisionExit(Collision collision)
    {
        ConfigManager.WriteConsole($"[testcolliders.OnCollisionExit] uncollision {collision.gameObject} un-hit to {name}");
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Code to handle collision goes here
        ConfigManager.WriteConsole($"[testcolliders.OnTriggerEnter] ENTER {collision.gameObject} hit to {name}");

    }
    private void OnTriggerExit(Collider collision)
    {
        ConfigManager.WriteConsole($"[testcolliders.OnTriggerExit] EXIT {collision.gameObject} un-hit to {name}");
    }

}