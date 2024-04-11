using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    public TMP_Text TileName;
    public TMP_Text Down;
    public TMP_Text Up;
    public Color HighLightColor = Color.yellow;

    private Material _material;
    private Color _originalColor;

    public bool UpExit
    {
        get => Up.gameObject.activeSelf;
        set { Up.gameObject.SetActive(value); }
    }

    public bool DownExit
    {
        get => Down.gameObject.activeSelf;
        set { Down.gameObject.SetActive(value); }
    }

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponentInChildren<MeshRenderer>().material;
        _originalColor = _material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetName(string name)
    {
        TileName.text = name;
    }

    public void Highlight(bool highlighted)
    {
        _material.color = highlighted ? HighLightColor : _originalColor;
    }
}
