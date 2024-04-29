using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum Behaviour
{
    Patroling,
    Sleeping,
    Guarding,
}

/// <summary>
/// AI handling guards. Uses NavmeshAgent to move around. Responds to sounds and visual comformation of the player.
/// </summary>
[RequireComponent(typeof(Sight), typeof(Audition), typeof(CharacterController))]
public class EnemyController : MonoBehaviour, ILockObject
{
    #region Properties
    // Component references
    private Sight _sight;
    private Audition _audition;
    private CharacterController _characterController;
    private NavMeshAgent _navMeshAgent;

    // Patrol
    int _patrolIndex = 0;
    private int _patrolStep = 1;
    private bool _chasing = false;
    private bool _patrolRetrace;
    private Action _positionReached = null;
    private Vector3[] _patrolPath = null;

    // Adjustable in inspector
    [Tooltip("Range of view cone when guarding something.")]
    public float GuardViewDistance = 5f;
    [Tooltip("Range of view cone.")]
    public float DefaultViewDistance = 20f;
    [Tooltip("Time it takes to return to default position when chasing and can't move.")]
    public float FrustrationTime = 1f;
    [Tooltip("Time it takes to investigate a sound or appearance of the player.")]
    public float InvestigationTime = 5f;
    [Tooltip("Time between step sounds.")]
    public float StepTime = 0.6f;
    [Tooltip("Walking Speed.")]
    public float NormalSpeed = 3f;
    [Tooltip("Speed when chasing the player.")]
    public float ChasingSpeed = 5f;
    [Tooltip("Rate of rotation.")]
    public float TurnRate = 4f;

    // Movement and Behavior
    private float _frustrationTimer;
    private float _investigationTimer;
    private float _stepTimer;
    private Vector3? _lookDirection = null;
    private float _lastMovement = float.MaxValue;
    private Vector3 _lastPosition = Vector3.zero;
    private PlayerController _player = null;
    public ILock Lock { get; set; }
    public Behaviour Behaviour { get; set; }
    [HideInInspector] public Vector3 DefaultPosition = Vector3.zero;
    [HideInInspector] public Vector3 DefaultDirection = Vector3.forward;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _sight = GetComponent<Sight>();
        _audition = GetComponent<Audition>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _player = FindObjectOfType<PlayerController>();

        _audition.SoundResponse += SoundResponse;
        _sight.Range = DefaultViewDistance;

        // This fixes height when enemy spawns
        _characterController.SimpleMove(Vector3.zero);
    }

    /// <summary>
    /// Goes to sound source and stands there for InvestigationTime duration./>
    /// </summary>
    /// <param name="sourceTarget"></param>
    /// <param name="sourcePosition"></param>
    private void SoundResponse(GameObject sourceTarget, Vector3 sourcePosition)
    {
        _chasing = true;
        _sight.Range = DefaultViewDistance;
        _frustrationTimer = FrustrationTime;
        LookAt(sourcePosition);
        MoveTo(sourcePosition, () =>
        {
            _chasing = false;
            _investigationTimer = InvestigationTime;
        });
    }

    // Update is called once per frame
    void Update()
    {
        ResolveBehaviour();
        ResolveRotation();
        ResolveMovement();
        CheckPlayerInSight();
    }

    #region Public Functions
    /// <summary>
    /// Sets desired direction. Turns there with TurnRate over time.
    /// </summary>
    /// <param name="direction"></param>
    public void LookInDirection(Vector3 direction)
    {
        _lookDirection = direction;
    }

    /// <summary>
    /// Sets direction looking at given position. Turns there with TurnRate over time.
    /// </summary>
    /// <param name="position"></param>
    public void LookAt(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        LookInDirection(direction);
    }

    /// <summary>
    /// Sets desired destination into NavMeshAgent.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="callback">Called once when destination reached.</param>
    public void MoveTo(Vector3 destination, Action callback = null)
    {
        _navMeshAgent.destination = destination;
        _positionReached = callback;
        return;
    }

    /// <summary>
    /// Sets patrol path and sets behavior to Patrolling.
    /// </summary>
    /// <param name="patrolPath"></param>
    /// <param name="startIndex"></param>
    /// <param name="retrace">Patrol back and forth or in a cycle?</param>
    public void Patrol(Vector3[] patrolPath, int startIndex = 0, bool retrace = true)
    {
        _patrolPath = patrolPath;
        _patrolIndex = startIndex;
        _patrolRetrace = retrace;
        Behaviour = Behaviour.Patroling;
    }

    /// <summary>
    /// Sets default position to the given position.
    /// Whenever the guard isn't doing anything, returns to default position and reduces sight range to 0.
    /// </summary>
    /// <param name="position"></param>
    public void Sleep(Vector3 position)
    {
        DefaultPosition = position;
        Behaviour = Behaviour.Sleeping;
    }

    /// <summary>
    /// Sets default position and direction of looking.
    /// Whenever the guard isn't doing anything, returns to default position, turns in default direction and reduces sight range to GuardViewDistance.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    public void Guard(Vector3 position, Vector3 direction)
    {
        DefaultPosition = position;
        DefaultDirection = direction;
        Behaviour = Behaviour.Guarding;
        //LookAt(DefaultPosition);
        //MoveTo(DefaultPosition, () =>
        //{
        //    LookInDirection(DefaultDirection);
        //    _sight.Range = GuardViewDistance;
        //});
    }

    #endregion

    #region Behaviors

    /// <summary>
    /// Resolves behaviour based on Behaviour property. However, chasing and investigation takes priority.
    /// </summary>
    private void ResolveBehaviour()
    {
        if (_investigationTimer > 0)
        {
            _investigationTimer -= Time.deltaTime;
            return;
        }

        if (_chasing)
        {
            ResolveChase();
            return;
        }

        if (Behaviour == Behaviour.Patroling)
            ResolvePatrol();

        if (Behaviour == Behaviour.Guarding)
            ResolveGuard();

        if (Behaviour == Behaviour.Sleeping)
            ResolveSleep();
    }

    /// <summary>
    /// Returns to default position, turns in default direction and reduces sight range to GuardViewDistance.
    /// </summary>
    private void ResolveGuard()
    {
        if ((transform.position - DefaultPosition).magnitude < 1f)
        {
            _sight.Range = GuardViewDistance;
            LookInDirection(DefaultDirection);
            return;
        }

        MoveTo(DefaultPosition, () =>
        {
            LookInDirection(DefaultDirection);
            _sight.Range = GuardViewDistance;
        });
    }

    /// <summary>
    /// Returns to default position and reduces sight range to 0.
    /// </summary>
    private void ResolveSleep()
    {
        MoveTo(DefaultPosition,
            () =>
            {
                _sight.Range = 0;
            });
    }

    /// <summary>
    /// Moves along the patrol path.
    /// </summary>
    private void ResolvePatrol()
    {
        if (_patrolPath == null || _patrolPath.Length < 1 || _chasing)
            return;

        Vector3 position = _patrolPath[_patrolIndex];
        MoveTo(position, () =>
        {
            int count = _patrolPath.Length;

            if (!_patrolRetrace)
            {
                _patrolIndex++;
                _patrolIndex %= count;
                return;
            }

            _patrolIndex += _patrolStep;
            if (_patrolIndex < 0 || _patrolIndex >= count)
            {
                _patrolStep *= -1;
                _patrolIndex = (int)Mathf.Clamp(_patrolIndex, 0, count - 1);
            }
        });
    }

    /// <summary>
    /// Rotates in _lookDirection based on TurnRate.
    /// </summary>
    private void ResolveRotation()
    {
        if (_lookDirection == null) return;

        Vector3 dir = _lookDirection.Value;
        dir.y = 0f;
        dir.Normalize();
        transform.forward = Vector3.RotateTowards(transform.forward, dir, TurnRate * Time.deltaTime, 0);

        if (Vector3.Angle(transform.forward, dir) <= float.Epsilon)
            _lookDirection = null;
    }

    /// <summary>
    /// Changes movement speed based on if chasing. If destination reached, calles the callback. Also plays step sounds.
    /// </summary>
    private void ResolveMovement()
    {
        _navMeshAgent.speed = _chasing ? ChasingSpeed : NormalSpeed;
        Vector3 d = _navMeshAgent.destination;

        if ((transform.position - d).magnitude < 0.5f)
        {
            _positionReached?.Invoke();
            _positionReached = null;
            _stepTimer = 0f;
        } else
        {
            _stepTimer += Time.deltaTime;
            if (_stepTimer > StepTime)
            {
                GameController.AudioManager.PlayStep("Rubber", gameObject, volume: 0.5f, spacialBlend: 1f, destroyTarget:true);
                _stepTimer %= StepTime;
            }
        }

        _lastMovement = (transform.position - _lastPosition).magnitude;
        _lastPosition = transform.position;
    }

    /// <summary>
    /// Stops chasing if reached frustration timer by not being able to move.
    /// </summary>
    private void ResolveChase()
    {
        if (_lastMovement < 0.0001f)
        {
            _frustrationTimer -= Time.deltaTime;
            _chasing = _frustrationTimer > 0;
        } else
        {
            _frustrationTimer = FrustrationTime;
        }
    }

    #endregion

    /// <summary>
    /// If player is in sight, sets behaviour to chasing the player. Kills the player if too close and sees him.
    /// </summary>
    private void CheckPlayerInSight()
    {
        _sight.VisionConeHilighted = false;
        if (!_player.IsHidden && _sight.CanSee(_player.transform))
        {
            _chasing = true;
            _frustrationTimer = FrustrationTime;
            LookAt(_player.transform.position);
            MoveTo(_player.transform.position, () =>
            {
                _chasing = false;
                _investigationTimer = InvestigationTime;
            });
            _sight.VisionConeHilighted = true;
            _sight.Range = DefaultViewDistance;

            if ((_player.transform.position - transform.position).magnitude < 2f)
                _player.Die();
        }
    }

    /// <summary>
    /// Kills the guard.
    /// </summary>
    public void Unlock()
    {
        Die();
    }

    /// <summary>
    /// Destroys object.
    /// </summary>
    public void Die()
    {
        gameObject.SetActive(false);
    }
}
