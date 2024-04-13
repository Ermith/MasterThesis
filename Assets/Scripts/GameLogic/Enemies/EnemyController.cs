using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        LookAt(sourcePosition);
        MoveTo(sourcePosition, () => _chasing = false);
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
            MoveTo(player.transform.position, () => _chasing = false);
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
        _moveTo = position;
        _positionReached = callback;
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
    }

    public void ResolveBehaviour()
    {
        if (_chasing)
            return;

        if (Behaviour == Behaviour.Patroling)
            ResolvePatrol();

        if (Behaviour == Behaviour.Guarding)
            ResolveGuard();

        if (Behaviour == Behaviour.Sleeping)
            ResolveSleep();
    }

    private void ResolveGuard()
    {
        MoveTo(DefaultPosition, () => {
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
        LookAt(position);
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
        if (_moveTo == null) return;

        Vector3 myPosition = transform.position;
        //myPosition.y = 0f;
        Vector3 difference = _moveTo.Value - myPosition;
        difference.y = 0f;
        Vector3 dir = difference.normalized;
        float distance = difference.magnitude;
        if (distance <= EPSILON_RADIUS)
        {
            _positionReached?.Invoke();
            _positionReached = null;
            _moveTo = null;
            return;
        }

        _characterController.SimpleMove(dir * 3);
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
