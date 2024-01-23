using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audition : MonoBehaviour
{
    public delegate void SoundResponseHandler(GameObject sourceTarget, Vector3 sourcePosition);
    public event SoundResponseHandler SoundResponse;

    public void Notify(GameObject sourceTarget, Vector3 sourcePosition)
    {
        SoundResponse(sourceTarget, sourcePosition);
    }

    public void Start()
    {
        GameController.AudioManager.RegisterAudition(this);
    }
}
