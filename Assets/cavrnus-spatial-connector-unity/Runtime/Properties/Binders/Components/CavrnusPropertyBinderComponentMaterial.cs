using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
    [AddComponentMenu("Cavrnus/DataBinding/Binders/BindMaterial")]
    public class CavrnusPropertyBinderComponentMaterial : CavrnusPropertyBinderComponent
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderString propertyBinderString;
        [SerializeField] private Renderer materialRenderer;
        
        [Space]
        [SerializeField] private List<Material> materials;
        private readonly Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();
        
        private IDisposable binding;
        
        private void Start()
        {
            materials.ForEach(m => materialLookup.Add(m.name, m));
            binding = propertyBinderString.BindProperty(value =>
            {
                if (value == null) return;

                if (materialLookup.TryGetValue(value, out var found))
                {
                    if (materialRenderer != null)
                        materialRenderer.material = found;
                }
            });
        }
        
        private void OnDestroy() => binding?.Dispose();
    }
}