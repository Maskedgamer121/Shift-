using UnityEngine;

/// <summary>
/// In-game UI panel to configure bullet settings.
/// Attach to any GameObject in the scene.
/// </summary>
public class BulletConfigUI : MonoBehaviour
{
    private Shooter2D shooter;
    private bool showPanel = false;

    // Local copies of settings
    private float fireRate = 0.2f;
    private int bulletsPerShot = 1;
    private float spreadAngle = 20f;
    private float randomSpread = 0f;
    private float bulletSpeed = 20f;
    private float bulletRange = 10f;
    private float bulletDamage = 25f;
    private float bulletSize = 1f;
    private bool burstMode = false;
    private int burstCount = 3;
    private float burstDelay = 0.05f;
    private int maxAmmo = 6;
    private float reloadTime = 2.5f;

    private void Start()
    {
        shooter = FindObjectOfType<Shooter2D>();
    }

    private void OnGUI()
    {
        // Toggle button top left
        if (GUI.Button(new Rect(10, 10, 120, 30), showPanel ? "Close Config" : "Bullet Config"))
            showPanel = !showPanel;

        if (!showPanel || shooter == null) return;

        // Panel background
        GUI.Box(new Rect(10, 45, 280, 500), "Bullet Configuration");

        float x = 20;
        float y = 70;
        float labelWidth = 140;
        float fieldWidth = 110;
        float rowHeight = 28;

        // Fire Rate
        GUI.Label(new Rect(x, y, labelWidth, 20), "Fire Rate");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), fireRate.ToString("F2")), out fireRate);
        y += rowHeight;

        // Bullets Per Shot
        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullets Per Shot");
        int.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletsPerShot.ToString()), out bulletsPerShot);
        y += rowHeight;

        // Spread Angle
        GUI.Label(new Rect(x, y, labelWidth, 20), "Spread Angle");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), spreadAngle.ToString("F1")), out spreadAngle);
        y += rowHeight;

        // Random Spread
        GUI.Label(new Rect(x, y, labelWidth, 20), "Random Spread");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), randomSpread.ToString("F1")), out randomSpread);
        y += rowHeight;

        // Bullet Speed
        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Speed");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletSpeed.ToString("F1")), out bulletSpeed);
        y += rowHeight;

        // Bullet Range
        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Range");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletRange.ToString("F1")), out bulletRange);
        y += rowHeight;

        // Bullet Damage
        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Damage");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletDamage.ToString("F1")), out bulletDamage);
        y += rowHeight;

        // Bullet Size
        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Size");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletSize.ToString("F2")), out bulletSize);
        y += rowHeight;

        // Burst Mode
        burstMode = GUI.Toggle(new Rect(x, y, 200, 20), burstMode, " Burst Mode");
        y += rowHeight;

        if (burstMode)
        {
            GUI.Label(new Rect(x, y, labelWidth, 20), "Burst Count");
            int.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), burstCount.ToString()), out burstCount);
            y += rowHeight;

            GUI.Label(new Rect(x, y, labelWidth, 20), "Burst Delay");
            float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), burstDelay.ToString("F2")), out burstDelay);
            y += rowHeight;
        }

        // Max Ammo
        GUI.Label(new Rect(x, y, labelWidth, 20), "Max Ammo");
        int.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), maxAmmo.ToString()), out maxAmmo);
        y += rowHeight;

        // Reload Time
        GUI.Label(new Rect(x, y, labelWidth, 20), "Reload Time");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), reloadTime.ToString("F1")), out reloadTime);
        y += rowHeight;

        // Apply button
        if (GUI.Button(new Rect(x, y + 5, 250, 30), "Apply Changes"))
            ApplySettings();
    }

    private void ApplySettings()
    {
        shooter.SetConfig(
            fireRate, bulletsPerShot, spreadAngle, randomSpread,
            bulletSpeed, bulletRange, 5f, bulletDamage,
            bulletSize, burstMode, burstCount, burstDelay, maxAmmo, reloadTime
        );

        Debug.Log("Bullet config applied!");
    }
}