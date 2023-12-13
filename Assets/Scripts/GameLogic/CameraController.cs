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
                    distance: 10f,
                    horizontalRotation: true,
                    verticalRotation: true,
                    damping: 0.1f);
                break;

            case CameraModeType.TopDown:
                SetParams(
                    distance: 20f,
                    horizontalRotation: true,
                    verticalRotation: false,
                    damping: 0.1f);

                SetRotation(_horizontalRotation, TopDownInclanation);

                break;

            default:
                break;
        }

        CameraMode = cameraMode;
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

    private void Awake()
    {
        SwitchMode(CameraModeType.ThirdPerson);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResolveRotation();
        ResolveMovement();
    }
}
