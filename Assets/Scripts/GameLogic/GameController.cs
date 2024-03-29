using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

[RequireComponent(
    typeof(AudioManager)
    )]
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static AudioManager AudioManager => Instance.GetComponent<AudioManager>();
    private bool _paused = false;
    public static bool IsPaused => Instance._paused;

    public Canvas PauseCanvas;
    public Canvas HUDCanvas;

    private Transform _interactText;


    // Awake is called before the Start method
    private void Awake()
    {
        Debug.Log("Game Instance");
        Instance = this;
        Time.timeScale = 1f;
        UnityEngine.Random.InitState(-488536290);
        Debug.Log(UnityEngine.Random.seed);
        _interactText = HUDCanvas.transform.Find("Interact Text");
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

    public static void Restart()
    {
        //Instance.StartCoroutine(RestartLoad());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static IEnumerator RestartLoad()
    {
        var a = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        while (!a.isDone) yield return null;
        // yield return new WaitForEndOfFrame();
        // Reset the player here
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //yield return null;
    }

    public static void ExecuteAfter(Action action, float time)
    {
        Instance.StartCoroutine(WaitCoroutine(action, time));
    }

    public static void ExecuteAfterUnscaled(Action action, float time)
    {
        Instance.StartCoroutine(WaitCoroutine(action, time));
    }

    private static IEnumerator WaitCoroutine(Action action, float wait)
    {
        yield return new WaitForSeconds(wait);

        action();
    }

    private static IEnumerator WaitCoroutineUnscaled(Action action, float wait)
    {
        yield return new WaitForSecondsRealtime(wait);
        action();
    }

    public static void ShowInteraction(string text)
    {
        Instance._interactText.gameObject.SetActive(true);
        Instance._interactText.GetComponentInChildren<TMPro.TMP_Text>().text = $"Press F to : {text}";
    }

    public static void HideInteraction()
    {
        Instance._interactText.gameObject.SetActive(false);
    }

    public static void Pause()
    {
        Instance._paused = true;
        Time.timeScale = 0f;
        Instance.PauseCanvas.gameObject.SetActive(true);
        ShowCursor(true);
    }

    public static void Resume()
    {
        Instance._paused = false;
        Time.timeScale = 1f;
        Instance.PauseCanvas.gameObject.SetActive(false);
        ShowCursor(false);
    }

    private static void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    public static void NewGame()
    {
        SceneManager.LoadScene("GameScene");
        ShowCursor(false);
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
        ShowCursor(true);
    }
}