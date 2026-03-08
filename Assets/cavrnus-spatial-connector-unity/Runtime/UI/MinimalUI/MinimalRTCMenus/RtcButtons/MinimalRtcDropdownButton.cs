using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class MinimalRtcDropdownButton : MonoBehaviour
    {
        public event Action<MinimalRtcDropdownButton, bool> OnDropdownButtonClicked;
        
        [SerializeField] private Button dropdownButton;
        [SerializeField] private RtcUiDropdownBase dropdown;
        [SerializeField] private GameObject arrowIconOn;
        [SerializeField] private GameObject arrowIconOff;
        
        private string id;
        private bool currentState = false;

        public void Setup(string id)
        {
            this.id = id;
            SetDropdownActiveState(false);
            
            dropdownButton.onClick.AddListener(DropdownButtonClicked);
        }
        
        private void DropdownOnOnUserClickedOffThisDropdown()
        {
            DropdownButtonClicked();
        }
        
        private void DropdownButtonClicked()
        {
            OnDropdownButtonClicked?.Invoke(this, currentState);
        }
        
        public void SetDropdownActiveState(bool state)
        {
            dropdown.SetActiveState(state);
            arrowIconOn.SetActive(state);
            arrowIconOff.SetActive(!state);
            
            currentState = state;
        }

#if UNITY_STANDALONE
        private void Update()
        {
            if (currentState) {
                DetectClickOutside();
            }
        }
#endif
        
         private void DetectClickOutside()
         {
             // Check for mouse down
             if (Input.GetMouseButtonDown(0)) {
                 // If pointer is over any UI element, check if it is part of this button or the dropdown
                 if (!EventSystem.current.IsPointerOverGameObject()) // Empty space click
                 {
                     DropdownButtonClicked();
                 }
                 else {
                     var pointerData = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

                     // Raycast to detect UI elements hit
                     var raycastResults = new System.Collections.Generic.List<RaycastResult>();
                     EventSystem.current.RaycastAll(pointerData, raycastResults);

                     var clickedOnThisObject = false;

                     foreach (var result in raycastResults) {
                         // Check if the click is on the button, dropdown, or their children
                         if (result.gameObject == gameObject || 
                             result.gameObject == dropdown.gameObject ||
                             result.gameObject.transform.IsChildOf(transform) ||
                             result.gameObject.transform.IsChildOf(dropdown.transform)) {
                             clickedOnThisObject = true;
                             break;
                         }
                     }

                     // If the click is outside, deactivate the dropdown and stop listening
                     if (!clickedOnThisObject) {
                         DropdownButtonClicked();
                     }
                 }
             }
         }
         
         public override bool Equals(object other)
        {
            if (other is MinimalRtcDropdownButton otherButton)
                return id == otherButton.id;
            
            return false;
        }
         
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        
        private void OnDestroy()
        {
            dropdownButton.onClick.RemoveListener(DropdownButtonClicked);
        }
    }
}