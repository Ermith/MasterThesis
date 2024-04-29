using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using URandom = UnityEngine.Random;

/// <summary>
/// Responsible for playing audios and audible sounds that Audition components respond to. Also contains references to all Audition components.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region SERIALIZED_FIELDS
    [SerializeField, Tooltip("Music plays on a loop throught the game.")]
    public Sound Music;

    [SerializeField, Tooltip("Sounds can be called by their name in scripts.")]
    public Sound[] Sounds;

    [SerializeField, Tooltip("Footsteps for different occasions")]
    public Sound[] StepSounds;

    [SerializeField, Tooltip("Debug visual for audible effects that enemies can hear.")]
    public SoundVisual SoundVisual;
    #endregion

    private List<Audition> _auditions = new();
    private Dictionary<string, List<Sound>> _stepDictionary = new();

    #region AUDIBLE EFFECTS
    public void RegisterAudition(Audition audition) => _auditions.Add(audition);
    public void UnregisterAudition(Audition audition) => _auditions.Remove(audition);
    /// <summary>
    /// Creates a visual effect and alerts all auditions in distance from source position.
    /// </summary>
    /// <param name="source">Who caused the sound.</param>
    /// <param name="sourcePosition">The location of the sound.</param>
    /// <param name="distance">Radius in which auditions will be alerted.</param>
    public void AudibleEffect(GameObject source, Vector3 sourcePosition, float distance)
    {
        var visual = Instantiate(SoundVisual);
        visual.transform.position = sourcePosition;
        visual.Range = distance;
        visual.StartAnimation();

        foreach (var audition in _auditions)
            if ((audition.transform.position - sourcePosition).magnitude <= distance)
                audition.Notify(source, sourcePosition);

    }
    #endregion

    #region SOUNDS
    /// <summary>
    /// Plays an audio by attatching <see cref="AudioSource"/> onto target game object. May destroy that object in the end if specified.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="target"></param>
    /// <param name="loop"></param>
    /// <param name="destroyTarget"></param>
    /// <returns></returns>
    public AudioSource PlayOnTarget(string name, GameObject target, bool loop = false, bool destroyTarget = false)
    {
        Sound sound = null;
        foreach (Sound s in Sounds)
            if (s.Name == name)
                sound = s;

        if (sound == null)
            return null;

        var audio = target.AddComponent<AudioSource>();
        audio.clip = sound.Clip;
        audio.volume = sound.Volume;// * Settings.SFXVolume;
        audio.pitch = sound.Pitch;
        audio.loop = loop;
        audio.spatialBlend = sound.SpacialBlend;
        audio.dopplerLevel = 0;
        StartCoroutine(PlayOnTargetCoroutine(audio, destroyTarget));

        return audio;
    }

    public void Stop(string name)
    {
        foreach (Sound s in Sounds)
            if (s.Name == name)
            {
                s?.AudioSource.Stop();
                return;
            }
    }

    public void StopLoop(string name)
    {
        foreach (Sound s in Sounds)
            if (s.Name == name)
            {
                s.AudioSource.loop = false;
                return;
            }
    }

    /// <summary>
    /// Plays an audio. If the position is specified, creates a new game object on that position and attattches the <see cref="AudioSource"/> onto that object.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="loop"></param>
    /// <param name="position"></param>
    /// <param name="pitch"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public AudioSource Play(string name, bool loop = false, Vector3? position = null, float? pitch = null, float? volume = null)
    {
        Sound sound = null;
        foreach (Sound s in Sounds)
            if (s.Name == name)
                sound = s;

        if (sound == null)
            return null;

        if (position != null)
        {
            var go = new GameObject(sound.Name);
            go.transform.position = position.Value;
            var audio = go.gameObject.AddComponent<AudioSource>();

            audio.clip = sound.Clip;
            audio.volume = (volume == null) ? sound.Volume : volume.Value;
            audio.pitch = (pitch == null) ? sound.Pitch : pitch.Value;
            audio.spatialBlend = sound.SpacialBlend;
            audio.loop = loop;
            audio.dopplerLevel = 0;

            StartCoroutine(PlaySpacialSoundCoroutine(audio));
            return audio;
        }

        sound.AudioSource.volume = sound.Volume * GameSettings.Volume;
        sound.AudioSource.pitch = (pitch == null) ? sound.Pitch : pitch.Value;
        sound.AudioSource.spatialBlend = 0;
        sound.AudioSource.dopplerLevel = 0;

        sound.AudioSource.loop = loop;
        sound.AudioSource.Play();
        return sound.AudioSource;
    }

    /// <summary>
    /// Playes a random step sound of a given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="target"></param>
    /// <param name="volume"></param>
    /// <param name="pitch"></param>
    /// <param name="spacialBlend"></param>
    /// <param name="destroyTarget"></param>
    /// <returns></returns>
    public AudioSource PlayStep(string name, GameObject target, float? volume = null, float? pitch = null, float? spacialBlend = null, bool destroyTarget = false)
    {
        List<Sound> stepVariations = _stepDictionary[name];
        int index = URandom.Range(0, stepVariations.Count);
        var source = stepVariations[index].AudioSource;

        GameObject obj = new GameObject(name);
        var audio = obj.AddComponent<AudioSource>();
        obj.transform.parent = target.transform;
        obj.transform.localPosition = Vector3.zero;

        audio.clip = source.clip;
        audio.volume = volume == null ? source.volume : volume.Value;
        audio.pitch = pitch == null ? source.pitch : pitch.Value;
        audio.spatialBlend = spacialBlend == null ? source.spatialBlend : spacialBlend.Value;

        StartCoroutine(PlayOnTargetCoroutine(audio, destroyTarget));
        return audio;
    }

    private IEnumerator PlaySpacialSoundCoroutine(AudioSource audio)
    {
        audio.volume *= GameSettings.Volume;
        audio.Play();

        while (audio.isPlaying)
            yield return null;

        Destroy(audio.gameObject);
    }

    private IEnumerator PlayOnTargetCoroutine(AudioSource audio, bool destroyTarget)
    {
        audio.volume *= GameSettings.Volume;
        audio.Play();
        while (audio.isPlaying)
            yield return null;

        if (destroyTarget)
            Destroy(audio.gameObject);
        else
            Destroy(audio);
    }
    #endregion

    private void Awake()
    {
        foreach (Sound s in Sounds)
        {
            s.AudioSource = gameObject.AddComponent<AudioSource>();
            s.AudioSource.clip = s.Clip;

            s.AudioSource.volume = s.Volume;
            s.AudioSource.pitch = s.Pitch;
            s.AudioSource.spatialBlend = s.SpacialBlend;
            s.AudioSource.dopplerLevel = 0;
        }

        if (Music == null)
            return;

        Music.AudioSource = gameObject.AddComponent<AudioSource>();
        Music.AudioSource.clip = Music.Clip;
        Music.AudioSource.volume = Music.Volume;// * Settings.MusicVolume;
        Music.AudioSource.pitch = Music.Pitch;
        Music.AudioSource.spatialBlend = Music.SpacialBlend;
        Music.AudioSource.dopplerLevel = 0;

        Music.AudioSource.loop = true;
        Music.AudioSource.volume *= GameSettings.Volume;
        Music.AudioSource.Play();

        foreach (Sound s in StepSounds)
        {
            s.AudioSource = gameObject.AddComponent<AudioSource>();
            s.AudioSource.clip = s.Clip;

            s.AudioSource.volume = s.Volume;
            s.AudioSource.pitch = s.Pitch;
            s.AudioSource.spatialBlend = s.SpacialBlend;
            s.AudioSource.dopplerLevel = 0;

            string key = s.Name.Split('_')[0];
            if (!_stepDictionary.ContainsKey(key))
                _stepDictionary[key] = new List<Sound>();

            _stepDictionary[key].Add(s);
        }
    }
}
