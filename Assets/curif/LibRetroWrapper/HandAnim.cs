using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This code original came from here and then was modified because it was pretty ugly:
/// https://gist.github.com/JokingJester/ad735e61f47bedde7ab7ff52ac302933/revisions
/// The concept comes from here:
/// https://medium.com/@dnwesdman/animating-gamedevhqs-vr-hands-with-the-xr-interaction-toolkit-52d843aef762
/// </summary>
public class HandAnim : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty indexAction;
    [SerializeField] private InputActionProperty thumbAction;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float speed = 4;

    private float _gripCurrent;
    private float _gripTarget;

    private float _triggerCurrent;
    private float _triggerTarget;

    private float _thumbCurrent;
    private float _thumbTarget;

    private float _pointCurrent;
    private float _pointTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimateGripAndPinch();
        AnimateThumb();
        AnimatePointing();
    }

    private void AnimateGripAndPinch()
    {
        _gripTarget = gripAction.action.ReadValue<float>();
        if (_gripCurrent != _gripTarget)
        {
            _gripCurrent = Mathf.MoveTowards(_gripCurrent, _gripTarget, Time.deltaTime * speed);
        }

        _triggerTarget = triggerAction.action.ReadValue<float>();
        if (_triggerCurrent != _triggerTarget)
        {
            _triggerCurrent = Mathf.MoveTowards(_triggerCurrent, _triggerTarget, Time.deltaTime * speed);
        }

        animator.SetFloat("Flex", _gripCurrent);
        animator.SetFloat("Pinch", _triggerCurrent);
    }

    private void AnimateThumb()
    {
        _thumbTarget = thumbAction.action.ReadValue<float>();
        _thumbTarget = _thumbTarget == 0 ? _thumbTarget = 1 : _thumbTarget = 0;

        if (_thumbCurrent != _thumbTarget)
        {
            _thumbCurrent = Mathf.MoveTowards(_thumbCurrent, _thumbTarget, Time.deltaTime * speed);
        }
        animator.SetLayerWeight(1, _thumbCurrent);
    }

    private void AnimatePointing()
    {
        _pointTarget = indexAction.action.ReadValue<float>();
        _pointTarget = _pointTarget == 0 ? _pointTarget = 1 : _pointTarget = 0;

        if (_pointCurrent != _pointTarget)
        {
            _pointCurrent = Mathf.MoveTowards(_pointCurrent, _pointTarget, Time.deltaTime * speed);
        }
        animator.SetLayerWeight(2, _pointCurrent);
    }
}