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
    private bool _paused = false;

    public Canvas PauseCanvas;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_paused) Resume();
            else Pause();
        }

    }

    public void Pause()
    {
        _paused = true;
        Time.timeScale = 0f;
        PauseCanvas.gameObject.SetActive(true);
    }

    public void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        PauseCanvas.gameObject.SetActive(false);
    }
}
