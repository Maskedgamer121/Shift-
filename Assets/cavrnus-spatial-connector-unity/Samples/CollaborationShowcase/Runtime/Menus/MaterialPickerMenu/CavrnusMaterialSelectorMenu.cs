using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Properties;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
	public class CavrnusMaterialSelectorMenu : MonoBehaviour
    {
        [Header("Cavrnus")]
        [SerializeField] private CavrnusPropertyBinderString propertyBinder;
                
        [Space]
        [SerializeField] private List<Material> materials;

        [Space] 
        [SerializeField] private CavrnusMaterialSelectorEntry cavrnusMaterialSelectorEntryPrefab;
        [SerializeField] private Transform entriesContainer;

        private readonly List<CavrnusMaterialSelectorEntry> materialEntries = new List<CavrnusMaterialSelectorEntry>();
        private IDisposable binding;

        private void Start()
        {
            foreach (var material in materials) {
                var item = Instantiate(cavrnusMaterialSelectorEntryPrefab, entriesContainer);
                item.Setup(material, MaterialSelected);
                
                materialEntries.Add(item);
            }
            
            binding = propertyBinder?.BindProperty(val => {
                foreach (var item in materialEntries) {
                    item.SetSelectionState(string.Equals(item.MaterialName, val));
                }
            });
        }

        private void MaterialSelected(Material material)
        {
            propertyBinder?.SetValue(material.name);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}