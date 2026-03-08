#if UNITY_EDITOR
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Editor.BindComponents
{
    [CustomEditor(typeof(CavrnusPropertySyncMaterialColor))]
    public class SyncMaterialColorEditor : UnityEditor.Editor
    {
        private CavrnusSpaceConnection spaceConn;
        private void Awake()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc =>
            {
                spaceConn = sc;
            });
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "This component directly references the target material, behaving similarly to editing the shared material.\n" +
                "All renderers using the target material will update/sync accordingly.",
                MessageType.Info);

            if (EditorGUILayout.LinkButton("[Documentation]"))
                Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/edit-v2/895156484");

            base.OnInspectorGUI();

            EditorGUILayout.HelpBox(
                "Whe connected to a space, buttons will appear below to test swapping colors.",
                MessageType.Info);
            
            if (Application.isPlaying)
            {
                if (spaceConn == null)
                    return;
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Apply Color (Runtime Only)", EditorStyles.boldLabel);

                var component = (CavrnusPropertySyncMaterialColor) target;
                if (GUILayout.Button("Red"))
                    component.SetValue(Color.red);
                if (GUILayout.Button("Green"))
                    component.SetValue(Color.green);
                if (GUILayout.Button("Blue"))
                    component.SetValue(Color.blue);
            }
        }
    }
}
#endif