using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class GuestJoinMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;

        public void Authenticate()
        {
            CavrnusFunctionLibrary.AuthenticateAsGuest(CavrnusSpatialConnector.Instance.YourServerDomain, nameField.text, auth => { }, err => Debug.LogError(err));
        }
    }
}