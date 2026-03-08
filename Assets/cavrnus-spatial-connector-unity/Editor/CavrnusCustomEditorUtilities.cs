#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cavrnus.SpatialConnector.Editor
{
	public static class CavrnusCustomEditorUtilities
    {
        public static EditorWindow CreateCenteredWindow(this EditorWindow w, string title, Vector2 size)
        {
            w.titleContent = new GUIContent(title);
            w.position = new Rect(Screen.width / 2, Screen.height / 2, size.x, size.y);
            w.maxSize = new Vector2(size.x, size.y);
            w.minSize = w.maxSize;
            w.CenterOnMainWin();

            return w;
        }

        public static Texture2D LoadTextureFromFile(string path)
        {
            var texture = new Texture2D(2, 2);      

            if (File.Exists(path)) {
                var fileData = File.ReadAllBytes(path);
                texture.LoadImage(fileData);
            }
            else
                Debug.Log($"File doesn't exist at: {path}");

            return texture;
        }

        public static void AddSpace(float amount)
        {
            GUILayout.Space(amount);
        }
        
        public static void AddDivider()
        {
            var whiteBoxStyle = new GUIStyle {normal = {background = MakeTex(1, 1, new Color(0.4f,0.4f,0.4f))}};
            GUILayout.Box("", whiteBoxStyle, GUILayout.Height(1));
        }

        #region Buttons
        
        public static void CreateLargeButton(string label, Vector2 size, int radius, Action onClick)
        {
            var style = ButtonStyle();
            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.fixedWidth = size.x;
            style.fixedHeight = size.y;
            style.alignment = TextAnchor.MiddleCenter;
            style.border = new RectOffset(radius, radius, radius, radius);
            
            if (GUILayout.Button(label, style))
                onClick?.Invoke();
        }
        
        public static void CreateLargeButtonWithColor(string label, Vector2 size, int radius, Color? color, Action onClick)
        {
            var style = ButtonStyle();
            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.fixedWidth = size.x;
            style.fixedHeight = size.y;
            style.alignment = TextAnchor.MiddleCenter;
            style.border = new RectOffset(radius, radius, radius, radius);

            if (color.HasValue)
            {
                var bgColor = color.Value;
                style.normal.background = MakeTex(1, 1, bgColor);
                style.hover.background = MakeTex(1, 1, bgColor * 1.1f);
                style.active.background = MakeTex(1, 1, bgColor * 0.9f);
            }

            if (GUILayout.Button(label, style))
                onClick?.Invoke();
        }

        public static void CreateButton(string label, Vector2 size, Action onClick = null)
        {
            var style = ButtonStyle();
            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.fixedWidth = size.x;
            style.fixedHeight = size.y;
            style.alignment = TextAnchor.MiddleCenter;
            
            if (GUILayout.Button(label, style))
                onClick?.Invoke();
        }
        
#endregion

        public static void CreateHeader(string text, int space = 10)
        {
            GUILayout.Label(text, HeadingStyle());
            GUILayout.Space(space);
        }

        public static void CreateLabel(string label, int fontSize = 14, bool bold = false, TextAnchor alignment = TextAnchor.MiddleLeft, string tooltip = "")
        {
            var bs = LabelStyle();
            bs.fontSize = fontSize;
            bs.alignment = alignment;
            bs.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            
            GUILayout.Label(new GUIContent(label, tooltip), bs);
        }
        
        public static void CreateLabelAbsolutePos(string label, Rect rect, Color color, int fontSize = 14, bool bold = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var style = LabelStyle();
            style.fontSize = fontSize;
            style.normal.textColor = color;
            style.alignment = alignment;
            style.hover.textColor = color;
            style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            
            GUI.Label(rect, label, style);
        }
        
        public static string CreateTextFieldWithLabelNoToolTip(string container, string label, int space = 10, int width = 120, string tooltip = "")
        {
            GUILayout.Label(label, LabelStyle());
            GUILayout.Space(space / 4);
            var gui= EditorGUILayout.TextField(container, GUILayout.Height(20), GUILayout.Width(width));
            GUILayout.Space(space);
            
            return gui;
        }

        public static string CreateTextFieldWithLabel(string container, string label, int space = 10, int width = 120, string tooltip = "")
        {
            var gui= EditorGUILayout.TextField(new GUIContent(label, tooltip), container, GUILayout.Height(20), GUILayout.Width(width));
            GUILayout.Space(space);

            return gui;
        }
        
        public static string CreateTextAreaWithLabel(string container, string label, int height = 200, int space = 10, bool isBold = false)
        {
            var labelStyle = isBold ? new GUIStyle(LabelStyle()) { fontStyle = FontStyle.Bold } : LabelStyle();

            GUILayout.Label(label, labelStyle);
            GUILayout.Space(space / 4);
    
            var gui = EditorGUILayout.TextArea(container, GUILayout.Height(height));
            GUILayout.Space(space);
    
            return gui;
        }
        
        public static void CenterOnMainWin(this EditorWindow window)
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            Rect pos = window.position;
            float centerWidth = (main.width - pos.width) * 0.5f;
            float centerHeight = (main.height - pos.height) * 0.5f;
            pos.x = main.x + centerWidth;
            pos.y = main.y + centerHeight;
            window.position = pos;
        }

        public static GUIStyle LabelStyle()
        {
            return new GUIStyle(EditorStyles.label);
        }
        
        public static GUIStyle TitleStyle()
        {
            var style = new GUIStyle(EditorStyles.label) {
                wordWrap = true, 
                fontSize = 28, 
                richText = true,
                fontStyle = FontStyle.Bold
            };

            return style;
        }
        
        public static GUIStyle ButtonStyle()
        {
            var skin = new GUIStyle(GUI.skin.button) {
                fontSize = 14, 
                fontStyle = FontStyle.Bold
            };

            return skin;
        }
        
        public static GUIStyle HeadingStyle()
        {
            var style = new GUIStyle(EditorStyles.label) {
                wordWrap = true, 
                fontSize = 16, 
                richText = true,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
            };

            return style;
        }
        
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static void MarkCurrentSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
#endif