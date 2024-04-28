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

    public CameraModeType Mode { get; private set; } = CameraModeType.FirstPerson;

    // Camera Mode Params
    private float _distance = 20;
    private float _horizontalRotation;
    private float _verticalRotation = 3f;
    private bool _horizontalRotationEnabled = true;
    private bool _verticalRotationEnabled = true;
    private float _damping = 0.1f;
    private Coroutine _coroutine;

    // Internal Results
    private Vector3 _direction;
    private Vector3 _velocity;
    private Vector3 _pivotPosition;

    // Camera Bob
    private enum BobState { None, RightStep, RightRise, LeftStep, LeftRise }
    public float BobDuration = 0.35f;
    public float BobScale = 0.075f;
    public bool BobEnabled = true;
    private BobState _bobState = BobState.None;
    private float _bobTime = 0f;
    private Vector3 _previousOffset;
    private Vector3 _bobOffset;
    public bool BobStep { get; private set; }

    // Custom rotation
    private float _customRotationDuration = 1f;
    private float _customRotationTime = 0f;
    private Vector3 _fromEulers;
    private Vector3 _toEulers;
    private Vector3 _rotationOffset;

    // Custom offset
    private float _customOffsetDuration = 1f;
    private float _customOffsetTime = 0f;
    private Vector3 _fromOffset;
    private Vector3 _toOffset;
    private Vector3 _offset;
    private bool _isLocalOffset;
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

    public Vector3 GetGroundRotation()
    {
        return Quaternion.LookRotation(GetGroundDirection()).eulerAngles;
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

        Mode = cameraMode;
    }

    public void BobStart()
    {
        if (_bobState == BobState.None)
        {
            _bobState = BobState.LeftStep;
        }
        _bobState = BobState.LeftStep;
        _previousOffset = _bobOffset;
        _bobTime = 0f;
    }

    public void BobEnd()
    {
        _bobState = BobState.None;
    }

    public bool Bobbing() => _bobState != BobState.None;

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

    public void CustomOffsetStart(float duration, Vector3 offset, bool local)
    {
        _customOffsetDuration = duration;
        _customOffsetTime = 0f;
        _fromOffset = _offset;
        _toOffset = offset;
        _isLocalOffset = local;
    }

    public void CustomOffsetEnd()
    {
        _fromOffset = _offset;
        _toOffset = Vector3.zero;
        _customOffsetDuration = _customOffsetTime;
        _customOffsetTime = 0f;
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
        if (_coroutine != null) { StopCoroutine(_coroutine); }
        _coroutine = StartCoroutine(DistanceCoroutine(distance, 0.2f));
        _horizontalRotationEnabled = horizontalRotation;
        _verticalRotationEnabled = verticalRotation;
        _damping = damping;
    }

    private void CameraBob()
    {
        BobStep = false;
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

        var t = Mathf.Clamp01(_bobTime / BobDuration);
        _bobTime += Time.deltaTime;

        if (_bobState == BobState.LeftStep || _bobState == BobState.RightStep)
            t = Easing.SmoothStart(t);
        else
            t = Easing.SmoothStep(t);

        _bobOffset = Vector3.Lerp(_previousOffset, targetOffset, t);


        if (t >= 1)
        {
            BobStep = _bobState == BobState.RightStep || _bobState == BobState.LeftStep;
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

        transform.position += _bobOffset;
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

    private void CustomOffsetEffect()
    {
        _customOffsetTime += Time.deltaTime;
        _customOffsetTime = Mathf.Clamp(_customOffsetTime, 0, _customOffsetDuration);
        float t = _customOffsetTime / _customOffsetDuration;

        _offset = Vector3.Lerp(_fromOffset, _toOffset, Easing.SmoothStep(t));
        Vector3 currentPosition = transform.position;

        if (_isLocalOffset)
            currentPosition += 
                _offset.x * transform.right +
                _offset.y * transform.up +
                _offset.z * transform.forward;
        else
            currentPosition += _offset;

        transform.position = currentPosition;
    }

    private void SetRotation(float polar, float inclanation)
    {
        _horizontalRotation = polar;

        float pi = Mathf.PI;
        _verticalRotation = Math.Clamp(inclanation, pi / 2 + 0.001f, pi * 3 / 2 - 0.001f);
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
        CustomOffsetEffect();
    }

    // Awake is called when the script instance is being loaded
    private void Start()
    {
        transform.forward = Target.forward;
        transform.position = Target.position;
        SwitchMode(CameraModeType.FirstPerson);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResolveRotation();
        ResolveMovement();
        ResolveEffects();
        ResolveCollision();
    }

    private IEnumerator DistanceCoroutine(float distance, float time)
    {
        float start = _distance;
        float timer = 0f;

        while (timer < time)
        {
            float t = Easing.SmoothStep(timer / time);

            _distance = start + t * (distance - start);
            timer += Time.deltaTime;
            yield return null;
        }

        _distance = distance;
        _coroutine = null;
    }

    private void ResolveCollision()
    {
        if (Mode == CameraModeType.TopDown)
            return;

        var dir = transform.position - Target.position;
        int mask = ~((1 << 2) | (1 << 6));
        if (Physics.Raycast(Target.position, dir.normalized, out RaycastHit hitInfo, dir.magnitude, mask))
        {
            transform.position = Target.position + dir.normalized * (hitInfo.distance - 1f);
        }

    }
}
