using System;
using UnityEngine;
using static CabinetInformation;
using static CabinetInformation.Physical;

public class InteractablePart : MonoBehaviour
{
    public Rigidbody rigidBody;
    public Collider collider;
    public CollisionDetection collisionDetection;
    public GrabDetection grabDetection;

    public Physical physicalInfo;

    public bool Initialized;

    private bool getbool(bool? b)
    {
        if (b == null)
            return false; //true is the default to freeze
        return b.Value;
    }

    public void Start()
    {
        base.gameObject.layer = LayerMask.NameToLayer("InteractablePart");

        switch (physicalInfo.shape)
        {
            case "box":
                collider = gameObject.AddComponent<BoxCollider>();
                break;
            case "sphere":
                collider = gameObject.AddComponent<SphereCollider>();
                break;
            case "capsule":
                collider = gameObject.AddComponent<CapsuleCollider>();
                break;
            default:
                collider = gameObject.AddComponent<BoxCollider>();
                break;
        }
        collider.isTrigger = false;

        rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody == null)
            rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        rigidBody.mass = physicalInfo.mass;

        if (physicalInfo.gravity)
            ActivateGravity();
        else
            DeactivateGravity();

        if (physicalInfo.material != null)
        {
            PhysicMaterial pmat = new PhysicMaterial($"{gameObject.name}-physics-material");
            pmat.staticFriction = physicalInfo.material.staticFriction;
            pmat.dynamicFriction = physicalInfo.material.dynamicFriction;
            pmat.bounciness = physicalInfo.material.bounciness;
            pmat.frictionCombine = CabinetInformation.Physical.Material.CombineFromString(physicalInfo.material.frictionCombine);
            pmat.bounceCombine = CabinetInformation.Physical.Material.CombineFromString(physicalInfo.material.bounceCombine);
            collider.material = pmat;
        }

        if (physicalInfo.receiveImpacts != null &&
            physicalInfo.receiveImpacts.parts.Count > 0)
        {
            //add a collision detection component to process the events
            collisionDetection = gameObject.GetComponent<CollisionDetection>();
            if (collisionDetection == null)
                collisionDetection = gameObject.AddComponent<CollisionDetection>();
            collisionDetection.impacts = physicalInfo.receiveImpacts;
        }

        if (physicalInfo.playerInteraction != null)
        {
            grabDetection = gameObject.GetComponent<GrabDetection>();
            if (grabDetection == null)
                grabDetection = gameObject.AddComponent<GrabDetection>();
            grabDetection.isGrabbable = physicalInfo.playerInteraction.isGrababble;
            grabDetection.isTouchable = physicalInfo.playerInteraction.isTouchable;
        }
        Initialized = true;
        Deactivate(); //wait for an insert coin or similar to start.
    }

    public void Activate()
    {
        collider.enabled = true;
        if (grabDetection != null) 
            grabDetection.enabled = true;
        if (collisionDetection != null)
            collisionDetection.enabled = true;
        rigidBody.isKinematic = false;
    }
    public void Deactivate()
    {
        collider.enabled = false;
        if (grabDetection != null)
            grabDetection.enabled = false;
        if (collisionDetection != null)
            collisionDetection.enabled = false;
        rigidBody.isKinematic = true;
    }

    public void ActivateGravity()
    {
        RigidbodyConstraints constraints = RigidbodyConstraints.None;

        rigidBody.useGravity = true;

        if (getbool(physicalInfo.freezePosition.x)) constraints |= RigidbodyConstraints.FreezePositionX;
        if (getbool(physicalInfo.freezePosition.y)) constraints |= RigidbodyConstraints.FreezePositionY;
        if (getbool(physicalInfo.freezePosition.z)) constraints |= RigidbodyConstraints.FreezePositionZ;

        if (getbool(physicalInfo.freezeRotation.x)) constraints |= RigidbodyConstraints.FreezeRotationX;
        if (getbool(physicalInfo.freezeRotation.y)) constraints |= RigidbodyConstraints.FreezeRotationY;
        if (getbool(physicalInfo.freezeRotation.z)) constraints |= RigidbodyConstraints.FreezeRotationZ;
        
        rigidBody.constraints = constraints;
    }


    public void DeactivateGravity()
    {
        rigidBody.useGravity = false;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public static InteractablePart Factory(GameObject go, Physical physicalInfo)
    {

        if (physicalInfo == null)
            throw new System.ArgumentException($"physical information in missing in the construction of an Interactable Part: {go.name}");

        InteractablePart ip = go.GetComponent<InteractablePart>();
        if (ip == null)
        {
            // add components
            ip = go.AddComponent<InteractablePart>();
            ip.physicalInfo = physicalInfo;
        }

        return ip;
    }

}
