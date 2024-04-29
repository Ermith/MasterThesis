using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Controls map UI and objects. Displays a map of the given level.
/// </summary>
public class Map : MonoBehaviour
{
    public GraphDrawing<GridVertex> Drawing;
    [Tooltip("Blueprint of a room tile.")]
    public MapTile Room;
    [Tooltip("Blueprint for the lines that represent edges.")]
    public LineRenderer Edge;
    [Tooltip("Blueprint for the lines that represent grid lines.")]
    public LineRenderer GridLine;
    [HideInInspector] public GameObject MapVisual;
    [Tooltip("Duration of animation it takes to switch floors.")]
    public float Duration = 1.0f;
    [Tooltip("Text to display number of floors and the selected floor.")]
    public TMP_Text Text;
    [Tooltip("Map has a separate orthogonal camera.")]
    public Camera Camera;
    [Tooltip("Transform that contains the camera.")]
    public Transform CameraPoint;
    [Tooltip("Blueprint used to highlight the room that player is standing on.")]
    public GameObject HighlightTile;
    [Tooltip("Image rotated based on the player rotation.")]
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

    /// <summary>
    /// Creates a game object containing LinRenderers as a visual grid.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Creates the visual for the entire map. The map is divided into multiple floors.
    /// </summary>
    public void CreateMap()
    {
        MapVisual = new GameObject("MapVisual");
        MapVisual.transform.parent = transform;
        MapVisual.transform.localPosition = Vector3.zero;

        // Create objects representing floors.
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

        // Ceeates objects for each vertex reprezenting a floor.
        foreach ((GridVertex v, (int x, int y, int z)) in Drawing.VertexPositions)
        {
            var obj = GameObject.Instantiate<MapTile>(Room, _floors[z].transform);
            obj.transform.localPosition = new Vector3(x, 0, y);
            obj.UpExit = v.Top;
            obj.DownExit = v.Bottom;
            obj.SetName($"{x}.{y}");
            _mapTiles[z, x, y] = obj;
        }

        // Edges are drawn through LineRenderers.
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

    /// <summary>
    /// Starts animation to change the highlighted floor.
    /// </summary>
    /// <param name="i"></param>
    public void ChangeFloor(int i)
    {
        StartCoroutine(ChangeFloorCoroutine(i, Duration));
    }

    /// <summary>
    /// Changes floor one step up or down. Uses <see cref="ChangeFloor(int)"/>
    /// </summary>
    /// <param name="up"></param>
    public void ChangeFloor(bool up)
    {
        GameController.AudioManager.Play("Blick", volume: 0.3f);
        int step = up ? 1 : -1;
        int floor = Math.Clamp(_currentFloor + step, 0, _floors.Count - 1);

        ChangeFloor(floor);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangeFloor(true);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangeFloor(false);


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

    /// <summary>
    /// Performs the animation for changing the highlighted floor.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Sets the current floor inactive and sets the new floor active. Updates UI text.
    /// </summary>
    /// <param name="i"></param>
    private void SetVisibleFloor(int i)
    {
        _floors[_currentFloor].SetActive(false);
        _floors[i].SetActive(true);
        _currentFloor = i;
        Text.text = $"{i + 1}/{_floors.Count}";
    }

    /// <summary>
    /// Changes floor based on coordinates, see <see cref="ChangeFloor(int)"/>. Moves the _highlightTile to the position based on coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="floor"></param>
    public void Highlight(int x, int y, int floor)
    {
        _highlightTile.transform.parent = _floors[floor].transform;
        _highlightTile.transform.localPosition = new Vector3(x, -0.01f, y);

        ChangeFloor(floor);
    }

    /// <summary>
    /// Updates UI representing player's orientation.
    /// </summary>
    /// <param name="rotation"></param>
    public void OrientCompass(Vector3 rotation)
    {
        var angles = Compass.transform.eulerAngles;
        angles.z = -rotation.y + 45;
        Compass.transform.eulerAngles = angles;
    }
}
