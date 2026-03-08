#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Editor
{
    public class SelectFolderLocationEditor : EditorWindow
    {
        public string FileName { get; private set; }
        public string FolderPath { get; private set; }
        public bool SaveLocationBool { get; private set; }

        private string headerTitle = "Replace Title!";
        private Vector2 fieldMinMaxLength;
        private bool showSaveValue;
        
        private string saveKeyValue = "";

        private bool showNameField = false;
        private string folderPathLabel = "";

        public void Init(string saveKeyValue)
        {
            this.saveKeyValue = saveKeyValue;

            SaveLocationBool = EditorPrefs.GetBool(saveKeyValue + "bool");
            FolderPath = SaveLocationBool ? EditorPrefs.GetString(saveKeyValue) : "Assets/";
        }
        
        public SelectFolderLocationEditor FieldLength(Vector2 fieldMinMaxLength)
        {
            this.fieldMinMaxLength = fieldMinMaxLength;
            return this;
        }
        
        public SelectFolderLocationEditor HeaderTitle(string headerTitle)
        {
            this.headerTitle = headerTitle;
            return this;
        }
        
        public SelectFolderLocationEditor ShowNameField(bool showNameField)
        {
            this.showNameField = showNameField;
            return this;
        }
        
        public SelectFolderLocationEditor ShowSaveValue(bool showSaveValue)
        {
            this.showSaveValue = showSaveValue;
            return this;
        }
        
        public SelectFolderLocationEditor SetFolderPathLabel(string folderPathLabel)
        {
            this.folderPathLabel = folderPathLabel;
            return this;
        }
        
        public void UpdateGUI()
        {
            EditorGUILayout.LabelField(headerTitle, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical();
            if (showNameField)
                FileName = EditorGUILayout.TextField("Name", FileName, GUILayout.MinWidth(fieldMinMaxLength.x), GUILayout.MaxWidth(fieldMinMaxLength.y));
            
            EditorGUILayout.BeginHorizontal();
            FolderPath = EditorGUILayout.TextField(folderPathLabel, FolderPath, GUILayout.MinWidth(fieldMinMaxLength.x), GUILayout.MaxWidth(fieldMinMaxLength.y));
            if (showSaveValue)
            {
                GUILayout.Space(15); // Optional spacing
                SaveLocationBool = GUILayout.Toggle(SaveLocationBool, "Save Value");
                EditorPrefs.SetBool(saveKeyValue + "bool", SaveLocationBool);
                EditorPrefs.SetString(saveKeyValue, FolderPath);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
#endif