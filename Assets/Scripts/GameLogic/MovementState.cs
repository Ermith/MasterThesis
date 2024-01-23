using UnityEngine;
/*/
public interface IMovementState
{
    float MovementSpeed { get; }
    bool CanShoot { get; }
    bool CanPeek { get; }
    bool CanInteract { get; }
    bool CanSwitch { get; }
    bool FreeLook { get; }
    bool BobEnabled { get; }
    float StepPeriod { get; }
    float StepRange { get; }
    float BobScale { get; }
    void Update();
    void Reset(PlayerController player);
    void Exit(PlayerController player);
    Vector3 GetMovement(Vector3 inputDir, Vector3 previousMovement, CameraController camera);
    Vector3 GetRotation(Vector3 movementDir, Vector3 previousRotation, CameraController camera);
}

public class Walking : IMovementState
{
    public Walking(float movementSpeed, float stepPeriod, float stepRange, float bobScale)
    {
        MovementSpeed = movementSpeed;
        StepPeriod = stepPeriod;
        StepRange = stepRange;
        BobScale = bobScale;
    }
    public float MovementSpeed { get; set; }
    public bool CanShoot => true;
    public bool CanPeek => true;
    public bool CanInteract => true;
    public bool CanSwitch => true;
    public bool FreeLook => false;

    public bool BobEnabled => true;

    public float StepPeriod { get; private set; }

    public float StepRange { get; private set; }

    public float BobScale { get; private set; }

    public void Update() { }

    public Vector3 GetRotation(Vector3 movementDir, Vector3 previousRotation, CameraController camera)
    {
        return camera.Mode == CameraModeType.FirstPerson
            ? camera.GetGroundDirection()
            : movementDir;
    }

    public Vector3 GetMovement(Vector3 inputDir, Vector3 previousMovement, CameraController camera)
    {
        Vector3 cameraDir = camera.GetGroundDirection();
        Vector3 cameraRight = -Vector3.Cross(cameraDir, Vector3.up);
        Vector3 direction = cameraDir * inputDir.z + cameraRight * inputDir.x;

        return direction;
    }

    public void Reset(PlayerController player)
    {
    }

    public void Exit(PlayerController player)
    {
    }
}

public class Standing : IMovementState
{
    public float MovementSpeed => 0;
    public bool CanShoot => true;
    public bool CanPeek => true;
    public bool CanInteract => true;
    public bool CanSwitch => true;
    public bool FreeLook => false;

    public bool BobEnabled => false;

    public float StepPeriod => 0;

    public float StepRange => 0;

    public float BobScale => 0;

    public void Exit(PlayerController player)
    {
    }

    public Vector3 GetRotation(Vector3 movementDir, Vector3 previousRotation, CameraController camera)
    {
        return camera.Mode == CameraModeType.FirstPerson
            ? camera.GetGroundDirection()
            : previousRotation;
    }

    public void Reset(PlayerController player)
    {
    }

    public void Update()
    {
    }

    Vector3 IMovementState.GetMovement(Vector3 inputDir, Vector3 previousMovement, CameraController camera)
    {
        return previousMovement;
    }
}

public class Sprinting : IMovementState
{
    public Sprinting(float movementSpeed, float stepPeriod, float stepRange, float bobScale)
    {
        MovementSpeed = movementSpeed;
        StepPeriod = stepPeriod;
        StepRange = stepRange;
        BobScale = bobScale;
    }

    public float MovementSpeed { get; private set; }

    public bool CanShoot => false;

    public bool CanPeek => false;

    public bool CanInteract => false;
    public bool CanSwitch => true;

    public bool FreeLook => false;

    public bool BobEnabled => true;

    public float StepPeriod { get; private set; }

    public float StepRange { get; private set; }

    public float BobScale { get; private set; }

    public Vector3 GetRotation(Vector3 movementDir, Vector3 previousRotation, CameraController camera)
    {
        return camera.Mode == CameraModeType.FirstPerson
            ? camera.GetGroundDirection()
            : movementDir;
    }

    public void Update()
    {
    }

    public Vector3 GetMovement(Vector3 inputDir, Vector3 previousMovement, CameraController camera)
    {
        Vector3 cameraDir = camera.GetGroundDirection();
        Vector3 cameraRight = -Vector3.Cross(cameraDir, Vector3.up);
        Vector3 direction = cameraDir * inputDir.z + cameraRight * inputDir.x;

        return direction;
    }

    public void Reset(PlayerController player)
    {
    }

    public void Exit(PlayerController player)
    {
    }
}

public class Sliding : IMovementState
{
    private float _duration;
    private float _timer;
    private Vector3 _direction;
    private float _initialSpeed;

    public Sliding(Vector3 direction, float initialSpeed, float duration)
    {
        _initialSpeed = initialSpeed;
        _duration = duration;
        _direction = direction;
        _timer = duration;
    }

    public float MovementSpeed =>
        _initialSpeed * Mathf.Clamp01(_timer / _duration);
    public bool CanShoot => true;
    public bool CanPeek => false;
    public bool CanInteract => true;
    public bool CanSwitch => _timer <= 0;
    public bool FreeLook => true;

    public bool BobEnabled => false;
    public float StepPeriod => 0;
    public float StepRange => 0;
    public float BobScale => 0;

    public void Update() {
        _timer -= Time.deltaTime;
    }

    public Vector3 GetRotation(Vector3 movementDir, Vector3 previousRotation, CameraController camera)
    {
        return movementDir;
    }

    public Vector3 GetMovement(Vector3 inputDir, Vector3 previousMovement, CameraController camera)
    {
        return _direction;
    }

    public void Reset(PlayerController player)
    {
        _timer = _duration;
        _direction = player.GetComponent<CharacterController>().velocity.normalized;

        if (player.Camera.Mode == CameraModeType.FirstPerson || player.Camera.Mode == CameraModeType.ThirdPerson)
            player.Camera.CustomOffsetStart(0.2f, Vector3.down, false);

        if (player.Camera.Mode == CameraModeType.FirstPerson)
            player.Camera.CustomRotationStart(0.2f, new Vector3(10, 0, 10));

        //GameController.AudioManager.Play("Slide");
    }

    public void Exit(PlayerController player)
    {
        player.Camera.CustomOffsetEnd();
        player.Camera.CustomRotationEnd();
    }
}
//*/