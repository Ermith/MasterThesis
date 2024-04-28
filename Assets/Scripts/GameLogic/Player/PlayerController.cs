using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Exposed Parameters
    public float MouseSensitivity = 0.7f;

    public float WalkingSpeed = 3f;
    public float RunningSpeed = 10f;
    public float SlidingSpeed = 15f;
    public float SlidingDuration = 1.3f;

    public float RunningStepRadius = 10f;
    public float WalkingStepRadius = 1.5f;

    public float WalkingStepPeriod = 0.35f;
    public float RunningStepPeriod = 0.15f;

    public float WalkingBobScale = 0.03f;
    public float RunningBobScale = 0.07f;

    public float PeekAngle = 20f;
    public float PeekTime = 0.15f;
    public Vector3 PeekOffset = new Vector3(1, -0.5f, 0);

    public CameraController Camera;
    #endregion

    // Components and children
    private CharacterController _characterController;
    private Transform _viewPoint;
    private Gun _gun;
    private GameObject _visual;
    private Animation _animation;
    private MeshRenderer _meshRenderer;
    public Animation Animation => _animation;
    private Vector3 _gunPosition;

    // Invisibility
    public float InvisibilityTime;
    private float _invisibilityTimer;

    // Sliding
    private float _slideCooldown = 1f;
    private float _slideCooldownTimer = 0.5f;

    public void UseInvisibiltyCamo()
    {
        if (CamoCount <= 0)
            return;

        CamoCount--;
        _invisibilityTimer = InvisibilityTime;
        GameController.AudioManager.Play("Beep");
    }

    private void UpdateInvisibility()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UseInvisibiltyCamo();

        bool wasInvisible = _invisible;
        _invisible = _invisibilityTimer > 0;

        if (wasInvisible && !_invisible)
            GameController.AudioManager.Play("ElectricDischarge");

        if (_invisible)
            _invisibilityTimer -= Time.deltaTime;

        _meshRenderer.material.color = IsHidden ? Color.black : Color.white;
        GameController.SetInvisOverlay(IsHidden && Camera.Mode == CameraModeType.FirstPerson);
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animation = GetComponent<Animation>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _viewPoint = transform.Find("ViewPoint");
        _visual = transform.Find("Visual").gameObject;
        _peekingBase = _viewPoint.localPosition;
        _peekDuration = PeekTime;
        _gun = GetComponentInChildren<Gun>();

        _standingState = new StandingState();
        _walkingState = new WalkingState(WalkingSpeed, WalkingBobScale, WalkingStepPeriod, WalkingStepRadius);
        _runningState = new RunningState(RunningSpeed, RunningBobScale, RunningStepPeriod, RunningStepRadius);
        _slidingState = new SlidingState(SlidingSpeed, SlidingDuration);
        _movementState = _standingState;
        _gunPosition = _gun.transform.localPosition;

        _gun.transform.parent = Camera.transform;
        _gun.transform.localPosition = _gunPosition;
        _slideCooldownTimer = 0f;
    }

    private void Update()
    {
        if (GameController.IsPaused || _dead)
            return;
        
        if (_slideCooldownTimer > 0f)
            _slideCooldownTimer -= Time.deltaTime;

        _movementState.Update(this);
        UpdateCamera();
        UpdateMovement();
        UpdatePeeking();
        UpdateInteraction();
        UpdateShooting();
        UpdateInvisibility();
        SwitchState();
    }

    private int _hidden = 0;
    private bool _invisible = false;
    public bool IsHidden => _hidden > 0 || _invisible;
    public void SetHidden(bool hidden)
    {
        _hidden += hidden ? 1 : -1;
        _meshRenderer.material.color = IsHidden ? Color.black : Color.white;
    }

    private bool _dead = false;
    public void Die()
    {
        if (_dead) return;

        _dead = true;
        GameController.AudioManager.Play("DeathGrunt");
        var clip = Animation.GetClip("DeathAnimation");
        Animation.Play("DeathAnimation");
        GameController.ExecuteAfter(GameController.NewGame, clip.length);
    }

    #region Update Functions
    /// <summary>
    /// Rotates camera based on mouse movement. Also changes camera mode based on keys pressed.
    /// </summary>
    private void UpdateCamera()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Camera.SwitchMode(CameraModeType.FirstPerson);
            _gun.transform.parent = Camera.transform;
            _gun.transform.localPosition = _gunPosition;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Camera.SwitchMode(CameraModeType.TopDown);
            _gun.transform.parent = _viewPoint;
            _gun.transform.localPosition = _gunPosition;
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Camera.SwitchMode(CameraModeType.ThirdPerson);
            _gun.transform.parent = _viewPoint;
            _gun.transform.localPosition = _gunPosition;
        }

        _visual.SetActive(Camera.Mode != CameraModeType.FirstPerson);

        // rotation
        float yaw = -Input.GetAxis("Mouse X");
        float pitch = Input.GetAxis("Mouse Y");
        Camera.Rotate(yaw, pitch, GameSettings.MouseSensitivity);
    }

    /// <summary>
    /// Ray cast if an interactable object is in sight. Interaction by pressing or holding the Interact button.
    /// Displays interaction prompts.
    /// </summary>
    private void UpdateInteraction()
    {
        if (!_movementState.CanInteract)
        {
            GameController.HideInteraction();
            return;
        }

        Vector3 direction = Camera.Mode == CameraModeType.TopDown
            ? Camera.GetGroundDirection()
            : Camera.transform.forward;

        Vector3 pos = Camera.Mode == CameraModeType.TopDown
            ? _viewPoint.position.Added(y: -1.5f)
            : _viewPoint.position;

        bool hit = Physics.Raycast(
            pos,
            direction,
            out RaycastHit hitInfo,
            maxDistance: 2f);

        if (!hit)
        {
            GameController.HideInteraction();
            return;
        }

        var usableObject =
            hitInfo.collider.gameObject.transform
            .GetComponentInParent<IInteractableObject>();

        if (usableObject == null || !usableObject.CanInteract)
        {
            GameController.HideInteraction();
            return;
        };

        if (usableObject.InteractionType == InteractionType.Single)
        {
            GameController.ShowInteraction(usableObject.InteractionPrompt());
            if (Input.GetKeyDown(KeyCode.F)) usableObject.Interact(this);
        }

        if (usableObject.InteractionType == InteractionType.Continuous)
        {
            if (Input.GetKey(KeyCode.F))
            {
                float p = usableObject.Interact(this);
                GameController.ShowContinuousInteraction(usableObject.InteractionPrompt(), p);
            } else
            {
                GameController.ShowContinuousInteraction(usableObject.InteractionPrompt());
            }

        }
    }

    /// <summary>
    /// Aims, Stops Aiming or Shoots the gun.
    /// </summary>
    private void UpdateShooting()
    {

        if (!_movementState.CanShoot || !Input.GetMouseButton(1))
        {
            _gun.StopAim();
            return;
        }

        Vector3 direction =
            Camera.Mode == CameraModeType.FirstPerson
            ? Camera.transform.forward
            : Camera.GetGroundDirection();

        //transform.forward = direction;
        _gun.Aim(direction);

        if (Input.GetMouseButtonDown(0))
        {
            _gun.Shoot();
        }
    }

    /// <summary>
    /// Movement is based on the <see cref="IMovementState"/>.
    /// </summary>
    private void UpdateMovement()
    {
        Vector2 inputDir = GetInputDir();
        Vector3 cameraForward = Camera.GetGroundDirection();
        Vector3 cameraRight = -Vector3.Cross(cameraForward, Vector3.up);
        Vector3 desiredDir = inputDir.x * cameraRight + inputDir.y * cameraForward;
        Vector3 movement = _movementState.GetMovement(desiredDir, _previousMovement);

        _characterController.SimpleMove(movement);
        _previousMovement = movement;

        // Rotation
        if (!_movementState.FreeLook && Camera.Mode == CameraModeType.FirstPerson)
            transform.forward = cameraForward;
        else if (_gun.Aiming)
            _viewPoint.forward = cameraForward;
        else if (movement.magnitude != 0)
            transform.forward = movement.normalized;
    }
    #endregion

    #region States And Movement
    private IMovementState _movementState;
    private StandingState _standingState;
    private WalkingState _walkingState;
    private RunningState _runningState;
    private SlidingState _slidingState;
    private Vector3 _previousMovement;

    /// <summary>
    /// This is basically a finite state machine, states being <see cref="IMovementState"/>.
    /// </summary>
    private void SwitchState()
    {
        if (_movementState.Locked)
            return;

        if (_movementState == _standingState)
            if (IsMovementRequest())
            {
                EnterState(_walkingState);
                return;
            }

        if (_movementState == _walkingState)
        {
            if (!IsMovementRequest())
            {
                EnterState(_standingState);
                _walkingState.StepSound(this);
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                EnterState(_runningState);
                return;
            }
        }

        if (_movementState == _runningState)
        {
            if (!IsMovementRequest())
            {
                EnterState(_standingState);
                _runningState.StepSound(this);
                return;
            }

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                EnterState(_walkingState);
                return;
            }

            if (Input.GetKey(KeyCode.Space) && _slideCooldownTimer <= 0f)
            {
                EnterState(_slidingState);
                return;
            }
        }

        if (_movementState == _slidingState)
        {
            if (IsMovementRequest())
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    EnterState(_runningState);
                else
                    EnterState(_walkingState);
            } else
                EnterState(_standingState);

            _slideCooldownTimer = _slideCooldown;
            return;
        }

    }

    private void EnterState(IMovementState state)
    {
        _movementState.Exit(this);
        state.Enter(this);
        _movementState = state;
    }

    /// <summary>
    /// Keys D,A sets x axis to +-1.
    /// Keys W,S sets y axis to +-1
    /// </summary>
    /// <returns></returns>
    public Vector2 GetInputDir()
    {
        // Movement
        Vector2 inputDir = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) inputDir.y += 1;
        if (Input.GetKey(KeyCode.A)) inputDir.x -= 1;
        if (Input.GetKey(KeyCode.S)) inputDir.y -= 1;
        if (Input.GetKey(KeyCode.D)) inputDir.x += 1;
        return inputDir.normalized;
    }

    /// <summary>
    /// Is the player pressing keys to move? Also returns false if opposing keys are pressed and resulting speed would be 0.
    /// </summary>
    /// <returns></returns>
    public bool IsMovementRequest()
    {
        int horizontal = 0;
        int vertical = 0;

        if (Input.GetKey(KeyCode.W)) vertical += 1;
        if (Input.GetKey(KeyCode.S)) vertical -= 1;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1;
        if (Input.GetKey(KeyCode.D)) horizontal += 1;

        return horizontal != 0 || vertical != 0;
    }

    #endregion

    #region Keys
    public List<IKey> Keys = new List<IKey>();
    public int CamoCount = 0;
    public int TrapKitCount = 0;
    public bool HasKeyForLock(ILock @lock)
    {
        foreach (IKey key in Keys)
            if (key.Locks.Contains(@lock))
                return true;

        return false;
    }

    public void AddKey(IKey key)
    {
        GameController.AudioManager.Play("Jingle");
        Keys.Add(key);
    }

    #endregion

    #region Peeking

    private Vector3 _peekingOffset;
    private Vector3 _peekingBase;
    private Vector3 _peekingWorldBase;
    private Vector3 _peekingFrom;
    private Vector3 _peekingTo;
    private float _peekDuration;
    private float _peekTimer;

    /// <summary>
    /// Adds rotation offset to the camera and position offset to the _viewPoint to peek around corners. Does not happen instantly, there is an animation.
    /// </summary>
    /// <param name="offset"></param>
    private void PeekStart(Vector3 offset)
    {
        float angle = offset.x < 0 ? PeekAngle : -PeekAngle;
        Camera.CustomRotationStart(_peekDuration, new Vector3(0, 0, angle));

        _peekingFrom = _peekingOffset;
        _peekingTo = offset;
        _peekTimer = 0;
        _peekDuration = PeekTime;
    }

    /// <summary>
    /// Sets peeking parameters to 0. Does not return instantly, there is an animation.
    /// </summary>
    private void PeekEnd()
    {
        if (_peekingTo == Vector3.zero) return;

        Camera.CustomRotationEnd();
        _peekingFrom = _peekingOffset;
        _peekingTo = Vector3.zero;
        _peekTimer = 0;
        _peekDuration = _peekDuration - _peekTimer;
    }

    /// <summary>
    /// Responds to peeking keys. Also performs the animation for peeking.
    /// </summary>
    private void UpdatePeeking()
    {
        if (!_movementState.CanPeek || PeekEndRequest() || Camera.Mode == CameraModeType.TopDown)
        {
            PeekEnd();
        }

        Vector3? offset = PeekStartRequest();
        if (_movementState.CanPeek && offset != null)
        {
            PeekStart(offset.Value);
        }

        float t = Mathf.Clamp01(_peekTimer / _peekDuration);
        _peekingOffset = Vector3.Lerp(_peekingFrom, _peekingTo, t);
        _peekTimer += Time.deltaTime;

        Vector3 cameraForawd = Camera.GetGroundDirection();
        Vector3 cameraRight = -Vector3.Cross(cameraForawd, Vector3.up);
        _viewPoint.localPosition = _peekingBase.Added(y: _peekingOffset.y);
        _viewPoint.position += cameraForawd * _peekingOffset.z + cameraRight * _peekingOffset.x;

        // Collision
        var worldBase = transform.localToWorldMatrix.MultiplyPoint(_peekingBase);
        var dir = _viewPoint.position - worldBase;
        int mask = ~((1 << 2) | (1 << 6));
        if (Physics.Raycast(worldBase, dir.normalized, out RaycastHit hitInfo, dir.magnitude, mask))
        {
            _viewPoint.position = worldBase + dir.normalized * (hitInfo.distance - 0.4f);
        }
    }

    /// <summary>
    /// Gets offset for peeking if keys are pressed.
    /// </summary>
    /// <returns></returns>
    private Vector3? PeekStartRequest()
    {
        Vector3 offset = Vector3.zero;
        if (Camera.Mode == CameraModeType.ThirdPerson)
            offset = offset.Added(z: 2);

        if (Input.GetKeyDown(KeyCode.E))
            return PeekOffset + offset;

        if (Input.GetKeyDown(KeyCode.Q))
            return PeekOffset.Multiplied(x: -1) + offset;

        return null;
    }

    /// <summary>
    /// Happens on release a peeking key.
    /// </summary>
    /// <returns></returns>
    private bool PeekEndRequest() =>
        Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q);
    #endregion
}
