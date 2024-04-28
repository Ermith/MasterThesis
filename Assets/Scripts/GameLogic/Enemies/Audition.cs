using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives ability to respond to Audible Sounds. See <seealso cref="AudioManager"/> for more information.
/// </summary>
public class Audition : MonoBehaviour
{
    public delegate void SoundResponseHandler(GameObject sourceTarget, Vector3 sourcePosition);

    /// <summary>
    /// Add your function here to respond to sounds. Range is handled by the sound, not the listener.
    /// </summary>
    public event SoundResponseHandler SoundResponse;

    /// <summary>
    /// Triggers the SoundResponse
    /// </summary>
    /// <param name="sourceTarget">Who made the sound?</param>
    /// <param name="sourcePosition">Where was the sound made?</param>
    public void Notify(GameObject sourceTarget, Vector3 sourcePosition)
    {
        SoundResponse(sourceTarget, sourcePosition);
    }

    public void Start()
    {
        // AudioManager is responsible for handling Audible Sounds.
        GameController.AudioManager.RegisterAudition(this);
    }
}
