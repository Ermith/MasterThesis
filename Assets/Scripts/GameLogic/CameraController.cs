using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraModeType { FirstPerson, ThirdPerson, TopDown }
public class CameraController : MonoBehaviour
{
    #region FIELDS
    // Control Parameters
    public Transform Target;
    public float TopDownInclanation = 3f;
    public float TopDownDistance = 20f;
    public float TopDownDampTime = 0.1f;
    public float ThirdPersonDistance = 5f;
    public float ThirdPersonDampTime = 0.1f;

    public CameraModeType CameraMode { get; private set; }

    // Camera Mode Params
    private float _distance = 20;
    private float _horizontalRotation;
    private float _verticalRotation; // rotation
    private bool _horizontalRotationEnabled = true;
    private bool _verticalRotationEnabled = true;
    private float _damping = 0.1f;

    // Internal Results
    private Vector3 _direction;
    private Vector3 _velocity;
    private Vector3 _pivotPosition;

    // Camera Bob
    private enum BobState { None, RightStep, RightRise, LeftStep, LeftRise }
    public float BobDuration = 0.35f;
    public float BobScale = 0.075f;
    public bool BobEnabled = true;
    private BobState _bobState = BobState.LeftStep;
    private float _bobTime = 0f;
    private Vector3 _previousOffset;

    // Custom rotation
    private float _customRotationDuration = 1f;
    private float _customRotationTime = 0f;
    private Vector3 _fromEulers;
    private Vector3 _toEulers;
    private Vector3 _rotationOffset;
    #endregion

    #region PUBLIC_INTERFACE
    public void Rotate(float horizontal, float vertical, float speed = 1)
    {
        if (!_verticalRotationEnabled) vertical = 0f;
        if (!_horizontalRotationEnabled) horizontal = 0f;

        speed *= 0.1f; // 1 is too fast
        float polar = _horizontalRotation + horizontal * speed;
        float inclanation = _verticalRotation + vertical * speed;

        SetRotation(polar, inclanation);
    }

    public Vector3 GetGroundDirection()
    {
        return new Vector3(
            -_direction.x,
            0,
            -_direction.z
            ).normalized;
    }

    public void SwitchMode(CameraModeType cameraMode)
    {
        switch (cameraMode)
        {
            case CameraModeType.FirstPerson:
                SetParams(
                    distance: 0f,
                    horizontalRotation: true,
                    verticalRotation: true,
                    damping: 0f);
                break;

            case CameraModeType.ThirdPerson:
                SetParams(
                    distance: ThirdPersonDistance,
                    horizontalRotation: true,
                    verticalRotation: true,
                    damping: ThirdPersonDampTime);
                break;

            case CameraModeType.TopDown:
                SetParams(
                    distance: TopDownDistance,
                    horizontalRotation: true,
                    verticalRotation: false,
                    damping: TopDownDampTime);

                SetRotation(_horizontalRotation, TopDownInclanation);

                break;

            default:
                break;
        }

        CameraMode = cameraMode;
    }

    public void BobStart()
    {
        if (_bobState == BobState.None)
            _bobState = BobState.LeftStep;
    }

    public void BobEnd()
    {
        _bobState = BobState.None;
    }

    public void CustomRotationStart(float duration, Vector3 eulers)
    {
        _customRotationDuration = duration;
        _customRotationTime = 0f;
        _fromEulers = _rotationOffset;
        _toEulers = eulers;
    }

    public void CustomRotationEnd()
    {
        _fromEulers = _rotationOffset;
        _toEulers = Vector3.zero;
        _customRotationDuration = _customRotationTime;
        _customRotationTime = 0f;
    }

    #endregion

    private Vector3 SphericalToCartezian(float polar, float elevation)
    {
        float a = Mathf.Cos(elevation);
        return new Vector3(
            a * Mathf.Cos(polar),
            Mathf.Sin(elevation),
            a * Mathf.Sin(polar));
    }
    
    private void SetParams(
        float distance,
        bool horizontalRotation,
        bool verticalRotation,
        float damping)
    {
        _distance = distance;
        _horizontalRotationEnabled = horizontalRotation;
        _verticalRotationEnabled = verticalRotation;
        _damping = damping;
    }

    private void CameraBob()
    {
        Vector3 left = (-transform.up - transform.right) * BobScale;
        Vector3 right = (-transform.up + transform.right) * BobScale;
        Vector3 mid = transform.up * BobScale;

        Vector3 targetOffset = _bobState switch
        {
            BobState.None => Vector3.zero,
            BobState.RightStep => right,
            BobState.LeftStep => left,
            BobState.RightRise => mid,
            BobState.LeftRise => mid,
            _ => Vector3.zero
        };


        _bobTime += Time.deltaTime;
        var t = _bobTime / BobDuration;

        var offset = Vector3.Lerp(_previousOffset, targetOffset, Easing.SmoothStep(t));

        if (t >= 1)
        {
            _previousOffset = targetOffset;
            _bobTime = 0;
            _bobState = _bobState switch
            {
                BobState.LeftStep => BobState.LeftRise,
                BobState.LeftRise => BobState.RightStep,
                BobState.RightStep => BobState.RightRise,
                BobState.RightRise => BobState.LeftStep,
                _ => BobState.None,
            };
        }

        transform.position += offset;
    }

    private void CustomRotationEffect()
    {
        _customRotationTime += Time.deltaTime;
        _customRotationTime = Mathf.Clamp(_customRotationTime, 0, _customRotationDuration);
        float t = _customRotationTime / _customRotationDuration;

        _rotationOffset = Vector3.Lerp(_fromEulers, _toEulers, Easing.SmoothStep(t));
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation += _rotationOffset;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    private void SetRotation(float polar, float inclanation)
    {
        _horizontalRotation = polar;

        float pi = Mathf.PI;
        _verticalRotation = Math.Clamp(inclanation, pi/2 + 0.001f, pi*3/2 - 0.001f);
    }

    private void ResolveRotation()
    {
        _direction = SphericalToCartezian(_horizontalRotation, _verticalRotation);
        transform.forward = -_direction;
    }

    private void ResolveMovement()
    {
        Vector3 from = _pivotPosition;
        Vector3 to = Target.position;
        _pivotPosition = Vector3.SmoothDamp(from, to, ref _velocity, _damping);

        transform.position = _pivotPosition + _direction * _distance;
    }

    private void ResolveEffects()
    {
        if (BobEnabled)
            CameraBob();

        CustomRotationEffect();
    }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        SwitchMode(CameraModeType.FirstPerson);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResolveRotation();
        ResolveMovement();
        ResolveEffects();
    }
}
