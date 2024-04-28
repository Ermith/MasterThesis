using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains static settings for the game. Namely Volume and Mouse Sensitivity. Also controls the Game Settings Panel.
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static float Volume = 0.8f;
    public static float MouseSensitivity = 0.35f;

    public Slider VolumeSlider;
    public Slider MouseSensitivitySlider;
    public Toggle FullscreenToggle;

    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        VolumeSlider.value = Volume;
        MouseSensitivitySlider.value = MouseSensitivity;
        FullscreenToggle.isOn = Screen.fullScreen;

        VolumeSlider.onValueChanged.AddListener((float volume) => { Volume = volume; });
        MouseSensitivitySlider.onValueChanged.AddListener((float sensitivity) => { MouseSensitivity = sensitivity; });
        FullscreenToggle.onValueChanged.AddListener((bool fullscreen) =>
        {
            Screen.fullScreen = fullscreen;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
