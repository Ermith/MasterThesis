using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer), typeof(MeshCollider)), ExecuteInEditMode]
public class Stairs : MonoBehaviour
{
    private Mesh _mesh;
    private MeshCollider _collider;

    [Range(1, 100)]
    public int Steps;

    // Start is called before the first frame update
    void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        Rebuild();
    }

    // Update is called once per frame
    void Update()
    {
        Rebuild();
    }

    private void Rebuild()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();

        for (int step = 1; step <= Steps; step++)
        {
            float current = (float)step / Steps * 2 - 1; // -0.4
            float previous = (float)(step - 1) / Steps * 2 - 1; // -1
            float next = (float)(step + 1) / Steps * 2 - 1;

            AddQuad(
                new Vector3(1, current, previous),
                new Vector3(1, current, 1),
                new Vector3(1, previous, 1),
                new Vector3(1, previous, previous),
                vertices, triangles);
            AddQuad(
                new Vector3(-1, current, previous),
                new Vector3(-1, current, 1),
                new Vector3(-1, previous, 1),
                new Vector3(-1, previous, previous),
                vertices, triangles, true);

            AddQuad(
                new Vector3(-1, current, previous),
                new Vector3(1, current, previous),
                new Vector3(1, previous, previous),
                new Vector3(-1, previous, previous),
                vertices, triangles);

            AddQuad(
                new Vector3(-1, current, current),
                new Vector3(1, current, current),
                new Vector3(1, current, previous),
                new Vector3(-1, current, previous),
                vertices, triangles);
        }

        // Bottom
        AddQuad(
            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, -1),
            vertices, triangles, true
            );

        // Back
        AddQuad(
            new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, -1, 1),
            new Vector3(-1, -1, 1),
            vertices, triangles, true
            );

        _mesh.Clear();
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="TL">Top Left</param>
    /// <param name="TR">Top Right</param>
    /// <param name="BL">Bottom Left</param>
    /// <param name="BR">Bottom Right</param>
    /// <param name=""></param>
    private void AddQuad(Vector3 TL, Vector3 TR, Vector3 BL, Vector3 BR, List<Vector3> vertices, List<int> indices, bool reverseWinding = false)
    {
        int offset = vertices.Count;
        vertices.Add(TL);
        vertices.Add(TR);
        vertices.Add(BL);
        vertices.Add(BR);


        if (reverseWinding)
        {
            indices.Add(offset + 0);
            indices.Add(offset + 2);
            indices.Add(offset + 1);

            indices.Add(offset + 2);
            indices.Add(offset + 0);
            indices.Add(offset + 3);
        } else
        {
            indices.Add(offset + 0);
            indices.Add(offset + 1);
            indices.Add(offset + 2);

            indices.Add(offset + 2);
            indices.Add(offset + 3);
            indices.Add(offset + 0);
        }
    }

}
