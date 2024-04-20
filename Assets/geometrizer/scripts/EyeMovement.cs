using System.Collections;
using UnityEngine;

public class EyeMovement : MonoBehaviour
{
    public float MinMoveTime = 0.1f;
    public float MaxMoveTime = 0.25f;
    public float MinHoldTime = 0.1f;
    public float MaxHoldTime = 1.0f;

    private SkinnedMeshRenderer eyeRenderer;
    private MaterialPropertyBlock props;

    private void Awake()
    {
        eyeRenderer = GetComponent<SkinnedMeshRenderer>();
        props = new MaterialPropertyBlock();
        eyeRenderer.GetPropertyBlock(props);
    }

    private void Start()
    {
        StartCoroutine(AnimateEyeHV());
        StartCoroutine(AnimateEyeFocus());
    }

    private IEnumerator AnimateEyeHV()
    {
        while (true)
        {
            float moveTime = Random.Range(MinMoveTime, MaxMoveTime);
            float newEyeH = Random.Range(0f, 1f);
            float newEyeV = Random.Range(0f, 1f);

            StartCoroutine(ChangeShaderParameter("_Eye_H", props.GetFloat("_Eye_H"), newEyeH, moveTime));
            StartCoroutine(ChangeShaderParameter("_Eye_V", props.GetFloat("_Eye_V"), newEyeV, moveTime));

            // Wait for the move animation plus a random hold time
            float holdTime = Random.Range(MinHoldTime, MaxHoldTime);
            yield return new WaitForSeconds(moveTime + holdTime);
        }
    }

    private IEnumerator AnimateEyeFocus()
    {
        while (true)
        {
            float moveTime = Random.Range(MinMoveTime, MaxMoveTime);
            float newEyeFocus = Random.Range(0f, 1f);

            StartCoroutine(ChangeShaderParameter("_Eye_Focus", props.GetFloat("_Eye_Focus"), newEyeFocus, moveTime));

            // Wait for the move animation plus a random hold time
            float holdTime = Random.Range(MinHoldTime, MaxHoldTime);
            yield return new WaitForSeconds(moveTime + holdTime);
        }
    }

    private IEnumerator ChangeShaderParameter(string parameterName, float startValue, float endValue, float duration)
    {
        eyeRenderer.GetPropertyBlock(props);
        float time = 0;
        while (time < duration)
        {
            float newValue = Mathf.Lerp(startValue, endValue, time / duration);
            props.SetFloat(parameterName, newValue);
            eyeRenderer.SetPropertyBlock(props);
            time += Time.deltaTime;
            yield return null;
        }
        props.SetFloat(parameterName, endValue);
        eyeRenderer.SetPropertyBlock(props);
    }
}
