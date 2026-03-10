using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SHIFT";

    [Header("Game Info")]
    [SerializeField] private string gameTitle = "SHIFT";
    [SerializeField] private string subtitle = "Survive the night";

    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle hintStyle;
    private bool stylesReady = false;

    private void InitStyles()
    {
        if (stylesReady) return;
        stylesReady = true;

        titleStyle = new GUIStyle();
        titleStyle.fontSize = 80;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);

        subtitleStyle = new GUIStyle();
        subtitleStyle.fontSize = 18;
        subtitleStyle.fontStyle = FontStyle.Italic;
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.12f, 0.1f, 0.08f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(0.22f, 0.18f, 0.12f));

        hintStyle = new GUIStyle();
        hintStyle.fontSize = 12;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(1, 1, 1, 0.3f);
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
        InitStyles();

        float w = Screen.width;
        float h = Screen.height;

        // Background
        GUI.color = new Color(0.05f, 0.05f, 0.07f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUI.Label(new Rect(0, h * 0.25f, w, 100), gameTitle, titleStyle);

        // Divider
        GUI.color = new Color(0.8f, 0.6f, 0.2f, 0.6f);
        GUI.DrawTexture(new Rect((w - 200) / 2f, h * 0.25f + 108f, 200, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Subtitle
        GUI.Label(new Rect(0, h * 0.25f + 118f, w, 40), subtitle.ToUpper(), subtitleStyle);

        // Play button
        float btnW = 220f;
        float btnH = 55f;
        float btnX = (w - btnW) / 2f;
        float playY = h * 0.58f;

        if (GUI.Button(new Rect(btnX, playY, btnW, btnH), "PLAY", buttonStyle))
            SceneManager.LoadScene(gameSceneName);

        // Quit button
        if (GUI.Button(new Rect(btnX, playY + btnH + 15f, btnW, btnH), "QUIT", buttonStyle))
            Application.Quit();

        // Hint
        GUI.Label(new Rect(0, h - 35f, w, 30), "WASD to move  |  Mouse to aim  |  LMB to shoot  |  R to reload", hintStyle);
    }
}