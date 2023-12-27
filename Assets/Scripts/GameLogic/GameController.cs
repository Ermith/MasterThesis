using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(
    typeof(AudioManager)
    )]
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static AudioManager AudioManager => Instance.GetComponent<AudioManager>();

    public static void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void ExecuteAfter(Action action, float time)
    {
        StartCoroutine(WaitCoroutine(action, time));
    }

    private IEnumerator WaitCoroutine(Action action, float wait)
    {
        yield return new WaitForSeconds(wait);

        action();
    }

    // Awake is called before the Start method
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
