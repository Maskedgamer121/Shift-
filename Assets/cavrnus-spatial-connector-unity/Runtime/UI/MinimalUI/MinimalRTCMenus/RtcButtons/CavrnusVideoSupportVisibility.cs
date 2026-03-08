using Cavrnus.SpatialConnector.Setup;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusVideoSupportVisibility : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(!CavrnusSpatialConnector.Instance.AdditionalSettings.DisableVideo);
        }
    }
}