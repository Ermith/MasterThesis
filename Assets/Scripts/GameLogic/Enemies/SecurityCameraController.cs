using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Security cameras have sight and rotate left and right as the base behaviour.
/// If they see the player, they will stop turning and play a loud sound that alerts guards.
/// If looking at the player for too long, they will shoot him.
/// </summary>
public class SecurityCameraController : MonoBehaviour, ILockObject
{
    public ILock Lock { get; set; }
    private Directions lookDirection = Directions.None;
    private Sight _sight;
    private PlayerController _player;
    private bool _seen = false;
    private float _shootTimer = 0;
    private float _turnTimer;
    private Vector3 _defaultRotation;
    private bool _right = false;

    [Tooltip("Range of audible sound that happens when the player is spotted.")]
    public float SoundRange = 22f;
    [Tooltip("Degree to which the camera turns left and right.")]
    public float TurnDegree = 80;
    [Tooltip("Time it takes for the camera tu turn left to right and vice versa.")]
    public float TurnPeriod = 3f;
    [Tooltip("Duration it takes for the camera to take bhaviour of a turret.")]
    public float ShootDuration = 2;
    [Tooltip("Point the camera turns around.")]
    public Transform TurnPoint;

    /// <summary>
    /// Sets default look direction. 
    /// </summary>
    /// <param name="dirs"></param>
    public void SetOrientation(Directions dirs)
    {
        transform.localRotation = Quaternion.LookRotation(dirs.ToVector3(), Vector3.up);
        _defaultRotation = TurnPoint.eulerAngles;
    }

    /// <summary>
    /// Disables this script and hides the view cone.
    /// </summary>
    public void Unlock()
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            renderer.material.color = Color.black;

        _sight.VisionConeVisible = false;
        enabled = false;
    }

    private void Awake()
    {
        if (!lookDirection.None())
            transform.localRotation =
                Quaternion.LookRotation(
                    lookDirection.ToVector3());

    }

    // Start is called before the first frame update
    void Start()
    {
        _sight = GetComponentInChildren<Sight>();
        _player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool see = _sight.CanSee(_player.transform) && !_player.IsHidden;

        // Enter Sight Line
        if (!_seen && see)
        {
            GameController.AudioManager.PlayOnTarget("AlarmSiren", gameObject);
            GameController.AudioManager.AudibleEffect(gameObject, transform.position, SoundRange);
        }

        // Leave Sight Line
        if (_seen && !see)
        {
            _shootTimer = 0f;
        }

        if (see)
        {
            _shootTimer += Time.deltaTime;
            if (_shootTimer > ShootDuration)
            {
                GameController.AudioManager.PlayOnTarget("Gunshot", gameObject);
                _player.Die();
                _shootTimer %= ShootDuration;
            }
        } else // turning
        {
            _turnTimer += Time.deltaTime;

            var fromRotation = _right
                ? _defaultRotation.Added(y: -TurnDegree)
                : _defaultRotation.Added(y: TurnDegree);


            var targetRotation = _right
                ? _defaultRotation.Added(y: TurnDegree)
                : _defaultRotation.Added(y: -TurnDegree);

            TurnPoint.eulerAngles = Vector3.Lerp(fromRotation, targetRotation, _turnTimer / TurnPeriod);

            if (_turnTimer >= TurnPeriod)
            {
                _turnTimer %= TurnPeriod;
                _right = !_right;
            }
        }

        _seen = see;
    }
}
