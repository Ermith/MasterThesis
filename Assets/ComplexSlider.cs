using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComplexSlider : MonoBehaviour
{
    public TMP_Text Text;
    public Slider Slider;
    // Start is called before the first frame update
    void Start()
    {
        Text.text = Slider.value.ToString();
        Slider.onValueChanged.AddListener((float val) => { Text.text = val.ToString(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
