using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.UI
{
    [AddComponentMenu("Cavrnus/UI/Binders/BindUIDropdown")]
    public class CavrnusPropertyBinderUIDropdown : CavrnusPropertyBinderUI
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderString binderStringPropertyBinder;
        
        [Header("Dropdown")]
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private List<string> dropdownOptions;
        
        private IDisposable binding;
        private Dictionary<string, int> valueToIndex = new Dictionary<string, int>();
        
        private void Awake()
        {
            if (dropdownOptions.Count > 0)
                SetupDropdown(dropdownOptions);
        }
        
        public void SetupDropdown(List<string> options)
        {
            dropdown.ClearOptions();
            dropdownOptions = options;
            CreateValueIndexMap(options);
            BindDropdown();
        }
        
        private void BindDropdown()
        {
            binding = binderStringPropertyBinder.BindProperty(val =>
            {
                if (valueToIndex.TryGetValue(val.ToLowerInvariant(), out var found))
                    dropdown.SetValueWithoutNotify(found);
            });
            
            dropdown.onValueChanged.AddListener(HandleValueChanged);
        }

        private void HandleValueChanged(int index)
        {
            if (index >= 0 && index < dropdownOptions.Count)
                binderStringPropertyBinder.SetValue(dropdownOptions[index]);
        }

        private void CreateValueIndexMap(List<string> options)
        {
            valueToIndex = new Dictionary<string, int>();
            
            dropdown.AddOptions(options);
            for (var index = 0; index < options.Count; index++)
            {
                var op = options[index];
                valueToIndex.Add(op, index);
            }
        }
        
        private void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(HandleValueChanged);
            binding?.Dispose();
        }
    }
}