using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.SpatialConnector.Editor;
using UnityEditor;
using UnityEngine;

namespace Assets.com.cavrnus.spatialconnector.Editor
{
	public class CavrnusEditorState
	{
		public string uniqueGuid;

		public bool shouldAutoOpen = true;

		public bool isAuthenticateOpen = true;
		public bool isSetupOpen = false;
		public bool isSyncOpen = false;

		public string CavrnusServer = "";
		public string editorApiKey;
		public string editorApiToken;

		public RestApiEndpoint editorEndpoint;

		public RoomMetadataRest[] loggedInRooms;
		public string loggedInRoomsTestingSpaceId;
		public bool loggedInRoomsTestSpaceJustCreated = false;
		public string loggedInUserName;
		public string loggedInOrgName;

		public bool awaitingDeviceLogin;
		public string deviceLoginUrl;
		public string deviceLoginCode;
		public string deviceLoginError;

		public bool IsLoggedIn => editorEndpoint.AuthorizationToken != null;

		public CavrnusEditorState()
		{
			uniqueGuid = EditorPrefs.GetString("CavrnusUniqueGuid", Guid.NewGuid().ToString());

			shouldAutoOpen = EditorPrefs.GetBool("ShouldAutoOpen", true);

			CavrnusServer = EditorPrefs.GetString("CavrnusServer");

			editorApiKey = EditorPrefs.GetString("CavrnusEditorApiAccessKey");
			editorApiToken = EditorPrefs.GetString("CavrnusEditorApiAccessToken");
		}

		public async Task Initialize()
		{
			editorEndpoint = new RestApiEndpoint("api.cavrn.us", null, null);
			if (!String.IsNullOrWhiteSpace(CavrnusServer))
				editorEndpoint = RestApiEndpoint.ParseFromHostname(CavrnusServer);

			if (!String.IsNullOrWhiteSpace(editorApiKey) && !String.IsNullOrWhiteSpace(editorApiToken))
			{
				var ruc = new RestUserCommunication(editorEndpoint);

				try
				{
					var loginres = await ruc.PostApiKeyLoginAsync(new RestUserCommunication.ApiKeyLoginRequest()
					{
						accessKey = editorApiKey, accessToken = editorApiToken
					});

					editorEndpoint = editorEndpoint.WithCustomerSubdomain(loginres.domain).WithAuthorization(loginres.token);

					var rucauth = new RestUserCommunication(editorEndpoint);
					var profile = await rucauth.GetUserProfileAsync();

					var rrcauth = new RestRoomCommunication(editorEndpoint);
					var rooms = await rrcauth.GetUserFullRoomsAndInvitesInfoAsync();
					loggedInRooms = rooms.rooms.Where(r => (r.members.FirstOrDefault(m => m.email == rooms.userProfile?._id)?.hidden ?? false) == true).ToArray();
					
					//Debug.Log($"Logged in, seeing {loggedInRooms.Length} rooms.");
					loggedInUserName = profile.GetVisibleName().Trim();
					loggedInOrgName = profile.customer.name;

					isSetupOpen = true;
					isSyncOpen = false;

					var foundroom = loggedInRooms.FirstOrDefault(r => r.name == "Unity Testing Space");
					if (foundroom == null) // create a new room for them
					{
						var crr = await rrcauth.PostCreateRoomAsync(new RestRoomCommunication.CreateRoomRequest()
						{
							description = "Created by Unity-Editor for easy startup",
							name = "Unity Testing Space",
							keywords = new string[] { "unity-editor-auto" }
						});
						loggedInRoomsTestingSpaceId = crr._id;
						loggedInRoomsTestSpaceJustCreated = true;
					}
					else
					{
						loggedInRoomsTestingSpaceId = foundroom._id;
					}
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Editor failed to login to Cavrnus with existing Api Key. Re-login if needed ( {e.Message} ).");
				}
			}
		}
		
		public void ChangeServer(string server)
		{
			if (server == CavrnusServer)
				return;

			EditorPrefs.SetString("CavrnusServer", server);
			CavrnusServer = server;

			InitEndpointFromServer();

			EditorPrefs.SetString("CavrnusEditorApiAccessKey", "");
			EditorPrefs.SetString("CavrnusEditorApiAccessToken", "");
			editorApiKey = "";
			editorApiToken = "";
		}

		private void InitEndpointFromServer()
		{
			if (String.IsNullOrWhiteSpace(CavrnusServer) || CavrnusServer == "cavrn.us" || CavrnusServer == "api.cavrn.us")
				editorEndpoint = new RestApiEndpoint("api.cavrn.us", null, null);
			else if (CavrnusServer == "dev.cavrn.us" || CavrnusServer == "api.dev.cavrn.us")
				editorEndpoint = new RestApiEndpoint("api.dev.cavrn.us", null, null);
			else if (CavrnusServer == "stage.cavrn.us" || CavrnusServer == "api.stage.cavrn.us")
				editorEndpoint = new RestApiEndpoint("api.stage.cavrn.us", null, null);
			else if (!CavrnusServer.Contains(".") && !CavrnusServer.StartsWith("http")) // single word, not on-prem, interpret as domain for production.
				editorEndpoint = new RestApiEndpoint("api.cavrn.us", CavrnusServer, null);
			else
				editorEndpoint = RestApiEndpoint.ParseFromHostname(CavrnusServer);
		}

		public async Task DoConnectToCavrnus()
		{
			CavrnusSetupHelpers.SendMetric("csc-unity-editorlogin");
	
			isSyncOpen = false;
			isSetupOpen = false;

			RestUserCommunication ruc = new RestUserCommunication(editorEndpoint.WithCustomerSubdomain(null));
			var dcrequest = await ruc.PostRequestDeviceCode(new RestUserCommunication.RequestAuthDeviceCode()
			{
				customActivatedMessage = "Your Unity Editor is logged in and ready! Return to your Unity Editor and get building!",
				source = "unity-editor",
			});

			awaitingDeviceLogin = true;
			deviceLoginCode = dcrequest.userCode;
			deviceLoginUrl = dcrequest.verificationUrl;
			deviceLoginError = null;

			Application.OpenURL(dcrequest.verificationUrl);

			// ui ready, now await actual login.
			while (true)
			{
				try
				{
					var dclogin = await ruc.PostDeviceCodeLoginAsync(dcrequest.deviceCode, dcrequest.userCode, 15 * 60 * 1000);

					// success, if here.
					editorEndpoint = editorEndpoint.WithCustomerSubdomain(dclogin.domain).WithAuthorization(dclogin.token);
					Debug.Log($"Logged in: {dclogin.token} to {dclogin.domain}");

					EditorPrefs.SetString("CavrnusServer", editorEndpoint.ToUserDomainString());
					CavrnusServer = editorEndpoint.ToUserDomainString();

					var rucauth = new RestUserCommunication(editorEndpoint);

					var profile = await rucauth.GetUserProfileAsync();

					loggedInUserName = profile.GetVisibleName().Trim();
					loggedInOrgName = profile.customer.name.Trim();

					var rrcauth = new RestRoomCommunication(editorEndpoint);
					var rooms = await rrcauth.GetUserFullRoomsAndInvitesInfoAsync();
					loggedInRooms = rooms.rooms.Where(r=>(r.members.FirstOrDefault(m=>m.email == rooms.userProfile?._id)?.hidden ?? false) == true).ToArray();

					loggedInRoomsTestingSpaceId = "";
					var foundroom = loggedInRooms.FirstOrDefault(r => r.name == "Unity Testing Space");
					if (foundroom == null) // create a new room for them
					{
						var crr = await rrcauth.PostCreateRoomAsync(new RestRoomCommunication.CreateRoomRequest()
						{
							description = "Created by Unity-Editor for easy startup",
							name = "Unity Testing Space",
							keywords = new string[]{"unity-editor-auto"}
						});
						loggedInRoomsTestingSpaceId = crr._id;
						loggedInRoomsTestSpaceJustCreated = true;
					}
					else
					{
						loggedInRoomsTestingSpaceId = foundroom._id;
					}

					isSyncOpen = false;
					isSetupOpen = true;

					var apikeygen = await rucauth.PostCreateApiKey(new RestUserCommunication.CreateApiKeyRequest(){name=$"unity-editor-connect-{DateTime.Today.ToString()}"});

					EditorPrefs.SetString("CavrnusEditorApiAccessKey", apikeygen.key);
					EditorPrefs.SetString("CavrnusEditorApiAccessToken", apikeygen.secret);
					editorApiKey = apikeygen.key;
					editorApiToken = apikeygen.secret;

					return;
				}
				catch (ErrorInfo ei)
				{
					if (ei.status == 401 &&
					    ei.message.Contains("Authorization not authorized")) // just means to wait more
					{
						await Task.Delay(1000);
						continue;
					}
					if (ei.status == 404)
					{
						await Task.Delay(1000);
						continue;
					}

					awaitingDeviceLogin = false;
					deviceLoginError = ei.message;

					throw ei;
				}
			}
		}

		public void SetShouldAutoOpen(bool b)
		{
			shouldAutoOpen = b;
			EditorPrefs.SetBool("ShouldAutoOpen", b);
		}

		public void Logout()
		{
			EditorPrefs.SetString("CavrnusEditorApiAccessKey", "");
			EditorPrefs.SetString("CavrnusEditorApiAccessToken", "");
			editorApiKey = "";
			editorApiToken = "";

			InitEndpointFromServer(); // resets the endpoint

			deviceLoginError = "";
			deviceLoginCode = "";
			deviceLoginUrl = "";
			loggedInOrgName = "";
			loggedInUserName = "";
			awaitingDeviceLogin = false;

			isSyncOpen = false;
			isSetupOpen = false;
		}
	}
}
