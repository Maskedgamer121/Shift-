#if UNITY_EDITOR
using Cavrnus.SpatialConnector.Properties.Binders;
using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Editor.BindComponents
{
    [CustomEditor(typeof(CavrnusPropertyBinderComponent), true)]
    public class CavrnusPropertyBinderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (EditorGUILayout.LinkButton("[Documentation]"))
                Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/1361608713/Property+Binders");
            
            base.OnInspectorGUI();
        }
    }
}
#endif