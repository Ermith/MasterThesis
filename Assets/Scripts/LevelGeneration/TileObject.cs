using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Impassable,
    Floor,
    LitFloor,
    Refuge,
    HalfRefuge,
    None
}

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(BoxCollider))]
public class TileObject : MonoBehaviour
{
    [SerializeField] private GameObject _hidingSpace;
    [SerializeField] private Material _impassableMaterial;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _refugeMaterial;
    [SerializeField] private Material _prospectMaterial;
    [SerializeField] private TileType _requestedType;

    private MeshRenderer _meshRenderer;
    private TileType _type;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = _defaultMaterial;
        _type = TileType.None;
    }

    public void SetTile(TileType type)
    {
        switch (type)
        {
            case TileType.Impassable:
                _meshRenderer.material = _impassableMaterial;
                Vector3 scale = transform.localScale;
                scale.y = 4;
                transform.localScale = scale;
                _type = TileType.Impassable;
                break;
            case TileType.Floor:
                _meshRenderer.material = _defaultMaterial;
                _type = TileType.Floor;
                break;
            case TileType.LitFloor:
                _meshRenderer.material = _prospectMaterial;
                _type = TileType.LitFloor;
                break;
            case TileType.Refuge:
                _meshRenderer.material = _refugeMaterial;
                //var space = Instantiate(_hidingSpace, transform.position, transform.rotation);
                Vector3 sc = transform.lossyScale;
                sc.y = 1;
                //space.transform.localScale = sc;
                _type = TileType.Refuge;
                break;
            case TileType.HalfRefuge:
                _meshRenderer.material = _refugeMaterial;
                _type = TileType.HalfRefuge;
                break;
            default:
                break;
        }
    }

    public void SetSize(float width, float height)
    {
        Vector3 scale = transform.localScale;
        scale.x *= width;
        scale.z *= height;
        transform.localScale = scale;
    }

    private void Update()
    {
        if (_type != _requestedType)
        {
            SetTile(_requestedType);
        }
    }
}
