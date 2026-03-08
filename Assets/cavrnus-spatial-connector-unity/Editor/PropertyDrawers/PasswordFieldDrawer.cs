#if UNITY_EDITOR

using Cavrnus.SpatialConnector.PropertyDrawers;
using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Editor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(PasswordFieldAttribute))]
    public class PasswordFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var password = EditorGUI.PasswordField(position, GUIContent.none, property.stringValue);
            property.stringValue = password;
            
            EditorGUI.EndProperty();
        }
    }
}
#endif