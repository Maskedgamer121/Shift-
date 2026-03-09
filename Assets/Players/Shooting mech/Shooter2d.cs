using UnityEngine;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Fire Settings")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private bool holdToShoot = true;

    [Header("Bullet Count & Spread")]
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private float spreadAngle = 20f;
    [SerializeField] private float randomSpread = 0f;
    [SerializeField] private bool evenSpread = true;

    [Header("Bullet Stats")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletRange = 10f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private float bulletDamage = 25f;
    [SerializeField] private float bulletSize = 1f;

    [Header("Burst Settings")]
    [SerializeField] private bool burstMode = false;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.05f;

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
    }

    private void Update()
    {
        if (firePoint == null || mainCamera == null) return;

        OrbitFirePoint();
        HandleReload();

        bool fireInput = holdToShoot ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (!isReloading && fireInput && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (burstMode)
                StartCoroutine(FireBurst());
            else
                Shoot();
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (currentAmmo <= 0) break;
            Shoot();
            yield return new WaitForSeconds(burstDelay);
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
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            StartReload();
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
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

        if (bulletsPerShot == 1)
        {
            float rnd = Random.Range(-randomSpread, randomSpread);
            Vector2 dir = Quaternion.Euler(0, 0, rnd) * firePoint.right;
            SpawnBullet(firePoint.position, dir);
        }
        else
        {
            if (evenSpread)
            {
                float totalSpread = spreadAngle * (bulletsPerShot - 1);
                float startAngle = -totalSpread / 2f;

                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float angleOffset = startAngle + (spreadAngle * i);
                    angleOffset += Random.Range(-randomSpread, randomSpread);
                    Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * firePoint.right;
                    SpawnBullet(firePoint.position, dir);
                }
            }
            else
            {
                for (int i = 0; i < bulletsPerShot; i++)
                {
                    float angleOffset = Random.Range(-spreadAngle, spreadAngle);
                    Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * firePoint.right;
                    SpawnBullet(firePoint.position, dir);
                }
            }
        }

        currentAmmo--;
        if (currentAmmo <= 0)
            StartReload();
    }

    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(0, 0, angle));

        bullet.transform.localScale = Vector3.one * bulletSize;

        Bullet2D bulletScript = bullet.GetComponent<Bullet2D>();
        if (bulletScript != null)
            bulletScript.SetProperties(direction, bulletSpeed, bulletLifetime, bulletDamage, bulletRange);
        else
            Destroy(bullet);
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