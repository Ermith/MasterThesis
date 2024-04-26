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

[RequireComponent(typeof(Sight), typeof(Audition), typeof(CharacterController))]
public class EnemyController : MonoBehaviour, ILockObject
{
    Sight _sight;
    Audition _audition;
    CharacterController _characterController;
    Vector3? _moveTo = null;
    float _turnRate = 4f;
    Vector3? _lookDirection = null;
    Vector3[] _patrolPositions = null;
    const float EPSILON_RADIUS = 0.75f;
    float _lastMovement = float.MaxValue;
    List<Vector3> _movementBuffer = new();

    // patrol
    int _patrolIndex = 0;
    int _patrolStep = 1;
    bool _chasing = false;
    bool _patrolRetrace;
    Action _positionReached = null;

    Vector3 _lookFrom;
    Vector3 _lookTo;
    float _rotationTime;

    public ILock Lock { get; set; }
    public Behaviour Behaviour { get; set; }
    public Vector3 DefaultPosition = Vector3.zero;
    public Vector3 DefaultDirection = Vector3.forward;
    public float GuardViewDistance = 5f;
    public float DefaultViewDistance = 20f;
    public float FrustrationTime = 1f;
    public float InvestigationTime = 5f;
    public float StepTime = 0.6f;
    public float NormalSpeed = 3f;
    public float ChasingSpeed = 5f;


    private float _frustrationTimer;
    private float _investigationTimer;
    private float _stepTimer;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _sight = GetComponent<Sight>();
        _audition = GetComponent<Audition>();
        _audition.SoundResponse += SoundResponse;
        _sight.Range = DefaultViewDistance;

        // This fixes height when enemy spawns
        _characterController.SimpleMove(Vector3.zero);
    }

    private void SoundResponse(GameObject sourceTarget, Vector3 sourcePosition)
    {
        _chasing = true;
        _sight.Range = DefaultViewDistance;
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

        var player = FindObjectOfType<PlayerController>();
        _sight.VisionConeHilighted = false;
        if (!player.IsHidden && _sight.CanSee(player.transform))
        {
            _chasing = true;
            LookAt(player.transform.position);
            MoveTo(player.transform.position, () =>
            {
                _chasing = false;
                _investigationTimer = InvestigationTime;
            });
            _sight.VisionConeHilighted = true;
            _sight.Range = DefaultViewDistance;

            if ((player.transform.position - transform.position).magnitude < 2f)
                player.Die();
        }
    }

    public void LookInDirection(Vector3 direction)
    {
        _lookDirection = direction;
    }

    public void LookAt(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        LookInDirection(direction);
    }

    public void MoveTo(Vector3 position, Action callback = null)
    {
        GetComponent<NavMeshAgent>().destination = position;
        _positionReached = callback;
        return;
    }

    public void Patrol(Vector3[] positions, int index = 0, bool retrace = true)
    {
        _patrolPositions = positions;
        _patrolIndex = index;
        _patrolRetrace = retrace;
        Behaviour = Behaviour.Patroling;
    }

    public void Sleep(Vector3 position)
    {
        DefaultPosition = position;
        Behaviour = Behaviour.Sleeping;
    }

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

    public void ResolveBehaviour()
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

    private void ResolveSleep()
    {
        MoveTo(DefaultPosition,
            () =>
            {
                _sight.Range = 0;
            });
    }

    private void ResolvePatrol()
    {
        if (_patrolPositions == null || _patrolPositions.Length < 1 || _chasing)
            return;

        Vector3 position = _patrolPositions[_patrolIndex];
        MoveTo(position, () =>
        {
            int count = _patrolPositions.Length;

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

    private void ResolveRotation()
    {
        if (_lookDirection == null) return;

        Vector3 dir = _lookDirection.Value;
        dir.y = 0f;
        dir.Normalize();
        transform.forward = Vector3.RotateTowards(transform.forward, dir, _turnRate * Time.deltaTime, 0);
        //transform.forward = Vector3.Lerp(transform.forward, dir, _turnRate * Time.deltaTime);
        if (Vector3.Angle(transform.forward, dir) <= float.Epsilon)
            _lookDirection = null;
    }

    private void ResolveMovement()
    {
        GetComponent<NavMeshAgent>().speed = _chasing ? ChasingSpeed : NormalSpeed;
        Vector3 d = GetComponent<NavMeshAgent>().destination;

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
    }

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

    public void Unlock()
    {
        Die();
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }
}
