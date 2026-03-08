using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cavrnus.Base.Core;
using Cavrnus.Base.Settings;
using Cavrnus.Comm;
using Cavrnus.Comm.Comm;
using Cavrnus.Comm.Comm.NotifyApi;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using UnityEngine;

[assembly: InternalsVisibleTo("Tests")]

namespace Cavrnus.SpatialConnector.Core
{
	internal static class CavrnusAuthHelpers
    {
		internal static async Task<CavrnusAuthentication> TryAuthenticateWithToken(string server, string token)
		{
			var endpointnoauth = ResolvedServer(server);
			
			RestApiEndpoint endpoint = endpointnoauth.WithAuthorization(token);
			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			
			try
			{
				await ruc.GetUserProfileAsync();
			}
			catch (ErrorInfo e)
			{
				if (e.status == 401)
				{
					if (CavrnusSpatialConnector.Instance.SaveUserToken)
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
			
					if (CavrnusSpatialConnector.Instance.SaveGuestToken)
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");

					return null;
				}

				return null;
			}

			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(ruc, endpoint, token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate((INotifyDataUser lu) => lu != null));

			NotifySetup();
			HandleAuth(CavrnusStatics.CurrentAuthentication);
			return CavrnusStatics.CurrentAuthentication;
		}

		internal static async void Authenticate(string server, string email, string mfatoken, string password, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			var endpoint = ResolvedServer(server);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.LoginRequest req = new RestUserCommunication.LoginRequest {
				email = email, 
				password = password,
				mfaToken = mfatoken
			};

			TokenResult token = null;
			try
			{
				token = await ruc.PostLocalAccountLoginAsync(req);
			}
			catch (NetworkRequestException e)
			{
				if (e.ToString().Contains("NameResolutionFailure"))
				{
					if (CavrnusSpatialConnector.Instance.SaveUserToken)
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
					if (CavrnusSpatialConnector.Instance.SaveGuestToken)
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");

					Debug.LogError($"Invalid Server: {server} [{endpoint.ToUserDomainString()}]");
				}
				else
				{
					Debug.LogError(e.ToString());
				}

				return;
			}
			catch (RestUserCommunication.CavrnusAuthMfaTokenRequiredException mfareq)
			{
				Debug.LogWarning(mfareq.Message);
				onFailure($"MFATokenRequired: {mfareq.MfaMethod}");
			}
			catch (RestUserCommunication.CavrnusAuthMfaSetupRequiredException mfsreq)
			{
				Debug.LogWarning(mfsreq.Message);
				onFailure($"MFASetupRequired: {mfsreq.ExtraInfo}");
			}
			catch (ErrorInfo clientError)
			{
				if (clientError.status == 401)
				{
					//Invalid Token
					if (CavrnusSpatialConnector.Instance.SaveUserToken)
						PlayerPrefs.SetString("MemberCavrnusAuthToken", "");
					if (CavrnusSpatialConnector.Instance.SaveGuestToken)
						PlayerPrefs.SetString("GuestCavrnusAuthToken", "");
				}
				Debug.LogError(clientError.ToString());
				return;
			}

			//DebugOutput.Info("Logged in as User, token: " + token.token);

			var tokenEndpoint = endpoint.WithAuthorization(token.token);
			var tokenRuc = new RestUserCommunication(tokenEndpoint, new FrameworkNetworkRequestImplementation());
			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(tokenRuc, tokenEndpoint, token.token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate(lu => lu != null));

			NotifySetup();
			HandleAuth(CavrnusStatics.CurrentAuthentication);
			onSuccess(CavrnusStatics.CurrentAuthentication);
		}

		internal static async void AuthenticateAsGuest(string server, string userName, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			var endpoint = ResolvedServer(server);

			RestUserCommunication ruc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			RestUserCommunication.GuestRegistrationRequest req = new RestUserCommunication.GuestRegistrationRequest();
			req.screenName = userName;
			req.expires = DateTime.UtcNow.AddDays(7);

			string token = "";
			try
			{
				var tokenRes = await ruc.PostGuestRegistrationAsync(req);
				token = tokenRes.token;
			}
			catch (NetworkRequestException e)
			{
				if (e.ToString().Contains("NameResolutionFailure"))
				{
					Debug.LogError($"Invalid Server: {server} [{endpoint.ToUserDomainString()}]");
				}
				else
				{
					Debug.LogError(e.ToString());
				}
				return;
			}
			catch (ErrorInfo clientError)
			{
				Debug.LogError(clientError.ToString());
				return;
			}


			//DebugOutput.Info("Logged in as Guest, token: " + token);
			
			var tokenEndpoint = endpoint.WithAuthorization(token);
			var tokenRuc = new RestUserCommunication(tokenEndpoint, new FrameworkNetworkRequestImplementation());
			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(tokenRuc, tokenEndpoint, token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate(lu => {
				if (lu != null) {
					
					return true;
				}

				return false;
			}));
			
			NotifySetup();
			HandleAuth(CavrnusStatics.CurrentAuthentication);
			onSuccess(CavrnusStatics.CurrentAuthentication);
		}

		internal static async void AuthenticateWithApiKey(string server, string accessKey, string accessToken, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			var endpoint = ResolvedServer(server);
			var removedCustomerDomainForapikeyuse = endpoint.WithCustomerSubdomain(null);

			RestUserCommunication ruc = new RestUserCommunication(removedCustomerDomainForapikeyuse, new FrameworkNetworkRequestImplementation());

			string token = "";

			try
			{
				var loginresult = await ruc.PostApiKeyLoginAsync(new RestUserCommunication.ApiKeyLoginRequest()
				{
					accessKey = accessKey,
					accessToken = accessToken
				});

				token = loginresult.token;
				endpoint = removedCustomerDomainForapikeyuse.WithCustomerSubdomain(loginresult.domain).WithAuthorization(loginresult.token);
			}
			catch (NetworkRequestException e)
			{
				if (e.ToString().Contains("NameResolutionFailure"))
				{
					Debug.LogError($"Invalid Server: {server} [{endpoint.ToUserDomainString()}]");
				}
				else
				{
					Debug.LogError(e.ToString());
				}
				return;
			}
			catch (ErrorInfo clientError)
			{
				Debug.LogError(clientError.ToString());
				return;
			}
			
			//DebugOutput.Info("Logged in with ApiKey, token: " + token);

			var tokenRuc = new RestUserCommunication(endpoint, new FrameworkNetworkRequestImplementation());
			CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(tokenRuc, endpoint, token);

			await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate(lu => {
				if (lu != null)
				{
					return true;
				}

				return false;
			}));

			NotifySetup();

			HandleAuth(CavrnusStatics.CurrentAuthentication);
			onSuccess(CavrnusStatics.CurrentAuthentication);
		}

		internal static async void BeginAuthenticateViaDeviceCode(string server, bool autoOpenBrowser, string activationMessage, Action<RestUserCommunication.ResponseAuthDeviceCode> onSuccess, Action<string> onFailure)
		{
			var endpoint = ResolvedServer(server);
			var removedCustomerDomainForapikeyuse = endpoint.WithCustomerSubdomain(null);

			RestUserCommunication ruc = new RestUserCommunication(removedCustomerDomainForapikeyuse, new FrameworkNetworkRequestImplementation());

			try
			{
				var devicecoderes = await ruc.PostRequestDeviceCode(new RestUserCommunication.RequestAuthDeviceCode()
				{
					source = Cavrnus.Comm.IntegrationInfo.Info.ClientId,
					customActivatedMessage = activationMessage,
					ttl = 15 * 60
				});

				if (autoOpenBrowser)
				{
					Application.OpenURL(devicecoderes.verificationUrl);
				}

				onSuccess(devicecoderes);
			}
			catch (NetworkRequestException e)
			{
				if (e.ToString().Contains("NameResolutionFailure"))
				{
					Debug.LogError($"Invalid Server: {server} [{endpoint.ToUserDomainString()}]");
					onFailure($"Invalid Server: {server} [{endpoint.ToUserDomainString()}]");
;				}
				else
				{
					Debug.LogError(e.ToString());
					onFailure(e.ToString());
				}
				return;
			}
			catch (ErrorInfo clientError)
			{
				Debug.LogError(clientError.ToString());
				onFailure(clientError.ToString());
				
				return;
			}

			//DebugOutput.Info("Logged in with ApiKey, token: " + token);
		}

		internal static async void AwaitConcludeAuthenticateViaDeviceCode(string server, string devicecode, string usercode, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			var endpoint = ResolvedServer(server);
			var removedCustomerDomainForapikeyuse = endpoint.WithCustomerSubdomain(null);

			RestUserCommunication ruc = new RestUserCommunication(removedCustomerDomainForapikeyuse, new FrameworkNetworkRequestImplementation());

			while (true)
			{
				try
				{
					var dcres = await ruc.PostDeviceCodeLoginAsync(devicecode, usercode, 15 * 60 * 1000);

					var tokenep = removedCustomerDomainForapikeyuse.WithCustomerSubdomain(dcres.domain).WithAuthorization(dcres.token);
					var tokenruc = new RestUserCommunication(tokenep);

					CavrnusStatics.CurrentAuthentication = new CavrnusAuthentication(tokenruc, tokenep, dcres.token);

					await Task.WhenAny(CavrnusStatics.Notify.UsersSystem.ConnectedUser.AwaitPredicate(lu =>
					{
						if (lu != null)
						{
							return true;
						}

						return false;
					}));

					NotifySetup();

					HandleAuth(CavrnusStatics.CurrentAuthentication);
					onSuccess(CavrnusStatics.CurrentAuthentication);

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

					Debug.LogWarning($"Failed to conclude authentication via device code: {ei}");
					onFailure(ei.Message);
					return;
				}

				catch (Exception e)
				{
					Debug.LogWarning($"Failed to conclude authentication via device code: {e}");
					onFailure(e.Message);
					return;
				}
			}
		}

		private static void NotifySetup()
		{
			CavrnusStatics.Notify.Initialize(CavrnusStatics.CurrentAuthentication.Endpoint, true);
			CavrnusStatics.Notify.ObjectsSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.PoliciesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.RolesSystem.StartListeningAll(null, err => DebugOutput.Error(err.ToString()));
			CavrnusStatics.Notify.UsersSystem.StartListening(null, err => DebugOutput.Error(err.ToString()));

			CavrnusStatics.ContentManager.SetEndpoint(CavrnusStatics.CurrentAuthentication.Endpoint);
		}

		private static void HandleAuth(CavrnusAuthentication auth)
		{
			if (CavrnusSpatialConnector.Instance.SaveUserToken)
			{
				PlayerPrefs.SetString("MemberCavrnusAuthToken", auth.Token);
			}
			
			if (CavrnusSpatialConnector.Instance.SaveGuestToken)
			{
				PlayerPrefs.SetString("GuestCavrnusAuthToken", auth.Token);
			}

			if(OnAuthActions.Count > 0)
			{
				foreach(var action in OnAuthActions)
				{
					action?.Invoke(auth);
				}

				OnAuthActions.Clear();
			}
		}

		private static readonly List<Action<CavrnusAuthentication>> OnAuthActions = new List<Action<CavrnusAuthentication>>();

		internal static void AwaitAuthentication(Action<CavrnusAuthentication> onAuth)
		{
			if(CavrnusStatics.CurrentAuthentication != null)
			{
				onAuth(CavrnusStatics.CurrentAuthentication);
			}
			else
			{
				OnAuthActions.Add(onAuth);
			}
		}

		private static RestApiEndpoint ResolvedServer(string server)
		{
			server = server.Trim();
			server = server.TrimEnd('/');

			if (string.IsNullOrWhiteSpace(server) || server == "cavrn.us" || server == "api.cavrn.us")
				return new RestApiEndpoint("api.cavrn.us", null, null);

			if (server == "dev.cavrn.us" || server == "api.dev.cavrn.us")
				return new RestApiEndpoint("api.dev.cavrn.us", null, null);
			else if (server == "stage.cavrn.us" || server == "api.stage.cavrn.us")
				return new RestApiEndpoint("api.stage.cavrn.us", null, null);
			else if (!server.Contains(".") && !server.StartsWith("http"))
				return new RestApiEndpoint("api.cavrn.us", server, null);
			else
				return RestApiEndpoint.ParseFromHostname(server);
		}
	}
}