using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusPopupHeader : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        
        private void Start()
        {
            closeButton.onClick.AddListener(CloseButtonClicked);
        }

        private void CloseButtonClicked()
        {
            CavrnusUIManager.Instance.Popups.DestroyAll();
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(CloseButtonClicked);
        }
    }
}