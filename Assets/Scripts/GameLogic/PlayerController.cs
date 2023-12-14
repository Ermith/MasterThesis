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
    public float PeekAngle = 20f;
    public float PeekDuration = 0.15f;
    public Vector3 PeekOffset = new Vector3(1, -0.5f, 0);
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
    private Renderer _visual;
    private Transform _viewPoint;
    private Vector3 _viewPointPosition;
    private Vector3 _viewPointOffset;
    private Vector3 _viewPointOffsetFrom;
    private Vector3 _viewPointOffsetTarget;
    private float _peekTime;

    // Start is called before the first frame update
    void Start()
    {
        _visual = transform.Find("Visual").GetComponent<Renderer>();
        _viewPoint = transform.Find("ViewPoint");
        _viewPointPosition = _viewPoint.localPosition;
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
        if (Input.GetKeyDown(KeyCode.F1)) Camera.SwitchMode(CameraModeType.FirstPerson);
        if (Input.GetKeyDown(KeyCode.F2)) Camera.SwitchMode(CameraModeType.TopDown);
        if (Input.GetKeyDown(KeyCode.F3)) Camera.SwitchMode(CameraModeType.ThirdPerson);

        SwitchVisual(Camera.CameraMode != CameraModeType.FirstPerson);
        Camera.BobEnabled = Camera.CameraMode == CameraModeType.FirstPerson;

        // peeking
        Vector3 peekLeft = new(-PeekOffset.x, PeekOffset.y, PeekOffset.z);
        if (Input.GetKeyDown(KeyCode.E)) { StartPeek(false); }
        if (Input.GetKeyDown(KeyCode.Q)) { StartPeek(true); }
        if (Input.GetKeyUp(KeyCode.E)) { EndPeek(); }
        if (Input.GetKeyUp(KeyCode.Q)) { EndPeek(); }
        ResolvePeek();


        //Movement
        Vector3 inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;
        inputDir.Normalize();

        float speed = WalkingSpeed;
        float radius = WalkingRadius;
        StepFrequency = 0.7f;
        Camera.BobDuration = 0.35f;
        Camera.BobScale = 0.075f;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = RunningSpeed;
            radius = RunningRadius;
            StepFrequency = 0.4f;
            Camera.BobDuration = 0.2f;
            Camera.BobScale = 0.15f;
        }

        bool isMoving = inputDir != Vector3.zero;

        if (isMoving) Camera.BobStart(); else Camera.BobEnd();
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
        transform.forward = cameraDir;

        if (Input.GetKeyDown(KeyCode.V))
        {
            Projectile projectile = Instantiate(Projectile);
            projectile.Shoot(transform.position, cameraDir, (Vector3 pos, GameObject target) =>
            {
                GameController.AudioManager.AudibleEffect(projectile.gameObject, pos, 10f);
            });
        }

        _lineRenderer.enabled = false;
        if (Input.GetKey(KeyCode.T))
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

        //GetComponent<MeshRenderer>().material.color = IsHidden ? Color.black : Color.white;
    }

    private void CreateSound(float range)
    {
        GameController.AudioManager.AudibleEffect(gameObject, transform.position + Vector3.down * _characterContrroller.height / 2, range);
    }

    private void SwitchVisual(bool visible)
    {
        _visual.shadowCastingMode = visible
            ? UnityEngine.Rendering.ShadowCastingMode.On
            : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    private void StartPeek(bool left)
    {
        _peekTime = 0f;
        float peekAngle = left ? PeekAngle : -PeekAngle;
        Camera.CustomRotationStart(PeekDuration, new Vector3(0, 0, peekAngle));
        Vector3 offset = PeekOffset;
        if (left) offset.x *= -1;

        _viewPointOffsetFrom = _viewPointOffset;
        _viewPointOffsetTarget = offset;
    }

    private void ResolvePeek()
    {
        _peekTime += Time.deltaTime;
        _peekTime = Mathf.Clamp(_peekTime, 0, PeekDuration);
        float t = _peekTime / PeekDuration;

        _viewPointOffset = Vector3.Lerp(_viewPointOffset, _viewPointOffsetTarget, Easing.SmoothStep(t));
        _viewPoint.localPosition = _viewPointPosition + _viewPointOffset;
    }

    private void EndPeek()
    {
        _peekTime = PeekDuration - _peekTime;
        _viewPointOffsetTarget = Vector3.zero;
        _viewPointOffsetFrom = _viewPointOffset;
        Camera.CustomRotationEnd();
    }
}