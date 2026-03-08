using System;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
    [AddComponentMenu("Cavrnus/DataBinding/Binders/BindLightIntensity")]
    public class CavrnusPropertyBinderComponentLightIntensity : CavrnusPropertyBinderComponent
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderFloat propertyBinderFloat;
        
        [Space]
        [SerializeField] private Light lightComponent;
        
        private IDisposable binding;
        
        private void Start()
        {
            binding = propertyBinderFloat.BindProperty(value =>
            {
                if (lightComponent != null) lightComponent.intensity = value;
            });
        }
        
        private void OnDestroy() => binding?.Dispose();
    }
}