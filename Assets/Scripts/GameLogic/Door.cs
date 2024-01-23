using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour, IInteractableObject, ILockObject
{
    public float Duration = 0.5f;

    private Transform _hinge;

    private Coroutine _clopenCoroutine;

    private bool _open = false;

    public ILock Lock { get; set; }

    public bool CanInteract => true;

    void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.color =
            (Lock == null) ? Color.green : Color.red;

        _hinge = transform.Find("Hinge");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact(Player player)
    {
        if (Lock != null)
        {
            Debug.Log("");

            if (player.HasKeyForLock(Lock))
                Unlock();
            else
            {
                GameController.AudioManager.Play("DoorLocked", position: transform.position);
                return;
            }
        }

        Vector3 playerDir = (player.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, playerDir);
        if (_open) Close(); else Open(dot < 0);
    }

    [ContextMenu("Open Door")]
    public void Open(bool forward)
    {
        if (_clopenCoroutine != null)
            StopCoroutine(_clopenCoroutine);

        float openAngle = forward ? -90f : 90f;
        _clopenCoroutine = StartCoroutine(ClopenCoroutine(Duration, openAngle, Easing.SmoothStep));
        GameController.AudioManager.Play("DoorOpen", position: transform.position);
        _open = true;
    }

    [ContextMenu("Close Door")]
    public void Close()
    {
        if (_clopenCoroutine != null)
            StopCoroutine(_clopenCoroutine);

        Debug.Log("Closing the door");
        _clopenCoroutine = StartCoroutine(ClopenCoroutine(Duration, 0, Easing.SmoothStep));
        GameController.AudioManager.Play("DoorCreak", position: transform.position);
        GameController.ExecuteAfter(() => GameController.AudioManager.Play("DoorShut", position: transform.position), Duration - 0.05f);
        _open = false;
    }

    IEnumerator ClopenCoroutine(float timeSpan, float endAngle, Func<float, float> easing)
    {
        float timer = 0;
        Quaternion startRotation = _hinge.localRotation;
        Quaternion endRotation = Quaternion.Euler(0, endAngle, 0);

        while (timer <= timeSpan)
        {
            float t = easing(timer / timeSpan);
            _hinge.localRotation = Quaternion.Lerp(startRotation, endRotation, t);
            timer += Time.deltaTime;
            yield return null;
        }

        _hinge.localRotation = endRotation;
        _clopenCoroutine = null;
    }

    public string InteractionPrompt()
    {
        return _open ? "Close Door" : "Open Door";
    }

    public void Unlock()
    {
        Lock = null;
        GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
}
