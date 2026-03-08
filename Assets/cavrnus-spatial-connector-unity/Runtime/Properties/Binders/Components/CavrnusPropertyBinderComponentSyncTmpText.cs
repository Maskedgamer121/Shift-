using System;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
    [AddComponentMenu("Cavrnus/DataBinding/Binders/BindTextMeshPro")]
    public class CavrnusPropertyBinderComponentSyncTmpText : CavrnusPropertyBinderComponent
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderString propertyBinderString;
        
        [Space]
        [SerializeField] private TextMeshProUGUI tmpro;
        
        private IDisposable binding;
        
        private void Start()
        {
            binding = propertyBinderString.BindProperty(value =>
            {
                if (tmpro != null) tmpro.text = value;
            });
        }
        
        private void OnDestroy() => binding?.Dispose();
    }
}