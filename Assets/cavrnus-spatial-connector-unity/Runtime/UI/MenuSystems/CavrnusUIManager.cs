using UnityEngine;
using UnityEngine.Serialization;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusUIManager : MonoBehaviour
    {
        public static CavrnusUIManager Instance { get; private set; }
        
        [FormerlySerializedAs("menuLookup")]
        [Header("Menus")] 
        [SerializeField] private CavrnusMenuLookup menuLookupPrefab;
        
        [Header("Canvas System")]
        [SerializeField] private CavrnusDefaultCanvasLayerProvider canvasLayerProviderPrefab;
        
        public ICavrnusPopupSystem Popups { get; private set; }
        public CavrnusMenuLookup MenuLookupPrefab => menuLookupPrefab;
        
        private void Awake()
        {
            Instance = this;

            menuLookupPrefab = Instantiate(menuLookupPrefab);
            
            var displayer = new CavrnusDesktopPopupDisplayer(Instantiate(canvasLayerProviderPrefab));
            Popups = new CavrnusDesktopPopupSystem(displayer);
        }
    }
}