using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GraphDrawing<GridVertex> Drawing;
    public MapTile Room;
    public LineRenderer Edge;
    public LineRenderer GridLine;
    public GameObject MapVisual;
    public float Duration = 1.0f;
    public TMP_Text Text;
    public Camera Camera;
    public Transform CameraPoint;
    public GameObject HighlightTile;
    public GameObject Compass;

    private List<GameObject> _floors = new();
    private MapTile[,,] _mapTiles;
    private int _currentFloor;
    private float _maxZoom;
    private float _minZoom;
    private MapTile _highlightedTile;
    private GameObject _highlightTile;

    public void Awake()
    {

    }

    private GameObject CreateGrid()
    {
        GameObject go = new GameObject("Grid");

        for (int x = 0; x <= Drawing.MaximumX + 1; x++)
        {
            var line = GameObject.Instantiate<LineRenderer>(GridLine);
            line.positionCount = 2;
            line.SetPositions(new Vector3[]
            {
                new Vector3(x, 0, 0),
                new Vector3(x, 0, Drawing.MaximumY + 1)
            });
            line.transform.parent = go.transform;
            line.transform.localPosition = Vector3.zero;
        }

        for (int y = 0; y <= Drawing.MaximumY + 1; y++)
        {
            var line = GameObject.Instantiate<LineRenderer>(GridLine);
            line.positionCount = 2;
            line.SetPositions(new Vector3[]
            {
                new Vector3(0, 0, y),
                new Vector3(Drawing.MaximumX + 1, 0, y)
            });
            line.transform.parent = go.transform;
            line.transform.localPosition = Vector3.zero;
        }

        return go;
    }

    public void CreateMap()
    {
        MapVisual = new GameObject("MapVisual");
        MapVisual.transform.parent = transform;
        MapVisual.transform.localPosition = Vector3.zero;

        for (int i = 0; i < Drawing.MaximumZ + 1; i++)
        {
            var floor = new GameObject($"Floor {i}");
            floor.transform.parent = MapVisual.transform;
            floor.transform.localPosition = new Vector3(0, i, 0);
            var grid = CreateGrid();
            grid.transform.parent = floor.transform;
            grid.transform.localPosition = (Vector3.left + Vector3.back) * 0.5f;
            _floors.Add(floor);
            floor.SetActive(false);
        }

        _mapTiles = new MapTile[Drawing.MaximumZ + 1, Drawing.MaximumX + 1, Drawing.MaximumY + 1];

        foreach ((GridVertex v, (int x, int y, int z)) in Drawing.VertexPositions)
        {
            var obj = GameObject.Instantiate<MapTile>(Room, _floors[z].transform);
            obj.transform.localPosition = new Vector3(x, 0, y);
            obj.UpExit = v.Top;
            obj.DownExit = v.Bottom;
            obj.SetName(v.ToString());
            _mapTiles[z, x, y] = obj;
        }

        foreach ((IEdge<GridVertex> edge, List<(int, int, int)> positions) in Drawing.EdgePositions)
        {
            var e = edge as GridEdge;
            if (e.From.Position.z != e.To.Position.z)
                continue;

            var line = GameObject.Instantiate<LineRenderer>(Edge);
            var realPositions = positions.Select(((int, int, int) element, int index) => new Vector3(element.Item1, 0, element.Item2));
            var r = realPositions.ToArray();
            line.positionCount = r.Length;
            line.SetPositions(realPositions.ToArray());
            line.transform.parent = _floors[positions[0].Item3].transform;
            line.transform.localPosition = new Vector3(0, 0.01f, 0);
        }

        float a = Drawing.MaximumX + 1;
        float b = Drawing.MaximumY + 1;
        _maxZoom = Mathf.Sqrt(a * a + b * b) / 2;
        Camera.orthographicSize = _maxZoom;
        CameraPoint.localPosition = new Vector3(a / 2 - 0.5f, 0, b / 2 - 0.5f);

        SetVisibleFloor(0);
        _highlightTile = GameObject.Instantiate(HighlightTile);
    }

    public void ChangeFloor(int i)
    {
        StartCoroutine(ChangeFloorCoroutine(i, Duration));
    }

    public void IncreaseFloor(bool up)
    {
        int step = up ? 1 : -1;
        int floor = Math.Clamp(_currentFloor + step, 0, _floors.Count - 1);

        ChangeFloor(floor);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            IncreaseFloor(true);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            IncreaseFloor(false);


        var zoom = Camera.orthographicSize;

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            zoom = Mathf.Min(zoom + 0.5f, _maxZoom);
            Camera.orthographicSize = zoom;
        }

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            zoom = Mathf.Max(zoom - 0.5f, 1.5f);
            Camera.orthographicSize = zoom;
        }

        Directions dir = Directions.None;
        if (Input.GetKey(KeyCode.W)) dir |= Directions.North;
        if (Input.GetKey(KeyCode.S)) dir |= Directions.South;
        if (Input.GetKey(KeyCode.D)) dir |= Directions.East;
        if (Input.GetKey(KeyCode.A)) dir |= Directions.West;
        var vec = dir.ToVector3().normalized;
        vec = Quaternion.Euler(0, CameraPoint.rotation.eulerAngles.y, 0) * vec;
        var pos = CameraPoint.localPosition + vec * 0.05f * zoom;
        pos.x = Mathf.Clamp(pos.x, 0, Drawing.MaximumX + 1);
        pos.z = Mathf.Clamp(pos.z, 0, Drawing.MaximumY + 1);




        CameraPoint.localPosition = pos;
    }

    private IEnumerator ChangeFloorCoroutine(int i, float duration)
    {
        float timer = 0;
        Vector3 from = MapVisual.transform.localPosition;
        Vector3 to = new Vector3(0, -i, 0);

        SetVisibleFloor(i);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            MapVisual.transform.localPosition = Vector3.Lerp(from, to, Easing.SmoothStep(t));
            yield return null;
        }
    }

    private void SetVisibleFloor(int i)
    {
        _floors[_currentFloor].SetActive(false);
        _floors[i].SetActive(true);
        _currentFloor = i;
        Text.text = $"{i + 1}/{_floors.Count}";
    }

    public void Highlight(int x, int y, int floor)
    {
        _highlightTile.transform.parent = _floors[floor].transform;
        _highlightTile.transform.localPosition = new Vector3(x, -0.01f, y);

        ChangeFloor(floor);
    }

    public void OrientCompass(Vector3 rotation)
    {
        var angles = Compass.transform.eulerAngles;
        angles.z = -rotation.y + 45;
        Compass.transform.eulerAngles = angles;
    }
}
