using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GenerationToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite Sprite;
    public Image Image;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Image.sprite = Sprite;
        Image.color = new Color(1, 1, 1, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Image.sprite = null;
        Image.color = new Color(1, 1, 1, 0);
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
