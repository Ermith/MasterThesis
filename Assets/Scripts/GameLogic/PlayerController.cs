using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private enum MovementState { Standing, Walking, Running, Sliding};
    private MovementState _movementState;

    public float WalkingSpeed = 2f;
    public float RunningSpeed = 5f;
    public float SlidingSpeed = 7f;
    public float WalkingRadius = 1.5f;
    public float RunningRadius = 5f;
    public float PeekAngle = 20f;
    public float PeekDuration = 0.15f;
    public Vector3 PeekOffset = new Vector3(1, -0.5f, 0);
    public CameraController Camera;
    public float StepFrequency = 5f;
    public Projectile Projectile;

    public float WalkingBob = 0.065f;
    public float WalkStepFrequency = 0.7f;
    public float RunBob = 0.15f;
    public float RunStepFrequency = 0.4f;

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
    private Animation _animation;
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
        _animation = GetComponent<Animation>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;

        SetStandingState();
    }

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        ResolveState();
        SwitchState();

        /*/
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


        // Movement
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
        //*/
        //GetComponent<MeshRenderer>().material.color = IsHidden ? Color.black : Color.white;
    }

    // State Computation
    private Vector3 _inputDir = Vector3.zero;
    private bool _runRequest = false;
    private bool _slideRequest = false;
    private float _slideTimer = 0f;
    private float _turnX, _turnY;
    private CameraModeType? _cameraModeRequest = null;
    private PeekRequest? _peekRequest = null;
    private float _slideDuration = 1.8f;

    private enum PeekRequest { Left, Right, Return }
    private bool _isPeeking;

    // Steate Parameters
    private float _movementSpeed;
    private float _stepSoundRadius;
    private float _stepPeriod;
    private float _stepTimer;
    private float _bobScale;
    private bool _canPeek;
    private bool _bobEnabled;

    private bool _dying = false;

    public void Die()
    {
        if (_dying) return;

        _dying = true;
        float duration = GameController.AudioManager.Play("DeathGrunt").clip.length;

        GameController.Instance.ExecuteAfter(() => SceneManager.LoadScene("MainMenuScene"), duration);
    }

    private void SetWalkingState()
    {
        SetState(
            movementSpeed: WalkingSpeed,
            stepSoundRadius: WalkingRadius,
            stepPeriod: WalkStepFrequency,
            bobScale: WalkingBob,
            canPeek: false,
            bobEnabled: true);

        Camera.BobStart();

        _movementState = MovementState.Walking;
    }

    private void SetRunningState()
    {
        SetState(
            movementSpeed: RunningSpeed,
            stepSoundRadius: RunningRadius,
            stepPeriod: RunStepFrequency,
            bobScale: RunBob,
            canPeek: false,
            bobEnabled: true);

        Camera.BobStart();

        _movementState = MovementState.Running;
    }

    private void SetStandingState()
    {
        Camera.BobEnd();

        SetState(
            canPeek: true
            );


        _movementState = MovementState.Standing;
    }


    private void SetSlidingState()
    {
        SetState(
            stepPeriod: WalkStepFrequency,
            stepSoundRadius: WalkingRadius,
            movementSpeed: SlidingSpeed,
            bobEnabled: false,
            canPeek: false
            );

        _movementState = MovementState.Sliding;
        _slideTimer = 0;

        if (Camera.CameraMode == CameraModeType.FirstPerson || Camera.CameraMode == CameraModeType.ThirdPerson)
            Camera.CustomOffsetStart(0.2f, Vector3.down, false);

        if (Camera.CameraMode == CameraModeType.FirstPerson)
            Camera.CustomRotationStart(0.2f, new Vector3(10, 0, 10));

        GameController.AudioManager.Play("Slide");
        _animation.Play();
    }

    private void SetState(
        float movementSpeed = 1f,
        float stepSoundRadius = 1f,
        float stepPeriod = 1f,
        float bobScale = 1f,
        bool canPeek = false,
        bool bobEnabled = false)
    {
        _movementSpeed = movementSpeed;
        _stepSoundRadius = stepSoundRadius;
        _stepPeriod = stepPeriod;
        _bobScale = bobScale;
        _canPeek = canPeek;
        _bobEnabled = bobEnabled;
        Camera.BobEnabled = _bobEnabled;
    }

    private void SwitchState()
    {
        switch (_movementState)
        {
            case MovementState.Standing:
                if (_inputDir != Vector3.zero && _runRequest) { SetRunningState();  }
                if (_inputDir != Vector3.zero && !_runRequest) { SetWalkingState();  }
                if (_movementState != MovementState.Standing) EndPeek();
                break;


            case MovementState.Walking:
                if (_inputDir == Vector3.zero) { SetStandingState(); _stepTimer = 0f; }
                else if (_runRequest) SetRunningState();
                break;


            case MovementState.Running:
                if (_inputDir == Vector3.zero) { SetStandingState(); _stepTimer = 0f; }
                else if (!_runRequest) SetWalkingState();
                else if (_slideRequest) { SetSlidingState(); _stepTimer = 0f; }
                break;

            case MovementState.Sliding:
                if (_slideTimer >= _slideDuration)
                {
                    SetStandingState();
                    Camera.CustomOffsetEnd();
                    Camera.CustomRotationEnd();
                }
                break;

            default:
                break;
        }
    }

    private void GatherInput()
    {
        // reset input
        _turnX = 0f;
        _turnY = 0f;
        _runRequest = false;
        _slideRequest = false;
        _inputDir = Vector3.zero;
        _peekRequest = null;

        if (GameController.IsPaused)
            return;

        // Camera
        if (Input.GetKeyDown(KeyCode.F1)) _cameraModeRequest = CameraModeType.FirstPerson;
        if (Input.GetKeyDown(KeyCode.F2)) _cameraModeRequest = CameraModeType.TopDown;
        if (Input.GetKeyDown(KeyCode.F3)) _cameraModeRequest = CameraModeType.ThirdPerson;
        _turnX = -Input.GetAxis("Mouse X");
        _turnY = Input.GetAxis("Mouse Y");

        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.F3)) FindObjectOfType<LevelCamera>().GetComponent<Camera>().enabled = false;
        if (Input.GetKeyDown(KeyCode.F4)) FindObjectOfType<LevelCamera>().GetComponent<Camera>().enabled = true;

        //Movement
        if (Input.GetKey(KeyCode.W)) _inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.A)) _inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.S)) _inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.D)) _inputDir += Vector3.right;
        _inputDir.Normalize();
        _runRequest = Input.GetKey(KeyCode.LeftShift);
        _slideRequest = Input.GetKey(KeyCode.Space);

        // Peeking
        if (!_canPeek)
            return;

        if (Input.GetKeyDown(KeyCode.E)) { _peekRequest = PeekRequest.Right; }
        if (Input.GetKeyDown(KeyCode.Q)) { _peekRequest = PeekRequest.Left; }
        if (Input.GetKeyUp(KeyCode.E)) { _peekRequest = PeekRequest.Return; }
        if (Input.GetKeyUp(KeyCode.Q)) { _peekRequest = PeekRequest.Return; }
    }

    private void ResolveState()
    {
        ResolveCamera();

        string stepSound = "SmallStep";
        if (_movementState == MovementState.Running) stepSound = "BigStep";

        if (_movementState == MovementState.Walking || _movementState == MovementState.Running)
        {
            ResolveMovement();
            _stepTimer += Time.deltaTime;
            if (_stepTimer > _stepPeriod)
            {
                StepSound(stepSound, _stepSoundRadius);
                _stepTimer %= _stepPeriod;
            }
        }

        if (_movementState == MovementState.Sliding)
            ResolveSliding();

        if (_peekRequest != null)
        {
            if (_peekRequest.Value == PeekRequest.Left)
                StartPeek(true);

            if (_peekRequest == PeekRequest.Right)
                StartPeek(false);

            if (_peekRequest == PeekRequest.Return)
                EndPeek();
        }
    }

    private void ResolveCamera()
    {
        if (_cameraModeRequest != null)
            Camera.SwitchMode(_cameraModeRequest.Value);

        Camera.Rotate(_turnX, _turnY);
        SwitchVisual(Camera.CameraMode != CameraModeType.FirstPerson);
        Camera.BobEnabled = Camera.CameraMode == CameraModeType.FirstPerson && _bobEnabled;
        Camera.BobDuration = _stepPeriod / 2f;
        Camera.BobScale = _bobScale;
    }

    private void ResolveMovement()
    {
        Vector3 cameraDir = Camera.GetGroundDirection();
        Vector3 camearaRight = -Vector3.Cross(cameraDir, Vector3.up);
        Vector3 direction = cameraDir * _inputDir.z + camearaRight * _inputDir.x;

        _characterContrroller.SimpleMove(direction * _movementSpeed);
        transform.forward = direction;
    }

    private void ResolveSliding()
    {
        float t = Mathf.Clamp01(1 - _slideTimer / _slideDuration);
        _characterContrroller.SimpleMove(transform.forward * t * _movementSpeed);

        _slideTimer += Time.deltaTime;
    }

    private void StepSound(string sound, float range)
    {
        GameController.AudioManager.AudibleEffect(gameObject, transform.position, range);
        GameController.AudioManager.Play(sound);
    }

    private void SwitchVisual(bool visible)
    {
        _visual.shadowCastingMode = visible
            ? UnityEngine.Rendering.ShadowCastingMode.On
            : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    private void StartPeek(bool left)
    {
        if (Camera.CameraMode == CameraModeType.TopDown)
            return;

        float peekAngle = left ? PeekAngle : -PeekAngle;
        Vector3 offset = PeekOffset;
        if (left) offset.x *= -1;

        if (Camera.CameraMode == CameraModeType.FirstPerson)
            Camera.CustomRotationStart(PeekDuration, new Vector3(0, 0, peekAngle));

        if (Camera.CameraMode == CameraModeType.ThirdPerson)
        {
            offset.x *= 1.5f;
            offset.z = 2;
            offset.y = 0;
        }

        Camera.CustomOffsetStart(PeekDuration, offset, true);
        _isPeeking = true;
    }
    private void EndPeek()
    {
        if (!_isPeeking) return;

        _isPeeking = false;
        Camera.CustomRotationEnd();
        Camera.CustomOffsetEnd();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("Collision!");
        if (hit.collider.tag == "Trap")
            Debug.Log("Trap");
    }
}