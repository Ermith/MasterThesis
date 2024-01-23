using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _maxDistance = 100;
    private float _duration = 0.2f;
    private float _time = 0f;
    private Vector3 _from;
    private Vector3 _to;
    private bool _active = false;
    private Action<Vector3, GameObject> _onHit;
    private GameObject _target;

    // Update is called once per frame
    void Update()
    {
        if (!_active) return;

        _time += Time.deltaTime;
        float t = Mathf.Min(_time / _duration, 1);
        transform.position = _from + (_to - _from) * t;

        if (t >= 1)
        {
            _onHit?.Invoke(transform.position, _target);
            Destroy(gameObject);
        }
    }

    public void Shoot(Vector3 from, Vector3 dir, Action<Vector3, GameObject> onHit)
    {
        if (_active) return;
        _active = true;

        _onHit = onHit;
        transform.position = from;
        _from = from;
        _to = from + dir * _maxDistance;

        if(Physics.Raycast(from, dir, out RaycastHit hitInfo))
        {
            _to = hitInfo.point;
            _target = hitInfo.collider.gameObject;
        }
    }
}
