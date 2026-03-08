using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Properties.UI
{
    [AddComponentMenu("Cavrnus/UI/Binders/BindUISlider")]
    public class CavrnusPropertyBinderUISlider : CavrnusPropertyBinderUI
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderFloat binderFloatPropertyBinder;
        [SerializeField] private Slider slider;

        [SerializeField] private bool overrideMinMax;
        [SerializeField] private Vector2 minMax;
        
        private IDisposable disp;

        private void Awake()
        {
            disp = binderFloatPropertyBinder.BindProperty(OnPropertyUpdated);
            if (slider)
            {
                slider.onValueChanged.AddListener(OnValueChanged);
                if (overrideMinMax)
                {
                    slider.minValue = minMax.x;
                    slider.maxValue = minMax.y;
                }
            }
        }

        private void OnPropertyUpdated(float val) => slider.SetValueWithoutNotify(val);
        private void OnValueChanged(float val) => binderFloatPropertyBinder.SetValue(val);

        private void OnDestroy()
        {
            disp?.Dispose();
			slider.onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}