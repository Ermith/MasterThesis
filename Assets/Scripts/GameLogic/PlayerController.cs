using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

/*/
[RequireComponent(typeof(CharacterController))]
public class KPlayerController : MonoBehaviour
{
    private enum MovementState { Standing, Walking, Running, Sliding };
    private MovementState _movementState;

    public float WalkingSpeed = 2f;
    public float SprintingSpeed = 5f;
    public float SlidingSpeed = 7f;
    public float WalkingRadius = 1.5f;
    public float RunningRadius = 5f;
    public float PeekAngle = 20f;
    public float PeekDuration = 0.15f;
    public Vector3 PeekOffset = new Vector3(1, -0.5f, 0);
    public CameraController Camera;
    public float StepFrequency = 5f;
    public Projectile Projectile;
    public float MouseSensitivity = 0.7f;

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
    private Transform _aimPoint;
    private Animation _animation;
    private Vector3 _viewPointPosition;
    private Vector3 _viewPointOffset;
    private Vector3 _viewPointOffsetFrom;
    private Vector3 _viewPointOffsetTarget;
    private float _peekTime;

    private List<IKey> _ownedKeys = new();

    private IMovementState _state;

    // Start is called before the first frame update
    void Start()
    {
        _visual = transform.Find("Visual").GetComponent<Renderer>();
        _viewPoint = transform.Find("ViewPoint");
        _aimPoint = transform.Find("AimPoint");
        _viewPointPosition = _viewPoint.localPosition;
        _characterContrroller = GetComponent<CharacterController>();
        _animation = GetComponent<Animation>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;

        SetStandingState();
        _standingState = new Standing();
        _walkingState = new Walking(WalkingSpeed, WalkStepFrequency, WalkingRadius, WalkingBob);
        _sprintingState = new Sprinting(SprintingSpeed, RunStepFrequency, RunningRadius, RunBob);
        _slidingState = new Sliding(transform.forward, SlidingSpeed, _slideDuration);
        _state = _walkingState;
    }

    private Standing _standingState;
    private Walking _walkingState;
    private Sprinting _sprintingState;
    private Sliding _slidingState;

    private void DetermineState()
    {
        if (!_state.CanSwitch) return;

        if (_state is Standing)
        {
            if (_inputDir != Vector3.zero)
            {
                if (_sprintRequest)
                    EnterState(_sprintingState);
                else
                    EnterState(_walkingState);

                return;
            }
        }

        if (_state is Walking)
        {
            if (_inputDir == Vector3.zero)
            {
                EnterState(_standingState);
                return;
            }

            if (_sprintRequest)
            {
                EnterState(_sprintingState);
                return;
            }
        }

        if (_state is Sprinting)
        {
            if (_slideRequest)
            {
                EnterState(_slidingState);
                return;
            }

            if (_inputDir == Vector3.zero)
            {
                EnterState(_standingState);
                return;
            }

            if (!_sprintRequest)
            {
                EnterState(_walkingState);
                return;
            }
        }

        if (_state is Sliding)
        {
            if (_inputDir == Vector3.zero)
            {
                EnterState(_standingState);
                return;
            }

            if (_sprintRequest)
            {
                EnterState(_sprintingState);
                return;
            }

            EnterState(_walkingState);
            return;
        }
    }

    private void EnterState(IMovementState state)
    {
        //_state.Exit(this);
        //state.Reset(this);
        _state = state;
    }

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        DetermineState();
        UpdateState();
    }

    // State Computation
    public Vector3 _inputDir = Vector3.zero;
    private bool _sprintRequest = false;
    private bool _slideRequest = false;
    private bool _aimRequest = false;
    private bool _shootRequest = false;
    private bool _useRequest = false;
    private float _slideTimer = 0f;
    private float _turnX, _turnY;
    private CameraModeType? _cameraModeRequest = null;
    private PeekRequest? _peekRequest = null;
    private float _slideDuration = 1.4f;

    public enum PeekRequest { Left, Right, Return }
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

        GameController.ExecuteAfter(GameController.MainMenu, duration);
    }

    private void SetWalkingState()
    {
        SetState(
            movementSpeed: WalkingSpeed,
            stepSoundRadius: WalkingRadius,
            stepPeriod: WalkStepFrequency,
            bobScale: WalkingBob,
            canPeek: true,
            bobEnabled: true);

        Camera.BobStart();

        _movementState = MovementState.Walking;
    }

    private void SetRunningState()
    {
        SetState(
            movementSpeed: SprintingSpeed,
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

        if (Camera.Mode == CameraModeType.FirstPerson || Camera.Mode == CameraModeType.ThirdPerson)
            Camera.CustomOffsetStart(0.2f, Vector3.down, false);

        if (Camera.Mode == CameraModeType.FirstPerson)
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
                if (_inputDir != Vector3.zero && _sprintRequest) { SetRunningState(); }
                if (_inputDir != Vector3.zero && !_sprintRequest) { SetWalkingState(); }
                if (_movementState != MovementState.Standing) EndPeek();
                break;


            case MovementState.Walking:
                if (_inputDir == Vector3.zero) { SetStandingState(); _stepTimer = 0f; } else if (_sprintRequest) SetRunningState();
                break;


            case MovementState.Running:
                if (_inputDir == Vector3.zero) { SetStandingState(); _stepTimer = 0f; } else if (!_sprintRequest) SetWalkingState();
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

    private void UpdateMovement()
    {
        Vector3 movement = _state.GetMovement(
            _inputDir,
            _characterContrroller.velocity,
            Camera);


        _characterContrroller.SimpleMove(movement * _state.MovementSpeed);
        transform.forward = _state.GetRotation(movement, transform.forward, Camera);

        if (!_state.BobEnabled)
        {
            Camera.BobEnd();
            return;
        }


        //Camera.BobScale = _state.BobScale;
        //Camera.BobDuration = _state.StepPeriod;
        //
        //if (!Camera.Bobbing())
        //{
        //    Camera.BobStart();
        //    Debug.Log("REALLY BOBBING");
        //}
        //
        //if (Camera.BobStep)
        //    GameController.AudioManager.AudibleEffect(
        //        gameObject,
        //        transform.position,
        //        _state.StepRange);
    }


    private void UpdateState()
    {
        _state.Update();
        UpdateCamera();
        UpdateMovement();
        UpdateShooting();
        UpdateInteraction();
        UpdatePeeking();

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

        if (_movementState != MovementState.Running)
            Shooting();

        if (_peekRequest != null)
        {
            if (_peekRequest.Value == PeekRequest.Left)
                StartPeek(true);

            if (_peekRequest == PeekRequest.Right)
                StartPeek(false);

            if (_peekRequest == PeekRequest.Return)
                EndPeek();
        }

        Use();
    }

    private void UpdateCamera()
    {
        if (_cameraModeRequest != null)
            Camera.SwitchMode(_cameraModeRequest.Value);

        Camera.Rotate(_turnX * MouseSensitivity, _turnY * MouseSensitivity);
        SwitchVisual(Camera.Mode != CameraModeType.FirstPerson);

        Camera.BobEnabled = Camera.Mode == CameraModeType.FirstPerson;

        if (!_state.BobEnabled)
        {
            Camera.BobEnd();
            return;
        }

        Camera.BobDuration = _state.StepPeriod / 2f;
        Camera.BobScale = _state.BobScale;
        if (!Camera.Bobbing())
            Camera.BobStart();

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

    private void UpdateShooting()
    {
        _lineRenderer.enabled = false;
        if (_aimRequest && _state.CanShoot)
        {
            float maxDistance = 1000;
            bool hit = Physics.Raycast(_aimPoint.position, Camera.GetGroundDirection(), out RaycastHit hitInfo, maxDistance);
            if (!hit) return;

            _lineRenderer.enabled = true;
            _lineRenderer.SetPositions(
                new Vector3[] { _aimPoint.position, hitInfo.point }
            );
        }

        if (_shootRequest)
        {
            Projectile projectile = Instantiate(Projectile);
            projectile.Shoot(_aimPoint.position, Camera.GetGroundDirection(), (Vector3 pos, GameObject target) =>
            {
                GameController.AudioManager.AudibleEffect(projectile.gameObject, pos, 10f);
                GameController.AudioManager.Play("GlassShatter", position: pos, volume: 0.5f);
            });
        }
    }

    private void UpdateInteraction()
    {
        if (!_state.CanInteract)
        {
            GameController.HideInteraction();
            return;
        }

        Vector3 direction = Camera.Mode == CameraModeType.TopDown
            ? Camera.GetGroundDirection()
            : Camera.transform.forward;

        bool hit = Physics.Raycast(
            _viewPoint.position,
            direction,
            out RaycastHit hitInfo,
            maxDistance: 2f);

        if (!hit)
        {
            GameController.HideInteraction();
            return;
        }

        var interactable =
            hitInfo.collider.gameObject.transform
            .GetComponentInParent<IInteractableObject>();

        if (interactable == null || !interactable.CanInteract)
        {
            GameController.HideInteraction();
            return;
        };

        GameController.ShowInteraction(interactable.InteractionPrompt());
        //if (_useRequest) interactable.Interact(this);
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

    private void UpdatePeeking()
    {
        if (!_state.CanPeek)
        {
            EndPeek();
            return;
        }

        if (_peekRequest == null)
            return;

        Debug.Log(_peekRequest.Value);

        switch (_peekRequest.Value)
        {
            case PeekRequest.Left:
                StartPeek(true);
                break;

            case PeekRequest.Right:
                StartPeek(false);
                break;

            case PeekRequest.Return:
                EndPeek();
                break;
        }

    }
    private void StartPeek(bool left)
    {
        if (Camera.Mode == CameraModeType.TopDown)
            return;

        float peekAngle = left ? PeekAngle : -PeekAngle;
        Vector3 offset = PeekOffset;
        if (left) offset.x *= -1;

        if (Camera.Mode == CameraModeType.FirstPerson)
            Camera.CustomRotationStart(PeekDuration, new Vector3(0, 0, peekAngle));

        if (Camera.Mode == CameraModeType.ThirdPerson)
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

    public void SetHidden(bool hidden)
    {
        IsHidden = hidden;
        Refuge = hidden;

        if (hidden)
            GetComponentInChildren<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
        else
            GetComponentInChildren<MeshRenderer>().material.color = Color.white;
    }

    public bool HasKey(ILock @lock)
    {
        foreach (var key in _ownedKeys)
            if (key.Locks.Contains(@lock))
                return true;

        return false;
    }

    public void PickupKey(IKey key)
    {
        _ownedKeys.Add(key);
        GameController.AudioManager.Play("Jingle");
    }


    private void GatherInput()
    {
        // reset input
        _turnX = 0f;
        _turnY = 0f;
        _sprintRequest = false;
        _slideRequest = false;
        _inputDir = Vector3.zero;
        _peekRequest = null;
        _aimRequest = false;
        _shootRequest = false;
        _useRequest = false;
        _cameraModeRequest = null;

        if (GameController.IsPaused)
            return;

        // Camera
        if (Input.GetKeyDown(KeyCode.F1)) _cameraModeRequest = CameraModeType.FirstPerson;
        if (Input.GetKeyDown(KeyCode.F2)) _cameraModeRequest = CameraModeType.TopDown;
        if (Input.GetKeyDown(KeyCode.F3)) _cameraModeRequest = CameraModeType.ThirdPerson;
        _turnX = -Input.GetAxis("Mouse X");
        _turnY = Input.GetAxis("Mouse Y");

        //if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.F3)) FindObjectOfType<LevelCamera>().GetComponent<Camera>().enabled = false;
        if (Input.GetKeyDown(KeyCode.F4)) FindObjectOfType<LevelCamera>().GetComponent<Camera>().enabled = true;

        //Movement
        if (Input.GetKey(KeyCode.W)) _inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.A)) _inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.S)) _inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.D)) _inputDir += Vector3.right;
        _inputDir.Normalize();
        _sprintRequest = Input.GetKey(KeyCode.LeftShift);
        _slideRequest = Input.GetKey(KeyCode.Space);

        // Peeking
        if (_canPeek)
        {
            if (Input.GetKeyDown(KeyCode.E)) { _peekRequest = PeekRequest.Right; }
            if (Input.GetKeyDown(KeyCode.Q)) { _peekRequest = PeekRequest.Left; }
            if (Input.GetKeyUp(KeyCode.E)) { _peekRequest = PeekRequest.Return; }
            if (Input.GetKeyUp(KeyCode.Q)) { _peekRequest = PeekRequest.Return; }
        }

        // Aiming and Shooting
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.T)) _aimRequest = true;
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.V)) && _aimRequest) _shootRequest = true;

        // Use
        if (Input.GetKeyDown(KeyCode.F)) _useRequest = true;
    }

    public void SetCameraMode(CameraModeType cameraMode)
    {
        Camera.SwitchMode(cameraMode);
        SwitchVisual(cameraMode != CameraModeType.FirstPerson);
        Camera.BobEnabled = cameraMode == CameraModeType.FirstPerson && _bobEnabled;
        Camera.BobDuration = _stepPeriod / 2f;
        Camera.BobScale = _bobScale;
    }

    public void RotateCamera(float yaw, float pitch)
    {
        Camera.Rotate(yaw * MouseSensitivity, pitch * MouseSensitivity);
    }

    public void SetMovementDirection(Vector3 inputDir, bool sprint = false, bool slide = false)
    {
        _inputDir = inputDir;
        _sprintRequest = sprint;
        _slideRequest = slide;
    }

    public void Peek(PeekRequest request)
    {
        _peekRequest = request;
    }
}//*/