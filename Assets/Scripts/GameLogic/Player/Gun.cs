using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Gun : MonoBehaviour
{
    public Projectile Projectile;
    public bool Aiming => _aiming;

    private LineRenderer _lineRenderer;
    private Transform _muzzle;
    private GameObject _visual;
    private bool _aiming = false;
    private Vector3 _baseDirection;

    // Start is called before the first frame update
    void Start()
    {
        _baseDirection = transform.forward;
        _muzzle = transform.Find("Muzzle");
        _visual = transform.Find("Visual").gameObject;
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateAiming();
    }

    public void Aim(Vector3 direction)
    {
        _aiming = true;
        _visual.SetActive(true);
        _lineRenderer.enabled = true;
        transform.forward = direction;
    }

    public void StopAim()
    {
        _aiming = false;
        _visual.SetActive(false);
        _lineRenderer.enabled = false;
        transform.forward = _baseDirection;
    }

    private void UpdateAiming()
    {
        if (!_aiming) return;

        float maxDistance = 10000;
        bool hit = Physics.Raycast(_muzzle.position, transform.forward, out RaycastHit hitInfo, maxDistance);
        
        Vector3 endPoint = hit ? hitInfo.point : transform.forward * maxDistance;

        _lineRenderer.SetPositions(
            new Vector3[] { _muzzle.position, endPoint }
        );
    }

    public void Shoot()
    {
        Projectile projectile = Instantiate(Projectile);
        projectile.Shoot(_muzzle.position, transform.forward, (Vector3 pos, GameObject target) =>
        {
            GameController.AudioManager.AudibleEffect(projectile.gameObject, pos, 10f);
            GameController.AudioManager.Play("GlassShatter", position: pos, volume: 0.5f);
        });
    }
}
