using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public SoundVisual SoundVisual;
    private List<Audition> _auditions = new List<Audition>();

    public void RegisterAudition(Audition audition) => _auditions.Add(audition);
    public void UnregisterAudition(Audition audition) => _auditions.Remove(audition);

    public void AudibleEffect(GameObject source, Vector3 sourcePosition, float distance, float heightCorrection = 0)
    {
        var visual = Instantiate(SoundVisual);
        visual.transform.position = sourcePosition;
        visual.Range = distance;
        visual.StartAnimation();

        foreach (var audition in _auditions)
            if ((audition.transform.position - sourcePosition).magnitude <= distance)
                audition.Notify(source, sourcePosition);

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
