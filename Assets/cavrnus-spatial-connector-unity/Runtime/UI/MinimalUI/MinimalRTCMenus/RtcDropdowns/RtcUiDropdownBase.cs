using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public abstract class RtcUiDropdownBase : MonoBehaviour
    {
        public event Action<int> OnDropdownValueChanged;
        
        [SerializeField] private Sprite icon;
        [SerializeField] private Image imageContainer;
        
        [SerializeField] protected TMP_Dropdown Dropdown;
        
        private CanvasGroup cg;

        protected CavrnusSpaceConnection SpaceConnection;

        public void Setup()
        {
            cg = gameObject.AddComponent<CanvasGroup>();

            imageContainer.sprite = icon;
            
            Dropdown.onValueChanged.AddListener(DropdownValueChanged);
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(connection =>
            {
                SpaceConnection = connection;
                OnSpaceConnected();
            });
        }

        protected virtual void DropdownValueChanged(int val)
        {
            OnDropdownValueChanged?.Invoke(val);
        }

        public void SetActiveState(bool state)
        {
            gameObject.DoFade(new List<CanvasGroup> {cg}, 0.3f, state);
            gameObject.SetActive(state);
        }

        protected virtual void OnDestroy()
        {
            Dropdown.onValueChanged.RemoveListener(DropdownValueChanged);
        }

        protected abstract void OnSpaceConnected();
    }
}