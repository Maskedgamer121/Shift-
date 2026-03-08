using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class SideMenuButton : MonoBehaviour
    {
        public event Action<int> ButtonSelected;
        
        [SerializeField] private UnityEvent<bool> onStateChanged;
        [SerializeField] private Image image;
        
        [SerializeField] private Button button;

        private int menuId;
        
        public void Setup(int id, CavrnusSideMenuData data)
        {
            menuId = id;
            image.sprite = data.MenuIcon;
            
            SetState(false);
        }

        private void Start()
        {
            button.onClick.AddListener(ButtonClick);
        }
        
        public void SetState(bool state)
        {
            onStateChanged?.Invoke(state);
        }

        private void ButtonClick()
        {
            ButtonSelected?.Invoke(menuId);
        }
    }
}