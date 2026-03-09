using UnityEngine;

/// <summary>
/// Shoots bullets toward the mouse with spread and multiple bullet support.
/// Attach to your Player GameObject.
/// </summary>
public class Shooter2D : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    [Header("Bullet Properties")]
    [SerializeField] private int bulletsPerShot = 1;        // How many bullets per click
    [SerializeField] private float spreadAngle = 15f;       // Spread in degrees between bullets
    [SerializeField] private float bulletSpeed = 20f;       // How fast bullets travel
    [SerializeField] private float bulletLifetime = 5f;     // How long before bullet disappears
    [SerializeField] private float bulletDamage = 25f;      // Damage per bullet

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private float reloadTime = 2.5f;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 1f;

    private float nextFireTime = 0f;
    private Camera mainCamera;
    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    private void Start()
    {
        mainCamera = Camera.main;
        currentAmmo = maxAmmo;

        if (mainCamera == null) Debug.LogError("Shooter2D: Main Camera not found!");
        if (firePoint == null) Debug.LogError("Shooter2D: FirePoint is not assigned!");
        if (bulletPrefab == null) Debug.LogError("Shooter2D: Bullet Prefab is not assigned!");
    }

    private void Update()
    {
        if (firePoint == null || mainCamera == null) return;

        OrbitFirePoint();
        HandleReload();

        if (!isReloading && Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void HandleReload()
    {
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                currentAmmo = maxAmmo;
                Debug.Log("Reloaded!");
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            StartReload();
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
        Debug.Log("Reloading...");
    }

    private void OrbitFirePoint()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        firePoint.position = transform.position + (Vector3)(direction * orbitRadius);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Calculate spread for multiple bullets
        float totalSpread = spreadAngle * (bulletsPerShot - 1);
        float startAngle = -totalSpread / 2f;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float angleOffset = startAngle + (spreadAngle * i);
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);
            Vector2 direction = rotation * Vector2.right;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
            Bullet2D bulletScript = bullet.GetComponent<Bullet2D>();

            if (bulletScript == null)
            {
                Destroy(bullet);
                return;
            }

            // Pass properties to bullet
            bulletScript.SetProperties(direction, bulletSpeed, bulletLifetime, bulletDamage);
        }

        currentAmmo--;
        if (currentAmmo <= 0)
            StartReload();
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        if (isReloading)
            GUI.Label(new Rect(20, 20, 300, 50), "Reloading... " + reloadTimer.ToString("F1") + "s", style);
        else
            GUI.Label(new Rect(20, 20, 300, 50), "Ammo: " + currentAmmo + " / " + maxAmmo, style);
    }
}