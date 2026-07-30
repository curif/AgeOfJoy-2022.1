/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;

// Dedicated box collider that teleports the player when they walk through it.
// Lives on its own GameObject so it never shares a collider with unrelated triggers
// (e.g. a screen's proximity box), which would otherwise fire this behavior by accident.
[RequireComponent(typeof(BoxCollider))]
public class TeleportationBox : MonoBehaviour
{
    [Tooltip("Room the player is teleported to when entering this box.")]
    public SceneDocument TeleportTo;

    [Tooltip("Teleportation component that performs the teleport. If empty, it's looked up on 'FixedObject'.")]
    public Teleportation Teleportation;

    private GameObject player;

    void Start()
    {
        player = GameObject.Find("OVRPlayerControllerGalery");

        if (Teleportation == null)
        {
            GameObject roomInit = GameObject.Find("FixedObject");
            if (roomInit != null)
                Teleportation = roomInit.GetComponent<Teleportation>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player)
            return;

        if (Teleportation == null || TeleportTo == null)
        {
            ConfigManager.WriteConsoleError($"[TeleportationBox] can't teleport, Teleportation or TeleportTo not set.");
            return;
        }

        Teleportation.Teleport(TeleportTo);
    }
}
