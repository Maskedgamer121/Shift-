using System;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public enum CavrnusCanvasLayerTypeEnum
    {
        Popup
    }
    public class CavrnusDefaultCanvasLayerProvider : MonoBehaviour, ICavrnusCanvasLayerProvider
    {
        [SerializeField] private Transform popupLayer;
        
        public Transform GetParentFor(CavrnusCanvasLayerTypeEnum layerType)
        {
            switch (layerType)
            {
                case CavrnusCanvasLayerTypeEnum.Popup:
                    return popupLayer; 
                default:
                    throw new ArgumentOutOfRangeException(nameof(layerType), layerType, null);
            }
        }
    }
}