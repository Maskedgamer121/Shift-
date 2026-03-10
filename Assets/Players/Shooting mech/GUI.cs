using UnityEngine;

public class BulletConfigUI : MonoBehaviour
{
    public static bool IsPanelOpen = false;

    private Shooter2D shooter;
    private bool showPanel = false;
    private bool isPaused = false;

    // String buffers for text fields
    private string sFireRate = "0.20";
    private string sBulletsPerShot = "1";
    private string sSpreadAngle = "20.0";
    private string sRandomSpread = "0.0";
    private string sBulletSpeed = "20.0";
    private string sBulletRange = "10.0";
    private string sBulletDamage = "25.0";
    private string sBulletSize = "1.00";
    private string sBurstCount = "3";
    private string sBurstDelay = "0.05";
    private string sMaxAmmo = "6";
    private string sReloadTime = "2.5";
    private bool burstMode = false;

    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle fieldStyle;
    private GUIStyle titleStyle;
    private GUIStyle pauseTitleStyle;
    private bool stylesReady = false;

    private void Start()
    {
        shooter = FindObjectOfType<Shooter2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showPanel)
            {
                showPanel = false;
                IsPanelOpen = isPaused;
            }
            else
            {
                isPaused = !isPaused;
                Time.timeScale = isPaused ? 0f : 1f;
                IsPanelOpen = isPaused;
            }
        }
    }

    private void InitStyles()
    {
        if (stylesReady) return;
        stylesReady = true;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.07f, 0.97f));

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        labelStyle.fontSize = 13;

        fieldStyle = new GUIStyle(GUI.skin.textField);
        fieldStyle.fontSize = 13;
        fieldStyle.normal.textColor = Color.white;
        fieldStyle.focused.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.12f, 0.1f, 0.08f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.25f, 0.2f, 0.15f));

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
        titleStyle.alignment = TextAnchor.MiddleCenter;

        pauseTitleStyle = new GUIStyle(GUI.skin.label);
        pauseTitleStyle.fontSize = 32;
        pauseTitleStyle.fontStyle = FontStyle.Bold;
        pauseTitleStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        pauseTitleStyle.alignment = TextAnchor.MiddleCenter;
    }

    private Texture2D MakeTex(int w, int h, Color col)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D t = new Texture2D(w, h);
        t.SetPixels(pix);
        t.Apply();
        return t;
    }

    private void OnGUI()
    {
        if (EndScreen.IsGameOver) return;

        InitStyles();

        if (isPaused && !showPanel)
        {
            DrawPauseMenu();
            return;
        }

        if (!isPaused)
        {
            if (GUI.Button(new Rect(10, 10, 130, 30), showPanel ? "X Close Config" : "Bullet Config", buttonStyle))
            {
                showPanel = !showPanel;
                IsPanelOpen = showPanel;
            }
        }

        if (!showPanel || shooter == null) return;

        DrawConfigPanel();
    }

    private void DrawPauseMenu()
    {
        float w = Screen.width;
        float h = Screen.height;

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelW = 300f;
        float panelH = 280f;
        float px = (w - panelW) / 2f;
        float py = (h - panelH) / 2f;

        GUI.Box(new Rect(px, py, panelW, panelH), "", panelStyle);
        GUI.Label(new Rect(px, py + 20f, panelW, 50), "PAUSED", pauseTitleStyle);

        GUI.color = new Color(0.8f, 0.6f, 0.2f, 0.6f);
        GUI.DrawTexture(new Rect(px + 50, py + 75f, panelW - 100, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float btnW = 200f;
        float btnH = 45f;
        float bx = px + (panelW - btnW) / 2f;

        if (GUI.Button(new Rect(bx, py + 95f, btnW, btnH), "RESUME", buttonStyle))
        {
            isPaused = false;
            Time.timeScale = 1f;
            IsPanelOpen = false;
        }

        if (GUI.Button(new Rect(bx, py + 150f, btnW, btnH), "BULLET CONFIG", buttonStyle))
        {
            showPanel = true;
            IsPanelOpen = true;
        }

        if (GUI.Button(new Rect(bx, py + 205f, btnW, btnH), "MAIN MENU", buttonStyle))
        {
            isPaused = false;
            Time.timeScale = 1f;
            IsPanelOpen = false;
            EndScreen.IsGameOver = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartScreen");
        }
    }

    private void DrawConfigPanel()
    {
        float panelW = 300f;
        float panelH = burstMode ? 560f : 500f;
        Rect panelRect = new Rect(10, 45, panelW, panelH);

        GUI.Box(panelRect, "", panelStyle);
        GUI.Label(new Rect(10, 50, panelW, 25), "BULLET CONFIGURATION", titleStyle);

        GUI.color = new Color(0.8f, 0.6f, 0.2f, 0.5f);
        GUI.DrawTexture(new Rect(20, 78, panelW - 20, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;


        float x = 20;
        float y = 88;
        float lw = 150;
        float fw = 120;
        float row = 30;

        DrawField(ref y, x, lw, fw, "Fire Rate",        ref sFireRate,       row);
        DrawField(ref y, x, lw, fw, "Bullets Per Shot", ref sBulletsPerShot,  row);
        DrawField(ref y, x, lw, fw, "Spread Angle",     ref sSpreadAngle,    row);
        DrawField(ref y, x, lw, fw, "Random Spread",    ref sRandomSpread,   row);
        DrawField(ref y, x, lw, fw, "Bullet Speed",     ref sBulletSpeed,    row);
        DrawField(ref y, x, lw, fw, "Bullet Range",     ref sBulletRange,    row);
        DrawField(ref y, x, lw, fw, "Bullet Damage",    ref sBulletDamage,   row);
        DrawField(ref y, x, lw, fw, "Bullet Size",      ref sBulletSize,     row);

        burstMode = GUI.Toggle(new Rect(x, y, 200, 22), burstMode, " Burst Mode", labelStyle);
        y += row;

        if (burstMode)
        {
            DrawField(ref y, x, lw, fw, "Burst Count", ref sBurstCount, row);
            DrawField(ref y, x, lw, fw, "Burst Delay", ref sBurstDelay, row);
        }

        DrawField(ref y, x, lw, fw, "Max Ammo",    ref sMaxAmmo,   row);
        DrawField(ref y, x, lw, fw, "Reload Time", ref sReloadTime, row);

        if (GUI.Button(new Rect(x, y + 5, panelW - 30, 35), "APPLY CHANGES", buttonStyle))
            ApplySettings();
    }

    private void DrawField(ref float y, float x, float lw, float fw, string label, ref string val, float row)
    {
        GUI.Label(new Rect(x, y, lw, 22), label, labelStyle);
        val = GUI.TextField(new Rect(x + lw, y, fw, 22), val, fieldStyle);
        y += row;
    }

    private void ApplySettings()
    {
        float.TryParse(sFireRate,      out float fireRate);
        int.TryParse(sBulletsPerShot,  out int bulletsPerShot);
        float.TryParse(sSpreadAngle,   out float spreadAngle);
        float.TryParse(sRandomSpread,  out float randomSpread);
        float.TryParse(sBulletSpeed,   out float bulletSpeed);
        float.TryParse(sBulletRange,   out float bulletRange);
        float.TryParse(sBulletDamage,  out float bulletDamage);
        float.TryParse(sBulletSize,    out float bulletSize);
        int.TryParse(sBurstCount,      out int burstCount);
        float.TryParse(sBurstDelay,    out float burstDelay);
        int.TryParse(sMaxAmmo,         out int maxAmmo);
        float.TryParse(sReloadTime,    out float reloadTime);

        shooter.SetConfig(
            fireRate, bulletsPerShot, spreadAngle, randomSpread,
            bulletSpeed, bulletRange, 5f, bulletDamage,
            bulletSize, burstMode, burstCount, burstDelay, maxAmmo, reloadTime
        );

        showPanel = false;
        IsPanelOpen = isPaused;
        Debug.Log("Bullet config applied!");
    }
}