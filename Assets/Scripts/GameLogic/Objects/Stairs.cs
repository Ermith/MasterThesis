using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Dynamic stairs mesh and collider. Responds to changes in editor.
/// </summary>
[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer), typeof(MeshCollider)), ExecuteInEditMode]
public class Stairs : MonoBehaviour
{
    private Mesh _mesh;

    [Range(1, 100)]
    public int Steps;
    public Vector3 Size;

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

    /// <summary>
    /// Builds the mesh based on properties.
    /// </summary>
    private void Rebuild()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();

        float width = Size.x;
        float height = Size.y;
        float length = Size.z;

        for (int step = 1; step <= Steps; step++)
        {
            float top = (float)step / Steps * height;
            float bot = (float)(step - 1) / Steps * height;

            float fwd = (float)step / Steps * length;
            float back = (float)(step - 1) / Steps * length;

            // left
            AddQuad(
                new Vector3(0, top, back),
                new Vector3(0, top, length),
                new Vector3(0, bot, length),
                new Vector3(0, bot, back),
                vertices, triangles, true);

            // right
            AddQuad(
                new Vector3(width, top, back),
                new Vector3(width, top, length),
                new Vector3(width, bot, length),
                new Vector3(width, bot, back),
                vertices, triangles);

            // front
            AddQuad(
                new Vector3(0, top, back),
                new Vector3(width, top, back),
                new Vector3(width, bot, back),
                new Vector3(0, bot, back),
                vertices, triangles);

            // top
            AddQuad(
                new Vector3(0, top, fwd),
                new Vector3(width, top, fwd),
                new Vector3(width, top, back),
                new Vector3(0, top, back),
                vertices, triangles);
        }

        // Bottom
        AddQuad(
            new Vector3(0, 0, length),
            new Vector3(width, 0, length),
            new Vector3(width, 0, 0),
            new Vector3(0, 0, 0),
            vertices, triangles, true
            );

        // Back
        AddQuad(
            new Vector3(0, height, length),
            new Vector3(width, height, length),
            new Vector3(width, 0, length),
            new Vector3(0, 0, length),
            vertices, triangles, true
            );

        _mesh.Clear();
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

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
