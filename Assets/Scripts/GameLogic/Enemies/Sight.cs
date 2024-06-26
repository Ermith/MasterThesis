using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Gives ability to check if something is within a view cone. Also renders the view cone.
/// </summary>
public class Sight : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private Mesh _emptyMesh;
    private Color _baseColor;
    private Color _highlightColor = Color.red;

    [Tooltip("'Resolution' of the view cone mesh.")]
    public int Segments = 20;
    [Tooltip("Angle of the view cone in radians.")]
    public float Angle = 90 * Mathf.Deg2Rad;
    [Tooltip("Range of the view cone.")]
    public float Range = 20;
    public Material VisionConeMaterial;

    [HideInInspector] public bool VisionConeVisible = true;
    [HideInInspector] public bool VisionConeHilighted = false;

    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = this.AddComponent<MeshRenderer>();
        _meshRenderer.material = VisionConeMaterial;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;

        _baseColor = _meshRenderer.material.color;
        _meshFilter = this.AddComponent<MeshFilter>();
        _mesh = new Mesh();
        _emptyMesh = new Mesh();
        _meshFilter.mesh = _mesh;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _meshFilter.mesh = VisionConeVisible ? _mesh : _emptyMesh;
        if (VisionConeVisible)
            RenderVisionCone();
    }

    /// <summary>
    /// Calculates the vision cone _mesh based on objects it intersects.
    /// </summary>
    private void RenderVisionCone()
    {
        var triangles = new int[(Segments - 1) * 3];
        var vertices = new Vector3[Segments + 1];
        vertices[Segments] = Vector3.zero;

        for (int segment = 0; segment < Segments; segment++)
        {
            float angle = Angle * ((float)segment / (Segments - 1) - 0.5f);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 segmentDir = transform.forward * cos + transform.right * sin;
            Vector3 vertexDir = (Vector3.forward * cos + Vector3.right * sin);

            bool hit = Physics.Raycast(transform.position, segmentDir, out RaycastHit hitInfo, Range, ~(0b1100));
            vertices[segment] = hit
                ? vertexDir * hitInfo.distance
                : vertexDir * Range;

            Vector3 scale = transform.lossyScale;
            Vector3 invertedScale = new(
                1 / scale.x,
                1 / scale.y,
                1 / scale.z);

            vertices[segment].Scale(invertedScale);

            if (segment == Segments - 1) continue;

            int t = segment * 3;
            triangles[t + 0] = Segments;
            triangles[t + 1] = segment + 1;
            triangles[t + 2] = segment + 2;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _meshRenderer.material.color = VisionConeHilighted ? _highlightColor : _baseColor;
    }

    /// <summary>
    /// Simple raycast and range check.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool CanSee(Transform target)
    {
        Vector3 relative = target.position - transform.position;
        float angle = Vector3.Angle(relative, transform.forward) * Mathf.Deg2Rad;

        bool hit = Physics.Raycast(transform.position, relative.normalized, out RaycastHit hitInfo, relative.magnitude, ~(0b1100));
        if (hit && hitInfo.collider.gameObject != target.gameObject) return false;

        return relative.magnitude <= Range && Mathf.Abs(angle) <= Angle / 2;
    }

    public void OnDisable()
    {
        VisionConeVisible = false;
        _meshRenderer.enabled = false;
    }
}
