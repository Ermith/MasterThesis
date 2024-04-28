using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using URandom = UnityEngine.Random;

/// <summary>
/// Controls the Generation Settings UI Panel. Contains static settings for level generation.
/// </summary>
public class GenerationSettings : MonoBehaviour
{
    public static bool DangerEnemies = true;
    public static bool DangerCameras = true;
    public static bool DangerDeathTraps = true;
    public static bool DangerSoundTraps = true;

    public static bool PatternHiddenShortcut = true;
    public static bool PatternLockedCycle = true;
    public static bool PatternDoubleLock = true;
    public static bool PatternLockedFork = true;
    public static bool PatternAlternativePath = true;

    public static bool FloorPatternHiddenShortcut = true;
    public static bool FloorPatternLockedCycle = true;
    public static bool FloorPatternLockedFork = true;

    public static int? Seed = null;
    public static int FloorPatternCount = 1;
    public static int PatternCount = 2;


    public Toggle DangerEnemiesToggle;
    public Toggle DangerCamerasToggle;
    public Toggle DangerDeathTrapsToggle;
    public Toggle DangerSoundTrapsToggle;

    public Toggle PatternHiddenShortcutToggle;
    public Toggle PatternLockedCycleToggle;
    public Toggle PatternDoubleLockToggle;
    public Toggle PatternLockedForkToggle;
    public Toggle PatternAlternativePathToggle;

    public Toggle FloorPatternHiddenShortcutToggle;
    public Toggle FloorPatternLockedCycleToggle;
    public Toggle FloorPatternLockedForkToggle;


    public Sprite DangerEnemiesSprite;
    public Sprite DangerCamerasSprite;
    public Sprite DangerDeathTrapsSprite;
    public Sprite DangerSoundTrapsSprite;

    public Sprite PatternHiddenShortcutSprite;
    public Sprite PatternLockedCycleSprite;
    public Sprite PatternDoubleLockSprite;
    public Sprite PatternLockedForkSprite;
    public Sprite PatternAlternativePathSprite;

    public Sprite FloorPatternHiddenShortcutSprite;
    public Sprite FloorPatternLockedCycleSprite;
    public Sprite FloorPatternLockedForkSprite;

    public RectTransform DangerEnemiesArea;


    public TMP_InputField SeedInput;
    public Slider FloorCountSlider;
    public Slider PatternCountSlider;

    // Start is called before the first frame update
    void Start()
    {
        DangerEnemiesToggle.isOn = DangerEnemies;
        DangerCamerasToggle.isOn = DangerCameras;
        DangerDeathTrapsToggle.isOn = DangerDeathTraps;
        DangerSoundTrapsToggle.isOn = DangerSoundTraps;
        PatternHiddenShortcutToggle.isOn = PatternHiddenShortcut;
        PatternLockedCycleToggle.isOn = PatternLockedCycle;
        PatternDoubleLockToggle.isOn = PatternDoubleLock;
        PatternLockedForkToggle.isOn = PatternLockedFork;
        PatternAlternativePathToggle.isOn = PatternAlternativePath;
        FloorPatternHiddenShortcutToggle.isOn = FloorPatternHiddenShortcut;
        FloorPatternLockedCycleToggle.isOn = FloorPatternLockedCycle;
        FloorPatternLockedForkToggle.isOn = FloorPatternLockedFork;
        SeedInput.text = Seed?.ToString();
        FloorCountSlider.value = FloorPatternCount;
        PatternCountSlider.value = PatternCount;

        DangerEnemiesToggle.onValueChanged.AddListener((bool val) => { DangerEnemies = val; });
        DangerCamerasToggle.onValueChanged.AddListener((bool val) => { DangerCameras = val; });
        DangerDeathTrapsToggle.onValueChanged.AddListener((bool val) => { DangerDeathTraps = val; });
        DangerSoundTrapsToggle.onValueChanged.AddListener((bool val) => { DangerSoundTraps = val; });
        PatternHiddenShortcutToggle.onValueChanged.AddListener((bool val) => { PatternHiddenShortcut = val; });
        PatternLockedCycleToggle.onValueChanged.AddListener((bool val) => { PatternLockedCycle = val; });
        PatternDoubleLockToggle.onValueChanged.AddListener((bool val) => { PatternDoubleLock = val; });
        PatternLockedForkToggle.onValueChanged.AddListener((bool val) => { PatternLockedFork = val; });
        PatternAlternativePathToggle.onValueChanged.AddListener((bool val) => { PatternAlternativePath = val; });
        FloorPatternHiddenShortcutToggle.isOn = FloorPatternHiddenShortcut;
        FloorPatternHiddenShortcutToggle.onValueChanged.AddListener((bool val) => { FloorPatternHiddenShortcut = val; });
        FloorPatternLockedCycleToggle.onValueChanged.AddListener((bool val) => { FloorPatternLockedCycle = val; });
        FloorPatternLockedForkToggle.onValueChanged.AddListener((bool val) => { FloorPatternLockedFork = val; });
        FloorCountSlider.onValueChanged.AddListener((float val) => { FloorPatternCount = (int)val; });
        PatternCountSlider.onValueChanged.AddListener((float val) => { PatternCount = (int)val; });
        SeedInput.onValueChanged.AddListener((string val) =>
        {
            if (val.Length == 0)
                Seed = null;
            else
                Seed = int.Parse(val);
        });
    }
}
