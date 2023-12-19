using System;
using UnityEngine;

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