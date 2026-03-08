using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.MobileDemo
{
    public class PopupHelper : MonoBehaviour
    {
        public void ClosePopup()
        {
            PopupCanvas.Instance.Close();
        }
    }
}