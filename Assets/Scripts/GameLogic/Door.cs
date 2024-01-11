using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour, UsableObject
{
    public DoorLock DoorLock = null;
    public float Duration = 0.5f;

    private Transform _hinge;

    private Coroutine _clopenCoroutine;

    private bool _open = false;

    void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.color =
            (DoorLock == null) ? Color.green : Color.red;

        _hinge = transform.Find("Hinge");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Use()
    {
        if (_open) Close(); else Open();
    }

    [ContextMenu("Open Door")]
    public void Open()
    {
        if (_clopenCoroutine != null)
            StopCoroutine(_clopenCoroutine);

        _clopenCoroutine = StartCoroutine(ClopenCoroutine(Duration, 90, Easing.SmoothStep));
        _open = true;
    }

    [ContextMenu("Close Door")]
    public void Close()
    {
        if (_clopenCoroutine != null)
            StopCoroutine(_clopenCoroutine);

        Debug.Log("Closing the door");
        _clopenCoroutine = StartCoroutine(ClopenCoroutine(Duration, 0, Easing.SmoothStep));
        _open = false;
    }

    IEnumerator ClopenCoroutine(float timeSpan, float endRotation, Func<float, float> easing)
    {
        float timer = 0;
        float startRotation = _hinge.localRotation.eulerAngles.y;
        float rotationSpan = (endRotation - startRotation);

        while (timer <= timeSpan)
        {
            float t = easing(timer / timeSpan);
            float yRotation = startRotation + rotationSpan * t;
            _hinge.localRotation = Quaternion.Euler(0, yRotation, 0);
            timer += Time.deltaTime;
            yield return null;
        }

        _hinge.localRotation = Quaternion.Euler(0, endRotation, 0);
        _clopenCoroutine = null;
    }
}
