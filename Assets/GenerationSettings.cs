using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using URandom = UnityEngine.Random;

public class GenerationSettings : MonoBehaviour
{
    public static bool DangerEnemies = true;
    public static bool DangerCameras = true;
    public static bool DangerDeathTraps = true;
    public static bool DangerSoundTraps = true;

    public static bool PatternHiddenShortcut = true;
    public static bool PatternLockedCycle = true;
    public static bool PatternDoubleLock = true;

    public static bool FloorPatternHiddenShortcut = true;
    public static bool FloorPatternLockedExtention = true;
    public static bool FloorPatternLockedAddition = true;

    public static int? Seed = null;
    public static int FloorPatternCount = 2;
    public static int PatternCount = 1;


    public Toggle DangerEnemiesToggle;
    public Toggle DangerCamerasToggle;
    public Toggle DangerDeathTrapsToggle;
    public Toggle DangerSoundTrapsToggle;

    public Toggle PatternHiddenShortcutToggle;
    public Toggle PatternLockedCycleToggle;
    public Toggle PatternDoubleLockToggle;

    public Toggle FloorPatternHiddenShortcutToggle;
    public Toggle FloorPatternLockedExtentionToggle;
    public Toggle FloorPatternLockedAdditionToggle;

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
        FloorPatternHiddenShortcutToggle.isOn = FloorPatternHiddenShortcut;
        FloorPatternLockedExtentionToggle.isOn = FloorPatternLockedExtention;
        FloorPatternLockedAdditionToggle.isOn = FloorPatternLockedAddition;
        //SeedInput.text = Seed.ToString();
        FloorCountSlider.value = FloorPatternCount;
        PatternCountSlider.value = PatternCount;

        DangerEnemiesToggle.onValueChanged.AddListener((bool val) => { DangerEnemies = val; });
        DangerCamerasToggle.onValueChanged.AddListener((bool val) => { DangerCameras = val; });
        DangerDeathTrapsToggle.onValueChanged.AddListener((bool val) => { DangerDeathTraps = val; });
        DangerSoundTrapsToggle.onValueChanged.AddListener((bool val) => { DangerSoundTraps = val; });
        PatternHiddenShortcutToggle.onValueChanged.AddListener((bool val) => { PatternHiddenShortcut = val; });
        PatternLockedCycleToggle.onValueChanged.AddListener((bool val) => { PatternLockedCycle = val; });
        PatternDoubleLockToggle.onValueChanged.AddListener((bool val) => { PatternDoubleLock = val; });
        FloorPatternHiddenShortcutToggle.onValueChanged.AddListener((bool val) => { FloorPatternHiddenShortcut = val; });
        FloorPatternLockedExtentionToggle.onValueChanged.AddListener((bool val) => { FloorPatternLockedExtention = val; });
        FloorPatternLockedAdditionToggle.onValueChanged.AddListener((bool val) => { FloorPatternLockedAddition = val; });
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

    // Update is called once per frame
    void Update()
    {

    }
}
