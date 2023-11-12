using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BoxCollider))]
public class AgentScenePosition : MonoBehaviour
{

    public bool IsPlayerPresent = false;
    public bool IsNPCPresent = false;
    public string NPCPresentName = "";
    public float BoxColliderHeight = 5f;
    public BoxCollider boxCollider;
    void Start()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                ConfigManager.WriteConsoleError($"[AgentScenePosition] box collider doesn't exists in the position");
                return;
            }
        }
        boxCollider.isTrigger = true;
        Vector3 size = boxCollider.size;
        size.y = BoxColliderHeight;
        boxCollider.size = size;
    }

    public bool ItsMe(string name)
    {
        return IsNPCPresent && NPCPresentName == name;

    }
    public bool NPCIsPresent(string name)
    {
        return IsNPCPresent && ItsMe(name);
    }
    public bool IsTaken
    {
        get { return IsNPCPresent || IsPlayerPresent; }
    }


    private bool colliderIsPlayer(Collider collision)
    {
        return collision.gameObject.name == "OVRPlayerControllerGalery" || 
                collision.gameObject.name == "GrabVolumeSmall" || 
                collision.gameObject.name == "GrabVolumeBig";
    }

    private void OnTriggerEnter(Collider collision)
    {
        // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerEnter] {name}: {collision.gameObject.name}");
        if (colliderIsPlayer(collision))
        {
            // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerEnter] {name}: {collision.gameObject.name}");
            IsPlayerPresent = true;
            IsNPCPresent = false;
            NPCPresentName = "";
        }
        else
        {
            if (!IsNPCPresent && collision.gameObject.tag == "NPC")
            {
                // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerEnter] {name}: NPC present in position is {collision.gameObject.name}");
                IsNPCPresent = true;
                NPCPresentName = collision.gameObject.name;
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        //ConfigManager.WriteConsole($"[OnTriggerExit] {name}: {collision.gameObject.name}");
        if (colliderIsPlayer(collision))
        {
            //ConfigManager.WriteConsole($"[OnTriggerExit] {name}: {collision.gameObject.name}");
            IsPlayerPresent = false;
        }
        else
        {
            if (collision.gameObject.tag == "NPC" && collision.gameObject.name == NPCPresentName)
            {
                //ConfigManager.WriteConsole($"[OnTriggerEnter] {name}: NPC left the position {collision.gameObject.name}");
                IsNPCPresent = false;
                NPCPresentName = "";
            }
        }
    }

}
