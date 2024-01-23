using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCameraController : MonoBehaviour, ILockObject
{
    private Directions lookDirection = Directions.None;
    Sight _sight;
    Transform _player;
    bool _seen = false;
    float _timer = 0;
    float _duration = 2;

    public ILock Lock { get; set; }

    public void SetOrientation(Directions dirs)
    {
        transform.localRotation = Quaternion.LookRotation(dirs.ToVector3(), Vector3.up);
    }

    public void Unlock()
    {
        Debug.Log("Security Camera Disabled");
        GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        _sight.enabled = false;
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
        _player = FindObjectOfType<Player>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        bool see = _sight.CanSee(_player);

        // Enter Sight Line
        if (!_seen && see)
        {
            GameController.AudioManager.PlayOnTarget("AlarmSiren", gameObject);
            GameController.AudioManager.AudibleEffect(gameObject, transform.position, 15);
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
                _player.GetComponent<Player>().Die();
                _timer %= _duration;
            }
        }

        _seen = see;
    }
}
