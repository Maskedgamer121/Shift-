using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class CavrnusEnableComponentsOnSpaceJoin : MonoBehaviour
    {
        [SerializeField] private List<Component> components;

        private void Awake() { SetComponentsEnabled(false); }

        private void Start() { CavrnusFunctionLibrary.AwaitAnySpaceConnection(csc => { SetComponentsEnabled(true); }); }

        private void SetComponentsEnabled(bool enabled)
        {
            foreach (var component in components) {
                if (component != null)
                    if (component is Behaviour behaviour)
                        behaviour.enabled = enabled;
            }
        }
    }
}