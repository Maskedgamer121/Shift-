using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public interface ICavrnusWidgetDisplayer
    {
        void Show(GameObject targetWidget);
        void Destroy(GameObject targetWidget);
    }
}