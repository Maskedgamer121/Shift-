using Cavrnus.SpatialConnector.Setup;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class ServerMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField serverField;

        public void ConfirmDomain()
        {
            CavrnusSpatialConnector.Instance.YourServerDomain = serverField.text;
            CavrnusSpatialConnector.Instance.Startup();

		}
    }
}