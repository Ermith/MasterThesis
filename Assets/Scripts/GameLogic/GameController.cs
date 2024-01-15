using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
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
    public static bool IsPaused => Instance._paused;

    public Canvas PauseCanvas;
    public Canvas HUDCanvas;

    private Transform _interactText;

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
        Debug.Log("Game Instance");
        Instance = this;
        Time.timeScale = 1f;
        //UnityEngine.Random.InitState(-488536290);
        Debug.Log(UnityEngine.Random.seed);
        _interactText = HUDCanvas.transform.Find("Interact Text");
    }

    // Start is called before the first frame update
    void Start()
    {
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
        ShowCursor(true);
    }

    public void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        PauseCanvas.gameObject.SetActive(false);
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
