using UnityEngine;

/// <summary>
/// In-game bullet config UI. Blocks shooting when panel is open.
/// Attach to any GameObject in the scene.
/// </summary>
public class BulletConfigUI : MonoBehaviour
{
    public static bool IsPanelOpen = false; // Shooter checks this to block firing

    private Shooter2D shooter;
    private bool showPanel = false;

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
        if (GUI.Button(new Rect(10, 10, 120, 30), showPanel ? "Close Config" : "Bullet Config"))
        {
            showPanel = !showPanel;
            IsPanelOpen = showPanel;
        }

        if (!showPanel || shooter == null) return;

        // Eat all mouse events inside the panel so clicks dont shoot
        Rect panelRect = new Rect(10, 45, 280, 520);
        GUI.Box(panelRect, "Bullet Configuration");

        if (Event.current.type == EventType.MouseDown && panelRect.Contains(Event.current.mousePosition))
            Event.current.Use();

        float x = 20;
        float y = 70;
        float labelWidth = 140;
        float fieldWidth = 110;
        float rowHeight = 28;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Fire Rate");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), fireRate.ToString("F2")), out fireRate);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullets Per Shot");
        int.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletsPerShot.ToString()), out bulletsPerShot);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Spread Angle");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), spreadAngle.ToString("F1")), out spreadAngle);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Random Spread");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), randomSpread.ToString("F1")), out randomSpread);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Speed");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletSpeed.ToString("F1")), out bulletSpeed);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Range");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletRange.ToString("F1")), out bulletRange);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Damage");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletDamage.ToString("F1")), out bulletDamage);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Bullet Size");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), bulletSize.ToString("F2")), out bulletSize);
        y += rowHeight;

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

        GUI.Label(new Rect(x, y, labelWidth, 20), "Max Ammo");
        int.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), maxAmmo.ToString()), out maxAmmo);
        y += rowHeight;

        GUI.Label(new Rect(x, y, labelWidth, 20), "Reload Time");
        float.TryParse(GUI.TextField(new Rect(x + labelWidth, y, fieldWidth, 20), reloadTime.ToString("F1")), out reloadTime);
        y += rowHeight;

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