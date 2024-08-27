using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using static CabinetInformation;

[RequireComponent(typeof(BoxCollider))]
public class InteractablePart : MonoBehaviour
{
    CollisionDetection collisionDetection;
    Rigidbody rigidBody;
    BoxCollider boxCollider;
    Physical physicalInfo;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("InteractablePart");
        boxCollider = GetComponent<BoxCollider>();
    }

    public static InteractablePart GetOrAdd(GameObject go)
    {
        InteractablePart ip = go.GetComponent<InteractablePart>();
        if (ip == null)
            ip = go.AddComponent<InteractablePart>();
        ip.SetAsNotPhysical();

        return ip;
    }

    //The collider object refers to the object that is being hit or impacted by the colliding object.
    public InteractablePart SetAsCollider(ReceiveImpacts impact)
    {

        collisionDetection = gameObject.GetComponent<CollisionDetection>();
        if (collisionDetection == null)
            collisionDetection = gameObject.AddComponent<CollisionDetection>();
        collisionDetection.impact = impact;

        return this;
    }
    public InteractablePart SetAsNotPhysical()
    {
        if (rigidBody != null)
        {
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
        }
        boxCollider.isTrigger = false;
        return this;
    }
    public InteractablePart SetAsPhysical()
    {
        if (physicalInfo == null)
            throw new System.Exception("The object should be assigned as physical in yaml description");

        if (rigidBody == null)
            rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
                rigidBody = gameObject.AddComponent<Rigidbody>();

        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;// !physicalInfo.gravity;

        return this;
    }

    public InteractablePart SetAsPhysical(Physical physicalInfo)
    {
        this.physicalInfo = physicalInfo;
        SetAsPhysical();

        rigidBody.mass = physicalInfo.mass;

        RigidbodyConstraints constraints = RigidbodyConstraints.None;

        if (physicalInfo.freezePosition.x) constraints |= RigidbodyConstraints.FreezePositionX;
        if (physicalInfo.freezePosition.y) constraints |= RigidbodyConstraints.FreezePositionY;
        if (physicalInfo.freezePosition.z) constraints |= RigidbodyConstraints.FreezePositionZ;

        if (physicalInfo.freezeRotation.x) constraints |= RigidbodyConstraints.FreezeRotationX;
        if (physicalInfo.freezeRotation.y) constraints |= RigidbodyConstraints.FreezeRotationY;
        if (physicalInfo.freezeRotation.z) constraints |= RigidbodyConstraints.FreezeRotationZ;

        rigidBody.constraints = constraints;

        if (physicalInfo.material != null)
        {
            PhysicMaterial pmat = new PhysicMaterial($"{name}-physics-material");
            pmat.staticFriction = physicalInfo.material.staticFriction;
            pmat.dynamicFriction = physicalInfo.material.dynamicFriction;
            pmat.bounciness = physicalInfo.material.bounciness;
            pmat.frictionCombine = CabinetInformation.Physical.Material.CombineFromString(physicalInfo.material.frictionCombine);
            pmat.bounceCombine = CabinetInformation.Physical.Material.CombineFromString(physicalInfo.material.bounceCombine);
            boxCollider.material = pmat;
        }
        return this;
    }


    //The colliding object is the object that is considered to be moving and initiating the collision.
    public InteractablePart SetAsColliding()
    {
        //The isKinematic property determines whether the physics engine controls the movement of the Rigidbody, or whether the object's movement is controlled exclusively by the user via scripts or animations.
        //When isKinematic is set to true: The Rigidbody will not be affected by forces, collisions, or other physics interactions.
        //rigidbody.isKinematic = true;
        //rigidbody.automaticInertiaTensor = false;
        //rigidbody.automaticCenterOfMass = false;

        //It controls whether the Rigidbody is affected by gravity, which is a fundamental force in the Unity physics engine.
        //rigidbody.useGravity = false;

        return this;
    }
}
