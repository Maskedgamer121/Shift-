#if UNITY_EDITOR
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Editor.BindComponents
{
    [CustomEditor(typeof(CavrnusPropertySyncMaterialTexture))]
    public class SyncMaterialTextureEditor : UnityEditor.Editor
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
                "Syncs the material's texture by name using a local lookup.\n" +
                "All clients must have the same textures available locally.\n" +
                "Use the 'Textures' list to define the available options.",
                MessageType.Info);

            if (EditorGUILayout.LinkButton("[Documentation]"))
                Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/edit-v2/895156484");

            base.OnInspectorGUI();

            EditorGUILayout.HelpBox(
                "For testing, populate the Textures collection and join a space. Buttons will appear below to swap textures.",
                MessageType.Info);
            
            // Runtime interaction UI
            if (Application.isPlaying)
            {
                if (spaceConn == null)
                    return;
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Apply Texture (Runtime Only)", EditorStyles.boldLabel);

                var component = (CavrnusPropertySyncMaterialTexture) target;
                foreach (var texture in component.textures)
                {
                    if (GUILayout.Button(texture.name))
                        component.SetValue(texture.name);
                }
            }
        }
    }
}
#endif