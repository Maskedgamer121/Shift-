using UnityEngine;

/// <summary>
/// Shoots 6 bullets then reloads for 3 seconds.
/// Attach to your Player GameObject.
/// </summary>
public class Shooter2D : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

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

        if (mainCamera == null)
            Debug.LogError("Shooter2D: Main Camera not found!");
        if (firePoint == null)
            Debug.LogError("Shooter2D: FirePoint is not assigned!");
        if (bulletPrefab == null)
            Debug.LogError("Shooter2D: Bullet Prefab is not assigned!");
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
            Debug.Log("Reloading... " + reloadTimer.ToString("F1") + "s");

            if (reloadTimer <= 0f)
            {
                isReloading = false;
                currentAmmo = maxAmmo;
                Debug.Log("Reloaded!");
            }
        }

        // Manual reload with R key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartReload();
        }
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

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet2D bulletScript = bullet.GetComponent<Bullet2D>();

        if (bulletScript == null)
        {
            Debug.LogError("Shooter2D: Bullet2D script not found on prefab!");
            Destroy(bullet);
            return;
        }

        bulletScript.SetDirection(firePoint.right);
        currentAmmo--;

        Debug.Log("Ammo: " + currentAmmo + "/" + maxAmmo);

        if (currentAmmo <= 0)
            StartReload();
    }

    // Display ammo and reload status on screen
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