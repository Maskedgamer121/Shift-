using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Properties;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
    public class CavrnusDirectionalLightManager : MonoBehaviour
    {
        [Header("Cavrnus Properties")]
        [SerializeField] private CavrnusPropertyBinderFloat lightRotationPropertyBinder;
        [SerializeField] private CavrnusPropertyBinderFloat shadowStrengthPropertyBinder;
        
        [Header("Unity Components")]
        [SerializeField] private GameObject lightContainer;
        [SerializeField] private Light targetLight;
        
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");

        private List<IDisposable> binds = new List<IDisposable>();        
        private void Start()
        {
            lightRotationPropertyBinder.BindProperty(val =>
            {
                RenderSettings.skybox.SetFloat(Rotation, val);
                lightContainer.transform.localRotation = Quaternion.Euler(new Vector3(0, -val, 0));      
            });
            
            shadowStrengthPropertyBinder.BindProperty(val =>
            {
                targetLight.shadowStrength = val;
            });
        }

        private void OnDestroy()
        {
            binds.ForEach(b => b?.Dispose());
        }
    }
}