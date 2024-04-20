using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

[RequireComponent(
    typeof(AudioManager)
    )]
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static AudioManager AudioManager => Instance.GetComponent<AudioManager>();
    private bool _paused = false;
    private bool _mapPaused = false;
    private int _objectives = 0;
    private int _objetivesFound = 0;
    public static bool IsPaused => Instance._paused || Instance._mapPaused;
    public static int Objectives { get { return Instance._objectives; } set { Instance._objectives = value; } }
    public static int ObjectivesFound { get { return Instance._objetivesFound; } set { Instance._objetivesFound = value; } }

    public Canvas PauseCanvas;
    public Canvas HUDCanvas;
    public Map Map;
    public PlayerController Player;
    public LevelGenerator LevelGenerator;
    public InfoScreen InfoScreen;
    public GameObject InteractionBar;
    public Image InteractionFill;
    public GameObject InvisOverlay;
    public GameObject Pointer;

    public GameObject InteractionText;
    private GameObject _settingsMenu;
    private GameObject _menu;
    private GameObject _generationMenu;


    // Awake is called before the Start method
    private void Awake()
    {
        Debug.Log("Game Instance");
        Instance = this;
        Time.timeScale = 1f;
        if (GenerationSettings.Seed == null)
            GenerationSettings.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        UnityEngine.Random.InitState(GenerationSettings.Seed.Value);
        Debug.Log(UnityEngine.Random.seed);
        _settingsMenu = PauseCanvas.transform.Find("SettingsPanel").gameObject;
        _menu = PauseCanvas.transform.Find("MenuPanel").gameObject;
        _generationMenu = PauseCanvas.transform.Find("GenerationSettingsPanel").gameObject;
        _objectives = 0;
        _objetivesFound = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_mapPaused) CloseMap();
            else if (_paused) Resume();
            else Pause();
        }

        if (Input.GetKeyDown(KeyCode.M) && !_paused)
        {
            if (_mapPaused) CloseMap();
            else OpenMap();
        }


        (int x, int y, int floor) = LevelGenerator.GridCoordinates(Player.transform.position);
        if (Player.Camera.Mode == CameraModeType.TopDown)
        {
            LevelGenerator.HighlightFloor(floor);
        } else
        {
            LevelGenerator.UnHilightFloors(floor);
        }


        if (Input.GetKeyDown(KeyCode.I))
        {
            InfoScreen.gameObject.SetActive(true);
            InfoScreen.SetCamoCount(Player.CamoCount);
            InfoScreen.SetTrapKitCount(Player.TrapKitCount);
            InfoScreen.SetSideObjectives(_objectives, _objetivesFound);
            InfoScreen.ClearKeys();
            foreach (IKey key in Player.Keys)
                if (key is DoorKey doorKey)
                    InfoScreen.AddKey(doorKey);

            InfoScreen.RefreshKeys();
        }

        if (Input.GetKeyUp(KeyCode.I))
            InfoScreen.gameObject.SetActive(false);

        Pointer.SetActive(Player.Camera.Mode == CameraModeType.FirstPerson);
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

    public static void OpenMap()
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);
        (int x, int y, int floor) = Instance.LevelGenerator.GridCoordinates(Instance.Player.transform.position);

        Instance._mapPaused = true;
        Time.timeScale = 0f;
        Instance.Map.gameObject.SetActive(true);
        Instance.Map.Highlight(x, y, floor);
        Instance.Map.OrientCompass(Instance.Player.transform.eulerAngles);
    }

    public static void CloseMap()
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);
        Instance._mapPaused = false;
        Time.timeScale = 1f;
        Instance.Map.gameObject.SetActive(false);
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
        Instance.InteractionText.gameObject.SetActive(true);
        Instance.InteractionText.GetComponentInChildren<TMPro.TMP_Text>().text = $"Press F to : {text}";
    }

    public static void ShowContinuousInteraction(string text, float interaction = -1)
    {
        Instance.InteractionText.gameObject.SetActive(true);
        Instance.InteractionText.GetComponentInChildren<TMPro.TMP_Text>().text = $"Hold F to : {text}";
        Instance.InteractionBar.gameObject.SetActive(interaction > 0);
        Instance.InteractionFill.fillAmount = interaction;
    }

    public static void HideInteraction()
    {
        Instance.InteractionText.gameObject.SetActive(false);
        Instance.InteractionBar.gameObject.SetActive(false);
    }

    public static void Pause()
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);
        Instance._paused = true;
        Time.timeScale = 0f;
        Instance.PauseCanvas.gameObject.SetActive(true);
        ShowCursor(true);
    }

    public static void Resume()
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);

        if (Instance._settingsMenu.activeSelf)
        {
            Instance._settingsMenu.SetActive(false);
            Instance._menu.SetActive(true);
            return;
        }

        if (Instance._generationMenu.activeSelf)
        {
            Instance._generationMenu.SetActive(false);
            Instance._menu.SetActive(true);
            return;
        }

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

    public static void SetInvisOverlay(bool active)
    {
        Instance.InvisOverlay.SetActive(active);
    }
}