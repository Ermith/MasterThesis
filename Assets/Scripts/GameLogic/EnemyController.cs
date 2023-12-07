using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sight), typeof(Audition), typeof(CharacterController))]
public class EnemyController : MonoBehaviour
{
    Sight _sight;
    Audition _audition;
    CharacterController _characterController;
    Vector3? _moveTo = null;
    float _turnRate = 4f;
    Vector3? _lookDirection = null;
    Vector3[] _patrolPositions = null;
    const float EPSILON_RADIUS = 0.15f;
    int _patrolIndex = 0;
    int _patrolStep = 1;
    bool _patrolEnabled = true;
    Action _positionReached = null;

    Vector3 _lookFrom;
    Vector3 _lookTo;
    float _rotationTime;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _sight = GetComponent<Sight>();
        _sight.HeightCorrection = _characterController.height / 2;
        _audition = GetComponent<Audition>();
        _audition.SoundResponse += SoundResponse;
    }

    private void SoundResponse(GameObject sourceTarget, Vector3 sourcePosition)
    {
        _patrolEnabled = false;
        LookAt(sourcePosition);
        MoveTo(sourcePosition, () => _patrolEnabled = true);
    }

    // Update is called once per frame
    void Update()
    {
        ResolvePatrol();
        ResolveRotation();
        ResolveMovement();

        var player = FindObjectOfType<PlayerController>();
        _sight.VisionConeHilighted = false;
        if (!player.IsHidden && _sight.CanSee(player.transform))
        {
            LookAt(player.transform.position);
            MoveTo(player.transform.position);
            _sight.VisionConeHilighted = true;
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

    public void Patrol(Vector3[] positions, int index = 0)
    {
        _patrolPositions = positions;
        _patrolIndex = index;
    }

    private void ResolvePatrol()
    {
        if (_patrolPositions == null || _patrolPositions.Length < 1 || !_patrolEnabled) return;

        Vector3 position = _patrolPositions[_patrolIndex];
        LookAt(position);
        MoveTo(position, () =>
        {
            int count = _patrolPositions.Length;
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
        myPosition.y = 0f;
        Vector3 difference = _moveTo.Value - myPosition;
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

}
