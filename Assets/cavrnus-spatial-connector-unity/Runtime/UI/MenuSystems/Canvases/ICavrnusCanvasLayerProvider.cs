using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public interface ICavrnusCanvasLayerProvider
    {
        Transform GetParentFor(CavrnusCanvasLayerTypeEnum layerType);
    }
}