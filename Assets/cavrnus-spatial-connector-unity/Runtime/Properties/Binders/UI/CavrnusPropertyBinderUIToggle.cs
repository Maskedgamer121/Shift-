using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Properties.UI
{
    [AddComponentMenu("Cavrnus/UI/Binders/BindUIToggle")]
    public class CavrnusPropertyBinderUIToggle : CavrnusPropertyBinderUI
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderBool binderBoolPropertyBinder;
        [SerializeField] private Toggle toggle;
        
        private IDisposable disp;
        
        private void Awake()
        {
			disp = binderBoolPropertyBinder.BindProperty(OnPropertyUpdated);
			toggle.onValueChanged.AddListener(ToggleClicked);
		}

        private void OnPropertyUpdated(bool val) => toggle.SetIsOnWithoutNotify(val);

        public void ToggleClicked(bool val) => binderBoolPropertyBinder.SetValue(val);
        
        private void OnDestroy()
        {
            disp?.Dispose();
			toggle.onValueChanged.RemoveListener(ToggleClicked);
        }
    }
}