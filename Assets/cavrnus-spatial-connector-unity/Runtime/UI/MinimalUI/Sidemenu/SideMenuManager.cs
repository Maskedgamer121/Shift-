using System.Collections.Generic;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class SideMenuManager : MonoBehaviour
    {
        [SerializeField] private SideMenuContainer sideMenuContainer;
        
        [Space]
        [SerializeField] private Transform buttonsContainer;
        [SerializeField] private SideMenuButton buttonPrefab;

        private int currentOpenMenuId = -1;
        
        // Created UI Elements
        private readonly List<CavrnusSideMenuData> instantiatedMenus = new ();
        private readonly List<SideMenuButton> instantiatedButtons = new();

        private void Awake()
        {
            sideMenuContainer.ManuallyClosed += SideMenuContainerOnManuallyClosed;
        }
        
        public void SetupMenus(List<CavrnusSideMenuData> menuData)
        {
            menuData.ForEach(SetupMenu);
        }

        public void SetupMenu(CavrnusSideMenuData menuData)
        {
            var instantiatedMenu = Instantiate(menuData.Menu);
            menuData.Menu = instantiatedMenu;
            instantiatedMenus.Add(menuData);
            
            var button = Instantiate(buttonPrefab, buttonsContainer, false);
            instantiatedButtons.Add(button);

            button.Setup(instantiatedButtons.Count - 1, menuData);
            button.ButtonSelected += ButtonOnButtonSelected;
            
            sideMenuContainer.AddMenuToContainer(menuData);
        }

        private void SideMenuContainerOnManuallyClosed()
        {
            instantiatedButtons[currentOpenMenuId].SetState(false);
            currentOpenMenuId = -1;
        }

        private void ButtonOnButtonSelected(int menuId)
        {
            ResetButtons();

            // close current menu and deactivate button
            if (menuId == currentOpenMenuId) {
                sideMenuContainer.SetMenuContainerVisibility(false);
                sideMenuContainer.SetTargetMenuVisibility(menuId, instantiatedMenus[menuId],false);
                
                currentOpenMenuId = -1;
            }
            else {
                instantiatedButtons[menuId].SetState(true);
                sideMenuContainer.SetMenuContainerVisibility(true);
                sideMenuContainer.SetTargetMenuVisibility(menuId, instantiatedMenus[menuId],true);

                currentOpenMenuId = menuId;
            }
        }

        private void ResetButtons()
        {
            instantiatedButtons.ForEach(b => b.SetState(false));
        }

        private void OnDestroy()
        {
            foreach (var button in instantiatedButtons) {
                button.ButtonSelected -= ButtonOnButtonSelected;
            }
            
            sideMenuContainer.ManuallyClosed -= SideMenuContainerOnManuallyClosed;
        }
    }
}