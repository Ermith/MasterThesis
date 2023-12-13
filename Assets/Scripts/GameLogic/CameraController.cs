using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraModeType { FirstPerson, ThirdPerson, TopDown }
public class CameraController : MonoBehaviour
{
    public Transform Target;
    public float TopDownInclanation = -4f;
    public float Distance = 20;
    private Vector3 _direction = Vector3.back + Vector3.up;
    private Vector3 _velocity = Vector3.zero;
    private float _polar, _inclanation;
    private bool _horizontalRotation = true, _verticalRotation = true;
    private float _damping = 0.5f;

    public CameraModeType CameraMode { get; set; } = CameraModeType.FirstPerson;

    private Vector3 SphericalToCartezian(float polar, float elevation)
    {
        float a = Mathf.Cos(elevation);
        return new Vector3(
            a * Mathf.Cos(polar),
            Mathf.Sin(elevation),
            a * Mathf.Sin(polar));
    }

    public void Rotate(float horizontal, float vertical, float speed = 1)
    {
        if (!_verticalRotation) vertical = 0f;
        if (!_horizontalRotation) horizontal = 0f;

        speed *= 0.1f; // 1 is too fast
        SetRotation(horizontal * speed, vertical * speed);
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
                    damping: 0.5f);
                break;

            case CameraModeType.TopDown:
                SetParams(
                    distance: 20f,
                    horizontalRotation: true,
                    verticalRotation: false,
                    damping: 0.5f);

                SetRotation(_polar, TopDownInclanation);
                
                break;

            default:
                break;
        }
    }

    private void SetParams(
        float distance,
        bool horizontalRotation,
        bool verticalRotation,
        float damping)
    {
        Distance = distance;
        _horizontalRotation = horizontalRotation;
        _verticalRotation = verticalRotation;
        _damping = damping;
    }

    private void ResolveMovement()
    {
        Vector3 from = transform.position;
        Vector3 to = Target.position;
        Vector3 currentPos = Vector3.SmoothDamp(from, to, ref _velocity,_damping);

        transform.position = currentPos + _direction * Distance;
    }

    private void SetRotation(float polar, float inclanation)
    {
        _polar = polar;
        _inclanation = inclanation;
    }

    private void ResolveRotation()
    {
        _direction = SphericalToCartezian(_polar, _inclanation);
        transform.forward = -_direction;
    }

    // Update is called once per frame
    void Update()
    {
        ResolveMovement();
        ResolveRotation();
    }
}
