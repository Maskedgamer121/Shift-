using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class LoginPanel : MonoBehaviour
	{
		[SerializeField] private TMP_InputField email;
		[SerializeField] private TMP_InputField password;

		public void Authenticate()
		{
			CavrnusFunctionLibrary.AuthenticateWithPassword(CavrnusSpatialConnector.Instance.YourServerDomain, email.text, password.text, auth => { }, err => Debug.LogError(err));
		}
	}
}