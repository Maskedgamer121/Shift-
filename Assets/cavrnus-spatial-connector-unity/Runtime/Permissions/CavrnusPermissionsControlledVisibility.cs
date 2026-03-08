using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Permissions
{
	public class CavrnusPermissionsControlledVisibility : MonoBehaviour
    {
        [SerializeField] private bool isGlobalPolicy;
        [SerializeField] private string permissionAction;

        private IDisposable disposable;
        private CavrnusPropertiesContainer ctx;

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                if (isGlobalPolicy) {
                    disposable =
                        CavrnusFunctionLibrary.BindGlobalPolicy(permissionAction, b => { gameObject.SetActive(b); });
                }
                else { disposable = sc.BindSpacePolicy(permissionAction, b => { gameObject.SetActive(b); }); }
            });
        }

        private void OnDestroy() => disposable?.Dispose();
    }
}