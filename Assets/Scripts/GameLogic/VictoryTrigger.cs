using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryTrigger : SmartCollider
{
    // Start is called before the first frame update
    void Start()
    {
        _triggerResponse += (Player player) =>
        {
            var audio = GameController.AudioManager.Play("Victory");
            GameController.ExecuteAfter(GameController.Restart, audio.clip.length);
        };
    }
}
