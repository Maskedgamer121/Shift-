using UnityEngine;

/// <summary>
/// Rotates FirePoint toward the mouse and shoots a bullet on left click.
/// Attach to your Player GameObject.
/// </summary>
public class Shooter2D : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    private float nextFireTime = 0f;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        AimAtMouse();

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void AimAtMouse()
    {
        // Get mouse position in world space
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Get direction from firepoint to mouse
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        // Rotate firepoint to face mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Shoot()
    {
        // Spawn bullet at firepoint, facing the same direction
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Bullet2D>().SetDirection(firePoint.right);
    }
}