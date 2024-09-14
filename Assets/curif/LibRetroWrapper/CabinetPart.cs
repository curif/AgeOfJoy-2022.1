using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using static CabinetInformation;

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

public class CabinetPart : MonoBehaviour
{
    [Header("World Space Transform")]
    public TransformData initialWorldTransform;

    [Header("Local Space Transform")]
    public TransformData initialLocalTransform;

    void Start()
    {
        // Store the initial world space transform
        initialWorldTransform = new TransformData
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };

        // Store the initial local space transform
        initialLocalTransform = new TransformData
        {
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = transform.localScale
        };
    }

    public GameObject GameObject { get { return transform.gameObject; } }
    public Transform Transform { get { return transform; } }

    public CabinetPart Scale(float crtScalePercentage, float crtXratio, float crtYratio, float crtZratio)
    {
        float scale = crtScalePercentage / 100f;
        transform.localScale =
            new Vector3(transform.localScale.x * crtXratio * scale,
                        transform.localScale.y * crtYratio * scale,
                        transform.localScale.z * crtZratio * scale);
        return this;
    }

    public CabinetPart Rotate(Vector3 rotation)
    {
        transform.Rotate((Vector3)rotation);
        return this;
    }

    public CabinetPart Rotate(float angleX, float angleY, float angleZ)
    {
        transform.Rotate(angleX, angleY, angleZ);
        return this;
    }

    public CabinetPart ApplyUserConfigurationGeometry(Geometry g)
    {
        if (g != null)
        {
            Scale(g.scalepercentage, g.ratio.x, g.ratio.y, g.ratio.z);
            Rotate(g.rotation.x, g.rotation.y, g.rotation.z);
        }
        return this;
    }


    public CabinetPart Enable(bool enabled)
    {
        gameObject.SetActive(enabled);
        return this;
    }

    public CabinetPart RotateLocalEulerAngleByAxisFromOrigin(string axis, float angle)
    {

        // Validate rotation value to be within 0 to 360 degrees
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {name} value must be between -360 and 360 degrees. value {angle}");

        Quaternion newRotation;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
        transform.localRotation = initialLocalTransform.rotation * newRotation;
        
        return this;
    }

    public CabinetPart RotateLocalEulerAngleByAxis(string axis, float angle)
    {

        // Validate rotation value to be within 0 to 360 degrees
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {name} value must be between -360 and 360 degrees. value {angle}");

        Quaternion newRotation;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception("cabPartsSetGlobalRotation: axis should be X, Y, or Z");
        }
        transform.localRotation *= newRotation;

        return this;
    }

    public float GetLocalRotationByAxis(string axis)
    {
        Quaternion deltaRotation = transform.localRotation * Quaternion.Inverse(initialLocalTransform.rotation);

        // Convert the delta rotation quaternion to Euler angles
        Vector3 deltaEuler = deltaRotation.eulerAngles;

        switch (axis)
        {
            case "X":
                return deltaEuler.x;
            case "Y":
                return deltaEuler.y;
            case "Z":
                return deltaEuler.z;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
    }

    public CabinetPart RotateWorldEulerAngleByAxis(string axis, float angle)
    {

        // Validate rotation value to be within 0 to 360 degrees
        if (angle < -360 || angle > 360)
            throw new Exception($"Rotation: {name} value must be between -360 and 360 degrees. value {angle}");

        Quaternion newRotation;
        switch (axis)
        {
            case "X":
                newRotation = Quaternion.Euler(angle, 0, 0);
                break;
            case "Y":
                newRotation = Quaternion.Euler(0, angle, 0);
                break;
            case "Z":
                newRotation = Quaternion.Euler(0, 0, angle);
                break;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
        transform.rotation = initialWorldTransform.rotation * newRotation;

        return this;
    }

    public float GetWorldRotationByAxis(string axis)
    {
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(initialWorldTransform.rotation);

        // Convert the delta rotation quaternion to Euler angles
        Vector3 deltaEuler = deltaRotation.eulerAngles;

        switch (axis)
        {
            case "X":
                return deltaEuler.x;
            case "Y":
                return deltaEuler.y;
            case "Z":
                return deltaEuler.z;
            default:
                throw new Exception($"Rotation: {name} axis should be X, Y, or Z: {axis}");
        }
    }
}
