using System;
using UnityEngine;

/// <summary>
/// Contains audio clip for <see cref="AudioManager"/>.
/// </summary>
[System.Serializable]
public class Sound
{
    public string Name;

    public AudioClip Clip;

    [Range(0f, 1f)]
    public float Volume = 1f;
    [Range(0.1f, 3f)]
    public float Pitch = 1f;
    [Range(0f, 1f)]
    public float SpacialBlend = 0f;

    //public bool Loop = false;


    [HideInInspector]
    public AudioSource AudioSource;
}