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

        Func<float, float> ease = _bobState switch
        {
            BobState.LeftRise | BobState.RightRise => (float t) => t* t,
            _ => (float t) => t
        };


        _bobTime += Time.deltaTime;
        var t = _bobTime / BobDuration;
        float smoothStart = t * t;
        float smoothEnd = 1 - (1 - t) * (1 - t);
        float mix = (1 - t) * smoothStart  + t * smoothEnd;

        var offset = Vector3.Lerp(_previousOffset, targetOffset, mix);

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

    private void ResolveEffects()
    {
        if (BobEnabled)
            CameraBob();
    }

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
