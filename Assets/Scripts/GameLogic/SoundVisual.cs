using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SoundVisual : MonoBehaviour
{
    public float Range;
    public float Duration = 1;
    private float _time = -1;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void StartAnimation()
    {
        _time = 0;
        Destroy(gameObject, Duration);
    }

    // Update is called once per frame
    void Update()
    {
        if (_time == -1) return;
        _time += Time.deltaTime;
        float t = _time / Duration;

        var scale = Vector3.one * Range * 2;
        scale.y = 0.01f;

        transform.localScale = scale;
    }
}
