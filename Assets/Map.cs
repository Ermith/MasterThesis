using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GraphDrawing<GridVertex> Drawing;
    public GameObject Room;
    public LineRenderer Edge;
    public LineRenderer GridLine;
    public GameObject MapVisual;
    public float Duration = 1.0f;
    public TMP_Text Text;

    private List<GameObject> _floors = new();
    private int _currentFloor;

    public void Start()
    {
        MapVisual = new GameObject("MapVisual");
        MapVisual.transform.parent = transform;
        MapVisual.transform.localPosition = Vector3.zero;
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
        for (int i = 0; i < Drawing.MaximumZ + 1; i++)
        {
            var floor = new GameObject($"Floor {i}");
            floor.transform.parent = MapVisual.transform;
            floor.transform.localPosition = new Vector3(0, i, 0);
            var grid = CreateGrid();
            grid.transform.parent = floor.transform;
            grid.transform.localPosition = (Vector3.left + Vector3.back) * 0.5f;
            _floors.Add(floor);
        }

        foreach ((GridVertex _, (int x, int y, int z)) in Drawing.VertexPositions)
        {
            var obj = GameObject.Instantiate(Room, _floors[z].transform);
            obj.transform.localPosition = new Vector3(x, 0, y);
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
    }

    public void ChangeFloor(int i)
    {
        StartCoroutine(ChangeFloorCoroutine(i, Duration));
    }

    public void IncreaseFloor(bool up)
    {
        int step = up ? 1 : -1;
        ChangeFloor((_currentFloor + step + _floors.Count) % _floors.Count);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            IncreaseFloor(true);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            IncreaseFloor(false);
    }

    private IEnumerator ChangeFloorCoroutine(int i, float duration)
    {
        float timer = 0;
        Vector3 from = MapVisual.transform.localPosition;
        Vector3 to = new Vector3(0, -i, 0);

        _floors[_currentFloor].SetActive(false);
        _floors[i].SetActive(true);
        _currentFloor = i;
        Text.text = $"{i + 1}/{_floors.Count}";

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            MapVisual.transform.localPosition = Vector3.Lerp(from, to, Easing.SmoothStep(t));
            yield return null;
        }
    }
}
