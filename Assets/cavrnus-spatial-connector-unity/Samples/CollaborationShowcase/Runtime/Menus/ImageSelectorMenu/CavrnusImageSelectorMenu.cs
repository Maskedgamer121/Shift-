using Cavrnus.Base.Collections;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
	public class CavrnusImageSelectorMenu : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private CavrnusPropertySyncSprite syncer;
        [SerializeField] private Button button;

        private void Start()
        {
            button.interactable = false;

            CavrnusFunctionLibrary.AwaitAnySpaceConnection(csc => {
                button.onClick.AddListener(SelectRandomImage);
                button.interactable = true;
            });
        }

        public void SelectRandomImage() { image.sprite = syncer.SpriteLookup.TakeRandom().Value; }

        private void OnDestroy() { button.onClick.RemoveListener(SelectRandomImage); }
    }
}