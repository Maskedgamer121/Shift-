using System;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
    [AddComponentMenu("Cavrnus/DataBinding/Binders/BindMaterialColor")]
    public class CavrnusPropertyBinderComponentMaterialColor : CavrnusPropertyBinderComponent
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderColor propertyBinderColor;
        
        [Space]
        [SerializeField] private string colorPropertyName = "_BaseColor";
        [SerializeField] private Material targetMaterial;

        private IDisposable binding;
        
        private void Start()
        {
            binding = propertyBinderColor.BindProperty(color =>
            {
                targetMaterial?.SetColor(colorPropertyName, color);
            });
        }

        private void OnDestroy() => binding?.Dispose();
    }
}