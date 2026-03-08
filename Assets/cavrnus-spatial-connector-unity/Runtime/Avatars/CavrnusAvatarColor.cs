using System;
using System.Collections.Generic;
using Cavrnus.EngineConnector;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Avatars
{
	public class CavrnusAvatarColor : MonoBehaviour
    {
        [SerializeField] private List<Renderer> primaryMeshRenderers;
        [SerializeField] private List<Renderer> secondaryMeshRenderers;
        
        private List<IDisposable> bindings = new List<IDisposable>();

        private void Start()
        {
            var userFlag = gameObject.GetComponentInAllParents<CavrnusUserFlag>();
            userFlag.AwaitUser(user =>
            {
                if (user != null) {
                    bindings.Add(user.BindColorPropertyValue("primaryColor", OnPrimaryColorUpdated));
                    bindings.Add(user.BindColorPropertyValue("secondaryColor", OnSecondaryColorUpdated));
                }
            });
        }

        private void OnPrimaryColorUpdated(Color val) => UpdateColor(primaryMeshRenderers, val);
        private void OnSecondaryColorUpdated(Color val) => UpdateColor(secondaryMeshRenderers, val);

        private static void UpdateColor(List<Renderer> renderers, Color val)
        {
            foreach (var r in renderers) {
                foreach (var mat in r.materials) { mat.color = val; }
            }
        }

        private void OnDestroy()
        {
            foreach (var binding in bindings)
                binding?.Dispose();
        }
    }
}