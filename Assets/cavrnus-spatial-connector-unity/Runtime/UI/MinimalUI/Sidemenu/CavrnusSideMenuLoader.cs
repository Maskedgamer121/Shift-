using System.Collections.Generic;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class CavrnusSideMenuLoader : MonoBehaviour
    {
        public List<CavrnusSideMenuData> Menus;

        private bool foundMenuManager;
        private void Update()
        {
            if (foundMenuManager) 
                return;
            
            if (CavrnusMinimalUIMenuManager.Instance != null) {
                CavrnusMinimalUIMenuManager.Instance.SideMenuManager.SetupMenus(Menus);

                foundMenuManager = true;

                enabled = false;
            }
        }
    }
}