#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Editor
{
    [CustomEditor(typeof(CavrnusPropertyBinder<>), true)]
    public class CavrnusPropertyObjectEditor : UnityEditor.Editor
    {
        private SerializedProperty PropertyName;
        private SerializedProperty ContainerType;
        private SerializedProperty ContainerName;
        private SerializedProperty IsUserMetadata;
        private SerializedProperty DefaultValue;

        private void OnEnable()
        {
			PropertyName = serializedObject.FindProperty(nameof(CavrnusPropertyBinder<object>.PropertyName));
			ContainerType = serializedObject.FindProperty(nameof(CavrnusPropertyBinder<object>.ContainerType));
			ContainerName = serializedObject.FindProperty(nameof(CavrnusPropertyBinder<object>.ContainerName));
			IsUserMetadata = serializedObject.FindProperty(nameof(CavrnusPropertyBinder<object>.IsUserMetadata));
			DefaultValue = serializedObject.FindProperty(nameof(CavrnusPropertyBinder<object>.DefaultValue));
		}
		public override void OnInspectorGUI()
        {
			EditorGUILayout.PropertyField(PropertyName, new GUIContent("Property Name"));

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(ContainerType, new GUIContent("Container Type"));

			switch (ContainerType.enumValueFlag)
			{
				case 0:
					EditorGUILayout.PropertyField(ContainerName, new GUIContent("Container Name"));
					break;
				case 1:
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextField("Container Name", "Specified on object components");
					EditorGUI.EndDisabledGroup();
					break;
				case 2:
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextField("Container Name", "Local User Container");
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal();
					var helpMsg = "This makes the data available across all spaces, including outside them. Ideal for usernames, avatars, etc.";
					EditorGUILayout.PropertyField(IsUserMetadata, new GUIContent("Save To User Account"));
					EditorGUILayout.HelpBox(helpMsg, MessageType.None, true);
					EditorGUILayout.EndHorizontal();
					break;
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(DefaultValue, new GUIContent("Default Value"));

			serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif