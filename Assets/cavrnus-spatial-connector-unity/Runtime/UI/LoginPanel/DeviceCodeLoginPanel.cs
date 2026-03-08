using System;
using System.Threading.Tasks;
using Cavrnus.Comm;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class DeviceCodeLoginPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text manualLinkText;
		[SerializeField] private TMP_Text codeText;
		[SerializeField] private GameObject progressDiv;
		[SerializeField] private Button loginButton;

		private bool activelyAuthenticating = false;
		private string linkTarget = "";

		public void OpenCurrentCodeLink()
		{
			Application.OpenURL(linkTarget);
		}
		public void BeginAuthenticate()
		{
			loginButton.interactable = false;
			progressDiv.SetActive(false);

			// You can change this, btw!
			string activationmessage = $"You have been authorized and may close this window and return to {Cavrnus.Comm.IntegrationInfo.Info.ClientId}.";

			CavrnusFunctionLibrary.BeginAuthenticationViaDeviceCode(CavrnusSpatialConnector.Instance.YourServerDomain, true, activationmessage, (codedata) =>
			{
				// Update UI to show code or at least that progress is being made.
				// Actually we want to wait a little bit to show it, because we don't want to confuse the user into clicking the link before its obvious that the 
				// browser isn't already being loaded.

				activelyAuthenticating = true;

				Task.Delay(TimeSpan.FromSeconds(4)).ContinueWith((_) =>
				{
					if (activelyAuthenticating) // still
					{
						progressDiv.SetActive(true);

						linkTarget = codedata.verificationUrl;
						manualLinkText.text = $"If your browser does not open,\n click here to manually open it.";
						codeText.text = $"Your user code is '{codedata.userCode}";
					}
				});

				CavrnusFunctionLibrary.ConcludeAuthenticationViaDeviceCode(CavrnusSpatialConnector.Instance.YourServerDomain, codedata.deviceCode, codedata.userCode, (auth) =>
				{
					activelyAuthenticating = false;
					linkTarget = "";
					progressDiv.SetActive(false);
				}, err =>
				{
					activelyAuthenticating = false;
					linkTarget = "";
					progressDiv.SetActive(false);

					Debug.LogError(err);
				});

			}, err => Debug.LogError(err));
		}
	}
}