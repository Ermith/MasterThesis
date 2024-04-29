using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// Top level singleton responsible for tying all of the other classes together.
/// Also contains <see cref="AudioManager"/>.
/// </summary>
[RequireComponent(
    typeof(AudioManager)
    )]
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static AudioManager AudioManager => Instance.GetComponent<AudioManager>();
    public static bool IsPaused => Instance._paused || Instance._mapPaused;
    public static int Objectives { get { return Instance._objectives; } set { Instance._objectives = value; } }
    public static int ObjectivesFound { get { return Instance._objetivesFound; } set { Instance._objetivesFound = value; } }

    [Tooltip("The menu to be shown when the game is paused.")]
    public Canvas PauseCanvas;
    [Tooltip("HUD displayed when playing the game.")]
    public Canvas HUDCanvas;
    [Tooltip("Map that will display the generated level.")]
    public Map Map;
    [Tooltip("The player...")]
    public PlayerController Player;
    [Tooltip("Used to get the generated level.")]
    public LevelGenerator LevelGenerator;
    [Tooltip("Screen that is displayed when a certain button is held.")]
    public InfoScreen InfoScreen;
    [Tooltip("Bar that shows the progress of an interaction.")]
    public GameObject InteractionBar;
    [Tooltip("Bar that fills with progress of an interaciton.")]
    public Image InteractionFill;
    [Tooltip("Overlay in first person when player is hidden or invisible.")]
    public GameObject InvisOverlay;
    [Tooltip("Crosshairs.")]
    public GameObject Pointer;
    [Tooltip("Text that displays what interaction does.")]
    public GameObject InteractionText;

    private GameObject _settingsMenu;
    private GameObject _menu;
    private GameObject _generationMenu;
    private bool _paused = false;
    private bool _mapPaused = false;
    private int _objectives = 0;
    private int _objetivesFound = 0;


    // Awake is called before the Start method
    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        // Init Random class
        if (GenerationSettings.Seed == null)
            GenerationSettings.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(GenerationSettings.Seed.Value);
        Debug.Log($"Seed: {GenerationSettings.Seed.Value}");

        _settingsMenu = PauseCanvas.transform.Find("SettingsPanel").gameObject;
        _menu = PauseCanvas.transform.Find("MenuPanel").gameObject;
        _generationMenu = PauseCanvas.transform.Find("GenerationSettingsPanel").gameObject;
        _objectives = 0;
        _objetivesFound = 0;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePauseAndMap();
        HandleInfoScreen();

        // When in top-down view, all of the upper floors and a roof is not rendered
        (int _, int _, int floor) = LevelGenerator.GridCoordinates(Player.transform.position);
        if (Player.Camera.Mode == CameraModeType.TopDown)
        {
            LevelGenerator.HighlightFloor(floor);
        } else
        {
            LevelGenerator.UnHilightFloors(floor);
        }

        // Render crosshairs only in the first person
        Pointer.SetActive(Player.Camera.Mode == CameraModeType.FirstPerson && !_paused && !_mapPaused);
    }

    /// <summary>
    /// Shows and Hides Map and Pause Screen. 'ESC' backs out of both of them.
    /// </summary>
    private void HandlePauseAndMap()
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
    }

    /// <summary>
    /// Holding an appropriate button shows the current progress and inventory. 
    /// Needs to be updated when shown.
    /// </summary>
    private void HandleInfoScreen()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
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

        if (Input.GetKeyUp(KeyCode.Tab))
            InfoScreen.gameObject.SetActive(false);

    }

    /// <summary>
    /// Just reload the current screen.
    /// </summary>
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

    /// <summary>
    /// Activates coroutine to do action after a given ammount of time.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="time"></param>
    public static void ExecuteAfter(Action action, float time)
    {
        Instance.StartCoroutine(WaitCoroutine(action, time));
    }

    public static void ExecuteAfterUnscaled(Action action, float time)
    {
        Instance.StartCoroutine(WaitCoroutineUnscaled(action, time));
    }

    /// <summary>
    /// Initilizes the map with player's direction and location.
    /// </summary>
    public static void OpenMap()
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);
        (int x, int y, int floor) = Instance.LevelGenerator.GridCoordinates(Instance.Player.transform.position);

        Instance._mapPaused = true;
        Time.timeScale = 0f;
        Instance.Map.gameObject.SetActive(true);
        Instance.Map.Highlight(x, y, floor);
        Instance.Map.OrientCompass(Instance.Player.Camera.GetGroundRotation());
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

    /// <summary>
    /// Shows a given interaction prompt on screen.
    /// "Press F to: {interaction}." 
    /// </summary>
    /// <param name="text"></param>
    public static void ShowInteraction(string text)
    {
        Instance.InteractionText.gameObject.SetActive(true);
        Instance.InteractionText.GetComponentInChildren<TMPro.TMP_Text>().text = $"Press F to : {text}";
    }

    /// <summary>
    /// Shows a given interaction prompt on screen.
    /// "Hold F to {interaction}."
    /// Continous interaction requires holding the button and fills a bar based on progress.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="interaction"></param>
    public static void ShowContinuousInteraction(string text, float interaction = -1)
    {
        Instance.InteractionText.gameObject.SetActive(true);
        Instance.InteractionText.GetComponentInChildren<TMPro.TMP_Text>().text = $"Hold F to : {text}";
        Instance.InteractionBar.gameObject.SetActive(interaction > 0);
        Instance.InteractionFill.fillAmount = interaction;
    }

    /// <summary>
    /// Hides the interaction prompt, whether continuous or normal.
    /// </summary>
    public static void HideInteraction()
    {
        Instance.InteractionText.gameObject.SetActive(false);
        Instance.InteractionBar.gameObject.SetActive(false);
    }

    /// <summary>
    /// Opens the pause menu.
    /// </summary>
    public static void Pause()
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);
        Instance._paused = true;
        Time.timeScale = 0f;
        Instance.PauseCanvas.gameObject.SetActive(true);
        ShowCursor(true);
    }

    /// <summary>
    /// Closes the pause menu and resumes the game.
    /// </summary>
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

    /// <summary>
    /// Shows or hides the cursor. To be used for example when opening a menu.
    /// </summary>
    /// <param name="show"></param>
    private static void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    /// <summary>
    /// Loads the GameScene
    /// </summary>
    public static void NewGame()
    {
        SceneManager.LoadScene("GameScene");
        ShowCursor(false);
    }

    /// <summary>
    /// Loads the MainMenu scene.
    /// </summary>
    public static void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
        ShowCursor(true);
    }

    /// <summary>
    /// Blue overlay to showcase that the player is hidden.
    /// </summary>
    /// <param name="active"></param>
    public static void SetInvisOverlay(bool active)
    {
        Instance.InvisOverlay.SetActive(active);
    }
}