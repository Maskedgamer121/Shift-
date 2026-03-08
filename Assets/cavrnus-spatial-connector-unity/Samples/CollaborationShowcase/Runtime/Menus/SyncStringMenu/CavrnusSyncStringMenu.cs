using System;
using Cavrnus.SpatialConnector.Properties;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
    public class CavrnusSyncStringMenu : MonoBehaviour
    {
        [SerializeField] private CavrnusPropertyBinderString propertyBinder;
        
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private TextMeshProUGUI tmPro;

        private IDisposable bind;
        
        private void Start()
        {
            bind = propertyBinder?.BindProperty(newString => {
                if (tmPro) tmPro.text = newString;
            });
            
            addButton.onClick.AddListener(AddButtonClicked);
            removeButton.onClick.AddListener(RemoveButtonClicked);
        }

        private void RemoveButtonClicked()
        {
            var current = tmPro.text;
            if (current.Length != 0)
                PostStringUpdate(current.Remove(current.Length - 1));
        }

        private void AddButtonClicked()
        {
            PostStringUpdate(tmPro.text + "!");
        }

        private void PostStringUpdate(string newString) => propertyBinder.SetValue(newString);

        private void OnDestroy()
        {
            bind?.Dispose();
            addButton.onClick.RemoveListener(AddButtonClicked);
            removeButton.onClick.RemoveListener(RemoveButtonClicked);
        }
    }
}