using Cavrnus.SpatialConnector.UI;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusOpenMenu : MonoBehaviour
    {
        public void OpenMenu(string menuId)
        {
            var menu = CavrnusUIManager.Instance.MenuLookupPrefab.Get(menuId);
            CavrnusUIManager.Instance.Popups.Create<CavrnusChatMenu>(menu);
        }
    }
}