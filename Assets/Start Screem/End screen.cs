using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows a Game Over screen when the player dies.
/// Attach to any GameObject in the SHIFT scene.
/// </summary>
public class EndScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuScene = "StartScreen";
    [SerializeField] private string gameScene = "SHIFT";

    public static bool IsGameOver = false;

    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle statsStyle;
    private bool stylesReady = false;

    private float survivalTime = 0f;
    private int enemiesKilled = 0;
    private bool wasTracking = false;

    private void Start()
    {
        IsGameOver = false;
    }

    private void Update()
    {
        if (!IsGameOver)
        {
            survivalTime += Time.deltaTime;
            enemiesKilled = TotalKills.count;
        }
    }

    private void InitStyles()
    {
        if (stylesReady) return;
        stylesReady = true;

        titleStyle = new GUIStyle();
        titleStyle.fontSize = 80;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.85f, 0.2f, 0.2f);

        subtitleStyle = new GUIStyle();
        subtitleStyle.fontSize = 20;
        subtitleStyle.fontStyle = FontStyle.Italic;
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);

        statsStyle = new GUIStyle();
        statsStyle.fontSize = 18;
        statsStyle.alignment = TextAnchor.MiddleCenter;
        statsStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.12f, 0.1f, 0.08f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.22f, 0.18f, 0.12f));
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        if (!IsGameOver) return;

        InitStyles();

        float w = Screen.width;
        float h = Screen.height;

        // Dark overlay
        GUI.color = new Color(0.03f, 0.03f, 0.05f, 0.95f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUI.Label(new Rect(0, h * 0.2f, w, 100), "YOU DIED", titleStyle);

        // Divider
        GUI.color = new Color(0.85f, 0.2f, 0.2f, 0.6f);
        GUI.DrawTexture(new Rect((w - 200) / 2f, h * 0.2f + 108f, 200, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Subtitle
        GUI.Label(new Rect(0, h * 0.2f + 118f, w, 40), "THE NIGHT CONSUMED YOU", subtitleStyle);

        // Stats
        string timeStr = string.Format("{0:0}:{1:00}", Mathf.Floor(survivalTime / 60), survivalTime % 60);
        GUI.Label(new Rect(0, h * 0.48f, w, 35), "Time Survived:  " + timeStr, statsStyle);
        GUI.Label(new Rect(0, h * 0.48f + 40f, w, 35), "Enemies Killed:  " + enemiesKilled, statsStyle);

        // Buttons
        float btnW = 220f;
        float btnH = 55f;
        float btnX = (w - btnW) / 2f;
        float btnY = h * 0.65f;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "PLAY AGAIN", buttonStyle))
        {
            IsGameOver = false;
            TotalKills.count = 0;
            SceneManager.LoadScene(gameScene);
        }

        if (GUI.Button(new Rect(btnX, btnY + btnH + 15f, btnW, btnH), "MAIN MENU", buttonStyle))
        {
            IsGameOver = false;
            TotalKills.count = 0;
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}