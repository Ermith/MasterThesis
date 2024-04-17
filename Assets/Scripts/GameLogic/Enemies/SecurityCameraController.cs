using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCameraController : MonoBehaviour, ILockObject
{
    private Directions lookDirection = Directions.None;
    Sight _sight;
    PlayerController _player;
    bool _seen = false;
    float _timer = 0;
    float _duration = 2;

    public ILock Lock { get; set; }
    public float SoundRange = 22f;
    public float TurnDegree = 80;
    public float TurnPeriod = 3f;
    public Transform TurnPoint;

    private float _turnTimer;
    private Vector3 _defaultRotation;
    private bool _right = false;

    public void SetOrientation(Directions dirs)
    {
        transform.localRotation = Quaternion.LookRotation(dirs.ToVector3(), Vector3.up);
        _defaultRotation = transform.eulerAngles;
    }

    public void Unlock()
    {
        Debug.Log("Security Camera Disabled");
        GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        //_sight.enabled = false;
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
            _timer = 0f;
        }

        if (see)
        {
            _timer += Time.deltaTime;
            if (_timer > _duration)
            {
                GameController.AudioManager.PlayOnTarget("Gunshot", gameObject);
                _player.GetComponent<PlayerController>().Die();
                _timer %= _duration;
            }
        } else // turning
        {
            _turnTimer += Time.deltaTime;

            var fromRotation = _right
                ? new Vector3(0, -TurnDegree, 0)
                : new Vector3(0, TurnDegree, 0);


            var targetRotation = _right
                ? new Vector3(0, TurnDegree, 0)
                : new Vector3(0, -TurnDegree, 0);

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
