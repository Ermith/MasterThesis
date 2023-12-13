using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float WalkingSpeed = 2f;
    public float RunningSpeed = 5f;
    public float WalkingRadius = 1.5f;
    public float RunningRadius = 5f;
    public CameraController Camera;
    public float StepFrequency = 5f;
    public Projectile Projectile;

    [HideInInspector]
    public bool IsHidden = false;
    [HideInInspector]
    public bool Refuge = false;


    Vector3 mousePosition;
    CharacterController _characterContrroller;
    private bool _wasMoving = false;
    private float walkTime = 0f;
    private LineRenderer _lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _characterContrroller = GetComponent<CharacterController>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera
        if (Input.GetKey(KeyCode.F1)) Camera.SwitchMode(CameraModeType.FirstPerson);
        if (Input.GetKey(KeyCode.F2)) Camera.SwitchMode(CameraModeType.TopDown);
        if (Input.GetKey(KeyCode.F3)) Camera.SwitchMode(CameraModeType.ThirdPerson);

        //Movement
        Vector3 inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;
        inputDir.Normalize();

        float speed = WalkingSpeed;
        float radius = WalkingRadius;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = RunningSpeed;
            radius = RunningRadius;
        }

        bool isMoving = inputDir != Vector3.zero;

        if (isMoving && !_wasMoving) walkTime = 0;
        if (isMoving) walkTime += Time.deltaTime;
        if (isMoving && walkTime > StepFrequency)
        {
            CreateSound(radius);
            walkTime %= StepFrequency;
        }

        if (!isMoving && _wasMoving)
            CreateSound(radius);

        _wasMoving = isMoving;


        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        Camera.Rotate(-x, y);
        Vector3 cameraDir = Camera.GetGroundDirection();
        Vector3 camearaRight = -Vector3.Cross(cameraDir, Vector3.up);

        Vector3 direction = cameraDir * inputDir.z + camearaRight * inputDir.x;
        _characterContrroller.SimpleMove(direction * speed);

        if (Input.GetKeyDown(KeyCode.V))
        {
            Projectile projectile = Instantiate(Projectile);
            projectile.Shoot(transform.position, cameraDir, (Vector3 pos, GameObject target) =>
            {
                GameController.AudioManager.AudibleEffect(projectile.gameObject, pos, 10f);
            });
        }

        _lineRenderer.enabled = false;
        if (Input.GetKey(KeyCode.Q))
        {
            float maxDistance = 1000;
            bool hit = Physics.Raycast(transform.position, Camera.GetGroundDirection(), out RaycastHit hitInfo, maxDistance);
            if (!hit) return;


            _lineRenderer.enabled = true;
            _lineRenderer.SetPositions(
                new Vector3[] { transform.position, hitInfo.point }
            );
        }

        IsHidden = Refuge;
        Refuge = false;

        GetComponent<MeshRenderer>().material.color = IsHidden ? Color.black : Color.white;
    }

    private void CreateSound(float range)
    {
        GameController.AudioManager.AudibleEffect(gameObject, transform.position + Vector3.down * _characterContrroller.height / 2, range);
    }
}