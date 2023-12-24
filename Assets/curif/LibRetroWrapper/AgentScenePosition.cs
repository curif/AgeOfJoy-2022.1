using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BoxCollider))]
public class AgentScenePosition : MonoBehaviour
{

    public bool IsPlayerColliding = false;
    public bool IsPlayerPresent = false;

    public bool IsNPCPresent = false;
    public string NPCPresentName = "";
    public float BoxColliderHeight = 5f;
    public BoxCollider boxCollider;
    public float playerStayDurationTimeSecs = 0f;
    private float playerTimer = 0f;

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
        playerTimer = 0f;
    }

    public bool ItsMe(string name)
    {
        return IsNPCPresent && NPCPresentName == name;
    }

    public bool NPCIsPresent(string name)
    {
        return IsNPCPresent && NPCPresentName == name;
    }

    public bool IsTaken
    {
        get { return IsNPCPresent || IsPlayerColliding; }
    }

    private bool colliderIsPlayer(Collider collision)
    {
        return collision.gameObject.name == "OVRPlayerControllerGalery"
        /* ||
                collision.gameObject.name == "GrabVolumeSmall" ||
                collision.gameObject.name == "GrabVolumeBig"*/;
    }

    private void OnTriggerEnter(Collider collision)
    {
        // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerEnter] {name}: {collision.gameObject.name}");
        if (colliderIsPlayer(collision))
        {
            // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerEnter] {name}: {collision.gameObject.name}");
            IsPlayerColliding = true;
            playerTimer = 0f;
            IsNPCPresent = false;
            NPCPresentName = "";
            if (playerStayDurationTimeSecs > 0)
                InvokeRepeating("checkPlayerTimer", playerStayDurationTimeSecs, 1f);
            else
                IsPlayerPresent = true;
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

    void checkPlayerTimer()
    {
        IsPlayerPresent = IsPlayerColliding;
        CancelInvoke("checkPlayerTimer");
        ConfigManager.WriteConsole($"[AgentScenePosition.checkPlayerTimer] {name}: is player present: {IsPlayerPresent} ");
    }

    private void OnTriggerExit(Collider collision)
    {
        //ConfigManager.WriteConsole($"[OnTriggerExit] {name}: {collision.gameObject.name}");
        if (colliderIsPlayer(collision))
        {
            // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerExit] {name}: {collision.gameObject.name}");
            IsPlayerColliding = false;
            IsPlayerPresent = false;
            playerTimer = 0f;
        }
        else
        {
            if (collision.gameObject.tag == "NPC" && collision.gameObject.name == NPCPresentName)
            {
                // ConfigManager.WriteConsole($"[AgentScenePosition.OnTriggerExit] {name}: NPC left the position {collision.gameObject.name}");
                IsNPCPresent = false;
                NPCPresentName = "";
            }
        }
    }

}
