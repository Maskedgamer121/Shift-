using System;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
    [AddComponentMenu("Cavrnus/DataBinding/Binders/BindLightColor")]
    public class CavrnusPropertyBinderComponentLightColor : CavrnusPropertyBinderComponent
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderColor propertyBinderColor;
        
        [Space]
        [SerializeField] private Light lightComponent;

        private IDisposable binding;
        
        private void Start()
        {
            binding = propertyBinderColor.BindProperty(color =>
            {
                if (lightComponent != null) lightComponent.color = color;
            });
        }

        private void OnDestroy() => binding?.Dispose();
    }
}