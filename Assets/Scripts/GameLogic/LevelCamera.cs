using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LevelCamera : MonoBehaviour
{
    private Camera _camera;
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic = true;
    }

    public void SetPosition(int width, int height, float scale, Vector3 offset)
    {
        Vector3 pos = offset + Vector3.up * 10;
        pos.x += width / 2 * scale;
        pos.z += height / 2 * scale;
        transform.position = pos;
        transform.eulerAngles = new Vector3(90, 0, 0);
        
        float sizeH = (height / 2 * scale);
        float sizeW = (width / _camera.aspect * scale / 2);// * 1.25f;
    
        _camera.orthographicSize = Mathf.Max(sizeH, sizeW) * 1.1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
