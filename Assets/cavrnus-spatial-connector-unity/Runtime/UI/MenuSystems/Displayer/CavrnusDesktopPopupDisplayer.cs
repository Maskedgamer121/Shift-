using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusDesktopPopupDisplayer : ICavrnusWidgetDisplayer
    {
        private ICavrnusCanvasLayerProvider canvasProvider;

        public CavrnusDesktopPopupDisplayer(ICavrnusCanvasLayerProvider canvasProvider)
        {
            this.canvasProvider = canvasProvider;
            canvasProvider.GetParentFor(CavrnusCanvasLayerTypeEnum.Popup);
        }
        
        public void Show(GameObject targetWidget)
        {
            var parent = canvasProvider.GetParentFor(CavrnusCanvasLayerTypeEnum.Popup);
            targetWidget.transform.SetParent(parent, worldPositionStays: false);

            // Center the widget
            var rectTransform = targetWidget.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
            }

            targetWidget.SetActive(true);
        }
        
        public void Destroy(GameObject targetWidget)
        {
            // maybe other special hide logic here?
            Object.Destroy(targetWidget);
        }
    }
}