using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player's gun that shoots given projectiles that cause loud sound on hit.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Gun : MonoBehaviour
{
    [Tooltip("Projectile to appear when gun shoots.")]
    public Projectile Projectile;
    public bool Aiming => _aiming;

    private LineRenderer _lineRenderer;
    private Transform _muzzle;
    private GameObject _visual;
    private bool _aiming = false;
    private Vector3 _baseDirection;
    private float _reloadTime = 1.5f;
    private float _reloadTimer;

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
        if (_reloadTimer >= 0f)
            _reloadTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Sets the gun visible and activates the laser sight (line renderer).
    /// </summary>
    /// <param name="direction"></param>
    public void Aim(Vector3 direction)
    {
        _aiming = true;
        _visual.SetActive(true);
        _lineRenderer.enabled = true;
        transform.forward = direction;
    }

    /// <summary>
    /// Hides the gun and deactivates the laser sight (line renderer).
    /// </summary>
    public void StopAim()
    {
        _aiming = false;
        _visual.SetActive(false);
        _lineRenderer.enabled = false;
        transform.forward = _baseDirection;
    }

    /// <summary>
    /// Ray cast to where the line renderer should end.
    /// </summary>
    private void UpdateAiming()
    {
        if (!_aiming) return;

        _lineRenderer.enabled = _reloadTimer <= 0f;
        float maxDistance = 10000;
        bool hit = Physics.Raycast(_muzzle.position, transform.forward, out RaycastHit hitInfo, maxDistance);
        
        Vector3 endPoint = hit ? hitInfo.point : transform.forward * maxDistance;

        _lineRenderer.SetPositions(
            new Vector3[] { _muzzle.position, endPoint }
        );
    }

    /// <summary>
    /// Releases the projectile. Initiates reload timer. Projectile causes loud noise on hit.
    /// </summary>
    public void Shoot()
    {
        if (_reloadTimer > 0f) return;
        _reloadTimer = _reloadTime;

        Projectile projectile = Instantiate(Projectile);
        projectile.Shoot(_muzzle.position, transform.forward, (Vector3 pos, GameObject target) =>
        {
            GameController.AudioManager.AudibleEffect(projectile.gameObject, pos, 10f);
            GameController.AudioManager.Play("GlassShatter", position: pos, volume: 0.5f);
        });
    }
}
