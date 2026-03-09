using UnityEngine;

public class Shooter2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Fire Settings")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private bool holdToShoot = true;        // true = hold mouse, false = click per shot

    [Header("Bullet Count & Spread")]
    [SerializeField] private int bulletsPerShot = 1;         // How many bullets per shot
    [SerializeField] private float spreadAngle = 20f;        // Angle between each bullet
    [SerializeField] private float randomSpread = 0f;        // Extra random spread per bullet (0 = no random)
    [SerializeField] private bool evenSpread = true;         // true = evenly spaced, false = random spray

    [Header("Bullet Stats")]
    [SerializeField] private float bulletSpeed = 20f;        // How fast bullets travel
    [SerializeField] private float bulletRange = 10f;        // How far bullets travel
    [SerializeField] private float bulletLifetime = 5f;      // Max time before bullet disappears
    [SerializeField] private float bulletDamage = 25f;       // Damage per bullet
    [SerializeField] private float bulletSize = 1f;          // Scale of bullet

    [Header("Burst Settings")]
    [SerializeField] private bool burstMode = false;         // Fire in bursts
    [SerializeField] private int burstCount = 3;             // Bullets per burst
    [SerializeField] private float burstDelay = 0.05f;       // Delay between burst bullets

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

    private System.Collections.IEnumerator FireBurst()
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
                // Random spray within total spread
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

        // Apply bullet size
        bullet.transform.localScale = Vector3.one * bulletSize;

        Bullet2D bulletScript = bullet.GetComponent<Bullet2D>();
        if (bulletScript != null)
            bulletScript.SetProperties(direction, bulletSpeed, bulletLifetime, bulletDamage, bulletRange);
        else
            Destroy(bullet);
    }

    public void SetConfig(float _fireRate, int _bulletsPerShot, float _spreadAngle, float _randomSpread,
        float _bulletSpeed, float _bulletRange, float _bulletLifetime, float _bulletDamage,
        float _bulletSize, bool _burstMode, int _burstCount, float _burstDelay, int _maxAmmo, float _reloadTime)
    {
        fireRate = _fireRate;
        bulletsPerShot = _bulletsPerShot;
        spreadAngle = _spreadAngle;
        randomSpread = _randomSpread;
        bulletSpeed = _bulletSpeed;
        bulletRange = _bulletRange;
        bulletLifetime = _bulletLifetime;
        bulletDamage = _bulletDamage;
        bulletSize = _bulletSize;
        burstMode = _burstMode;
        burstCount = _burstCount;
        burstDelay = _burstDelay;
        maxAmmo = _maxAmmo;
        reloadTime = _reloadTime;
        currentAmmo = maxAmmo;
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