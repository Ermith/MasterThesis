using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraModeType { FirstPerson, ThirdPerson, TopDown }
public class CameraController : MonoBehaviour
{
    public Transform Target;
    public float TopDownElevation = -4f;
    public float Distance = 20;
    private Vector3 _direction = Vector3.back + Vector3.up;
    private float polar, elevation;

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
        speed *= 0.1f; // 1 is too fast
        polar += horizontal * speed;
        elevation = (CameraMode == CameraModeType.TopDown)
            ? TopDownElevation
            : elevation + vertical * speed;

        _direction = SphericalToCartezian(polar, elevation);
    }

    public Vector3 GetGroundDirection()
    {
        return new Vector3(
            -_direction.x,
            0,
            -_direction.z
            ).normalized;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Target.position;
        if (CameraMode != CameraModeType.FirstPerson)
            transform.position = Target.position + _direction * Distance;

        transform.forward = -_direction;
    }
}
