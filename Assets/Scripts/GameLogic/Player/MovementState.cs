using System;
using UnityEngine;


interface IMovementState
{
    bool CanShoot { get; }
    bool CanPeek { get; }
    bool CanInteract { get; }
    bool Locked { get; }
    bool FreeLook { get; }
    void Enter(PlayerController player);
    void Exit(PlayerController player);
    void Update(PlayerController player);
    Vector3 GetMovement(Vector3 desiredMovement, Vector3 previousMovement);
}

class StandingState : IMovementState
{
    public StandingState()
    {
    }

    public bool Locked => false;
    public bool FreeLook => false;

    public bool CanShoot => true;

    public bool CanPeek => true;

    public bool CanInteract => true;

    public void Enter(PlayerController player)
    {
    }

    public void Exit(PlayerController player)
    {
    }

    public Vector3 GetMovement(Vector3 desiredMovement, Vector3 previousMovement)
    {
        return Vector3.zero;
    }

    public void Update(PlayerController player)
    {
    }
}

class WalkingState : IMovementState
{
    private float _movementSpeed;
    private float _bobScale;
    private float _stepPeriod;
    private float _stepRadius;

    private float _stepTimer;

    public WalkingState(float movementSpeed, float bobScale, float stepPeriod, float stepRadius)
    {
        _movementSpeed = movementSpeed;
        _bobScale = bobScale;
        _stepPeriod = stepPeriod;
        _stepRadius = stepRadius;
    }

    public bool Locked => false;
    public bool FreeLook => false;

    public bool CanShoot => true;

    public bool CanPeek => true;

    public bool CanInteract => true;

    public void Enter(PlayerController player)
    {
        player.Camera.BobDuration = _stepPeriod / 2;
        player.Camera.BobScale = _bobScale;
        _stepTimer = 0.5f * _stepPeriod;

        if (player.Camera.Mode == CameraModeType.FirstPerson)
            player.Camera.BobStart();
    }
    public void Update(PlayerController player)
    {
        if (_stepTimer > _stepPeriod)
        {
            _stepTimer %= _stepPeriod;
            StepSound(player);
        }

        _stepTimer += Time.deltaTime;
    }

    public void Exit(PlayerController player)
    {
        player.Camera.BobEnd();
    }

    public Vector3 GetMovement(Vector3 desiredMovement, Vector3 previousMovement)
    {
        return desiredMovement * _movementSpeed;
    }

    public void StepSound(PlayerController player)
    {
        GameController.AudioManager.PlayStep("Default", player.gameObject, volume: 0.3f);
        GameController.AudioManager.AudibleEffect(player.gameObject, player.transform.position, _stepRadius);
    }
}

class RunningState : IMovementState
{
    private float _movementSpeed;
    private float _bobScale;
    private float _stepPeriod;
    private float _stepRadius;

    private float _stepTimer;

    public RunningState(float movementSpeed, float bobScale, float stepPeriod, float stepRadius)
    {
        _movementSpeed = movementSpeed;
        _bobScale = bobScale;
        _stepPeriod = stepPeriod;
        _stepRadius = stepRadius;
    }

    public bool Locked => false;
    public bool FreeLook => false;

    public bool CanShoot => false;

    public bool CanPeek => false;

    public bool CanInteract => false;

    public void Enter(PlayerController player)
    {
        player.Camera.BobDuration = _stepPeriod / 2;
        player.Camera.BobScale = _bobScale;
        _stepTimer = 0.5f * _stepPeriod;

        if (player.Camera.Mode == CameraModeType.FirstPerson)
            player.Camera.BobStart();
    }
    public void Update(PlayerController player)
    {
        if (_stepTimer > _stepPeriod)
        {
            _stepTimer %= _stepPeriod;
            StepSound(player);
        }

        _stepTimer += Time.deltaTime;
    }

    public void Exit(PlayerController player)
    {
        player.Camera.BobEnd();
    }

    public Vector3 GetMovement(Vector3 desiredMovement, Vector3 previousMovement)
    {
        return desiredMovement * _movementSpeed;
    }

    public void StepSound(PlayerController player)
    {
        GameController.AudioManager.PlayStep("Default", player.gameObject, volume: 1f);
        GameController.AudioManager.AudibleEffect(player.gameObject, player.transform.position, _stepRadius);
    }
}

class SlidingState : IMovementState
{
    private float _duration;
    private float _timer;
    private float _initialSpeed;

    public SlidingState(float initialSpeed, float duration)
    {
        _initialSpeed = initialSpeed;
        _duration = duration;
        _timer = duration;
    }

    public bool Locked => _timer > 0;
    public bool FreeLook => true;

    public bool CanShoot => true;

    public bool CanPeek => false;

    public bool CanInteract => true;

    public void Enter(PlayerController player)
    {
        _timer = _duration;

        if (player.Camera.Mode == CameraModeType.FirstPerson || player.Camera.Mode == CameraModeType.ThirdPerson)
            player.Camera.CustomOffsetStart(0.2f, Vector3.down, false);

        if (player.Camera.Mode == CameraModeType.FirstPerson)
        {
            float rotation = player.GetInputDir().x > 0 ? 10 : -10;
            player.Camera.CustomRotationStart(0.2f, new Vector3(10, 0, rotation));
        }

        GameController.AudioManager.Play("Slide");
        player.Animation.Play("SlideAnimation");
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("EXIT");
        player.Camera.CustomRotationEnd();
        player.Camera.CustomOffsetEnd();
    }

    public Vector3 GetMovement(Vector3 desiredMovement, Vector3 previousMovement)
    {
        float t = Mathf.Clamp01(_timer / _duration);
        return previousMovement.normalized * _initialSpeed * t;
    }

    public void Update(PlayerController player)
    {
        _timer -= Time.deltaTime;
    }
}
