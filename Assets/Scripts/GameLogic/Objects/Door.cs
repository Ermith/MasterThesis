using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Can be opened or closed by interaction. Can be locked by a <see cref="DoorLock"/>. Is unlocked if the player has appropriate <see cref="DoorKey"/>.
/// </summary>
public class Door : MonoBehaviour, IInteractableObject, ILockObject
{
    private Transform _hinge;
    private Coroutine _clopenCoroutine;
    private bool _open = false;

    [Tooltip("Time it takes to open or close the door.")]
    public float Duration = 0.5f;
    [Tooltip("Does the door change color if locked?")]
    public bool ChangeColor = true;
    [HideInInspector] public ILock Lock { get; set; }

    [HideInInspector] public bool CanInteract => true;

    [HideInInspector] public InteractionType InteractionType => InteractionType.Single;

    void Start()
    {
        if (ChangeColor)
            GetComponentInChildren<MeshRenderer>().material.color =
                (Lock == null) ? Color.green : Color.red;

        _hinge = transform.Find("Hinge");
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Opens or closes the door. If the door is locked, it is instantly unlocked if the player has appropriate <see cref="DoorKey"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public float Interact(PlayerController player)
    {
        if (Lock != null)
        {
            if (player.HasKeyForLock(Lock))
                Unlock();
            else
            {
                GameController.AudioManager.Play("DoorLocked", position: transform.position);
                return -1;
            }
        }

        Vector3 playerDir = (player.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, playerDir);
        if (_open) Close(); else Open(dot < 0);

        return -1;
    }

    /// <summary>
    /// Starts open door animation based on the Duration property.
    /// </summary>
    /// <param name="forward">Which way to open?</param>
    public void Open(bool forward)
    {
        if (_clopenCoroutine != null)
            StopCoroutine(_clopenCoroutine);

        float openAngle = forward ? -90f : 90f;
        _clopenCoroutine = StartCoroutine(ClopenCoroutine(Duration, openAngle, Easing.SmoothStep));
        GameController.AudioManager.Play("DoorOpen", position: transform.position);
        _open = true;
    }

    /// <summary>
    /// Starts closing door animation based on the Duration property.
    /// </summary>
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

    /// <summary>
    /// Coroutine for opening and closing the door animations.
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="endAngle"></param>
    /// <param name="easing"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Removes the lock and changes color.
    /// </summary>
    public void Unlock()
    {
        Lock = null;

        if (ChangeColor)
            GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
}
