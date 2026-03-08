using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Properties;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
    public class CavrnusLightSyncMenu : MonoBehaviour
    {
        [Header("Cavrnus Properties")]
        [SerializeField] private CavrnusPropertyBinderColor propertyBinderColor;
        [SerializeField] private CavrnusPropertyBinderFloat propertyBinderFloat;
        
        [Space]
        [SerializeField] private Light lightComponent;
        
        private List<IDisposable> disposables = new List<IDisposable>();
        
        private void Start()
        {
            disposables.Add(propertyBinderFloat.BindProperty(val =>
            {
                if (lightComponent != null) 
                    lightComponent.intensity = val;
            }));
            
            disposables.Add(propertyBinderColor.BindProperty(val =>
            {
                if (lightComponent != null) 
                    lightComponent.color = val;
            }));
        }
        
        private void OnDestroy() => disposables.ForEach(d => d?.Dispose());
    }
}