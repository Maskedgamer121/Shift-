using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Properties;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
	public class CavrnusColorSelectorMenu : MonoBehaviour
    {
        [SerializeField] private CavrnusPropertyBinderColor propertyBinder;
                
        [Space]
        [SerializeField] private CavrnusColorPickerEntry colorPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private List<Color> colors;
        
        private readonly List<CavrnusColorPickerEntry> colorItems = new List<CavrnusColorPickerEntry>();
        private IDisposable binding;

        private void Start()
        {
            foreach (var color in colors)
            {
                var go = Instantiate(colorPrefab, container);
                var colorItem = go.GetComponent<CavrnusColorPickerEntry>();
                colorItems.Add(colorItem);

                colorItem.Setup(color, ColorSelected);
            }

            binding = propertyBinder?.BindProperty(col =>
            {
                foreach (var item in colorItems)
                {
                    item.SetSelectionState(ColorsEqual(item.Color, col));
                }
            });
        }

        private void ColorSelected(Color val)
        {
            propertyBinder?.SetValue(val);
        }
        
        private bool ColorsEqual(Color c1, Color c2, float tolerance = 0.1f)
        {
            return false;
        }
        
        private void OnDestroy() => binding?.Dispose();
    }
}