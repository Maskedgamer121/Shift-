using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cavrnus.Base.Core;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.Comm.Comm.LocalTypes;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.Comm.Prop.StringProp;
using Cavrnus.LiveRoomSystem.LiveObjectManagement.ObjectTypeManagers;
using Cavrnus.LiveRoomSystem.Views;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.Setup;
using Newtonsoft.Json.Linq;
using Cavrnus.EngineConnector;
using Cavrnus.SpatialConnector.Core.HistoryBuilder;
using UnityEngine;
using StringEditingMetadata = Cavrnus.Comm.Prop.StringProp.StringEditingMetadata;
using StringPropertyMetadata = Cavrnus.Comm.Prop.StringProp.StringPropertyMetadata;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// CavrnusFunctionLibrary Provides the main entry points into the Cavrnus system.
	/// </summary>
	public static class CavrnusFunctionLibrary
	{
        /// <summary>
		/// Sets up all static helpers and systems required for Cavrnus to run.
		/// This function will throw if called multiple times without being Shutdown in between.
		/// </summary>
        public static void InitializeCavrnus()
        {
	        CavrnusStatics.Setup(CavrnusSpatialConnector.Instance.AdditionalSettings);
        }

		/// <summary>
		/// Stops all static Cavrnus systems to support gracefully shutting down your application.
		/// </summary>
        public static void ShutdownCavrnus()
		{
			CavrnusStatics.Shutdown();
		}

		#region Authentication

		/// <summary>
		/// Returns true if you are logged in.
		/// </summary>
		public static bool IsLoggedIn()
		{
			return CavrnusStatics.CurrentAuthentication != null;
		}

		/// <summary>
		/// Logs in to your Cavrnus server. The authentication token will be provided to the onSuccess callback, and will also be retained internally for use in subsequent calls.
		/// Failures will result in calls to the onFailure callback.
		/// </summary>
		/// <param name="server">Your cavrnus server address, such as 'mydomain.cavrn.us'.</param>
		/// <param name="email">Your cavrnus user username, which is usually but not necessarily an email.</param>
		/// <param name="password">Your password.</param>
		/// <param name="onSuccess">A callback fired when authentication is successful.</param>
		/// <param name="onFailure">A callback fired if there are problems with login. Aside from general authorization and network errors this could also be a string that begins with
		/// 'MFATokenRequired', implying the need for the user to enter an extra code to log in, or 'MFASetupRequired', indicating that the account cannot be logged into until setting up
		/// a multi-factor authentication method. The URL to navigate to to setup this code will be in the remainder of the string.</param>
		public static void AuthenticateWithPassword(string server, string email, string password, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			CavrnusAuthHelpers.Authenticate(server.Trim(), email.Trim(), null, password, onSuccess, onFailure);
		}

		/// <summary>
		/// Logs in to your Cavrnus server. The authentication token will be provided to the onSuccess callback, and will also be retained internally for use in subsequent calls.
		/// Failures will result in calls to the onFailure callback.
		/// </summary>
		/// <param name="server">Your cavrnus server address, such as 'mydomain.cavrn.us'.</param>
		/// <param name="email">Your cavrnus user username, which is usually but not necessarily an email.</param>
		/// <param name="password">Your password.</param>
		/// <param name="mfatoken">Your multi-factor authorization token. To figure out if this parameter is required, use AuthenticateWithPassword without this argument, and if a code
		/// is required it will fail with an error 'MFATokenRequired...'</param>
		/// <param name="onSuccess">A callback fired when authentication is successful.</param>
		/// <param name="onFailure">A callback fired if there are problems with login. Aside from general authorization and network errors this could also be a string that begins with
		/// 'MFATokenRequired', implying the need for the user to enter an extra code to log in, or 'MFASetupRequired', indicating that the account cannot be logged into until setting up
		/// a multi-factor authentication method. The URL to navigate to to setup this code will be in the remainder of the string.</param>
		public static void AuthenticateWithPasswordAndMfaToken(string server, string email, string password, string mfatoken, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			CavrnusAuthHelpers.Authenticate(server.Trim(), email.Trim(), mfatoken.Trim(), password, onSuccess, onFailure);
		}

		/// <summary>
		/// Creates a guest user account with a given name and joins as that user
		/// </summary>
		/// <param name="server">Your cavrnus server address, such as 'mydomain.cavrn.us'.</param>
		/// <param name="userName"></param>
		/// <param name="onSuccess">A callback fired when authentication is successful.</param>
		/// <param name="onFailure">A callback fired if there are problems with login.</param>
		public static void AuthenticateAsGuest(string server, string userName, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			CavrnusAuthHelpers.AuthenticateAsGuest(server.Trim(), userName.Trim(), onSuccess, onFailure);
		}

		/// <summary>
		/// Logs in to an existing account with an apikey and token previously generated.
		/// </summary>
		/// <param name="server">Optionally, your cavrnus server address, such as 'mydomain.cavrn.us'. Apikey logins do not require knowing the domain ahead of time; an empty server will target Cavrnus cloud servers.</param>
		/// <param name="accessKey">The previously generated accessKey.</param>
		/// <param name="accessToken">The matching accessToken that goes along with the given key.</param>
		/// <param name="onSuccess">A callback fired when authentication is successful.</param>
		/// <param name="onFailure">A callback fired if there are problems with login.</param>
		public static void AuthenticateWithApiKey(string server, string accessKey, string accessToken, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			CavrnusAuthHelpers.AuthenticateWithApiKey(server.Trim(), accessKey, accessToken, onSuccess, onFailure);
		}

		/// <summary>
		/// Logs in to an existing account using a device code, obtained by launching a browser window, which authorizes the application's request for a login.
		///
		/// This call needs to be followed by a call to ConcludeAuthenticationViaDeviceCode, which will not succeed or fail until either the user's code is authenticated, or the request times-out.
		/// </summary>
		/// <param name="server">Optionally, your cavrnus server address, such as 'mydomain.cavrn.us'. Devicecode logins do not require knowing the domain ahead of time; an empty server will target Cavrnus cloud servers.</param>
		/// <param name="autoOpenBrowser">If true, let this call execute Application.OpenURL to launch the browser. Otherwise you're doing it yourself, or using another method to authenticate the generated code.</param>
		/// <param name="activationMessage">If provided (not empty), this message will be presented to the user in their browser after it has completed authenticating. We recommend something like 'Authorization complete, you can return to 'your app here'.', or something the like.</param>
		/// <param name="onBegun">A callback fired when the device code auth request is complete and ready to be fulfilled.</param>
		/// <param name="onFailure">A callback fired if there are problems with login.</param>
		public static void BeginAuthenticationViaDeviceCode(string server, bool autoOpenBrowser, string activationMessage, Action<RestUserCommunication.ResponseAuthDeviceCode> onBegun, Action<string> onFailure)
		{
			CavrnusAuthHelpers.BeginAuthenticateViaDeviceCode(server, autoOpenBrowser, activationMessage, onBegun, onFailure);
		}

		/// <summary>
		/// Awaits authorization of a previously generated device code, and logs in the user with it when it is able to do so. See BeginAuthenticationViaDeviceCode.
		/// Success or failure will not occur until the user authorizes the code (using their browser or alternate device), or the request times out, which by default is 15 minutes.
		/// </summary>
		/// <param name="server">Optionally, your cavrnus server address, such as 'mydomain.cavrn.us'. Devicecode logins do not require knowing the domain ahead of time; an empty server will target Cavrnus cloud servers.</param>
		/// <param name="devicecode">The device code previously created, about which we are awaiting authentication. The device code is 'private' and should not be shown to the user; it uniquely identifies this request.</param>
		/// <param name="usercode">The user code previously created, about which we are awaiting authentication. The user code is the 'public' code that a user can manually enter on an alternate device if needed.</param>
		/// <param name="onSuccess">A callback fired when the client is authenticated and logged in.</param>
		/// <param name="onFailure">A callback fired if there are problems with login.</param>
		public static void ConcludeAuthenticationViaDeviceCode(string server, string devicecode, string usercode, Action<CavrnusAuthentication> onSuccess, Action<string> onFailure)
		{
			CavrnusAuthHelpers.AwaitConcludeAuthenticateViaDeviceCode(server, devicecode, usercode, onSuccess, onFailure);
		}


		/// <summary>
		/// Sets up a callback to be invoked whenever the Cavrnus System has been authenticated. This will happen as a consequence of a different Authenticate* function call.
		/// Use this function when you want some code to run after logging in, but you don't care how that login has happened.
		/// </summary>
		/// <param name="onAuth"></param>
		public static void AwaitAuthentication(Action<CavrnusAuthentication> onAuth)
		{
			CavrnusAuthHelpers.AwaitAuthentication(onAuth);
		}

		#endregion

		#region Spaces

		/// <summary>
		/// Given a live connection, calls the provided callback with information about that space both immediately, and whenever it changes in the future.
		/// This binding can be stopped by disposing of the returned handle.
		/// </summary>
		/// <param name="spaceConn">The space about which you wish to be informed.</param>
		/// <param name="onSpaceInfoUpdated">The callback invoked with space information.</param>
		/// <returns></returns>
		public static IDisposable BindSpaceInfo(this CavrnusSpaceConnection spaceConn, Action<CavrnusSpaceInfo> onSpaceInfoUpdated)
		{
			return spaceConn.BindSpaceInfo(onSpaceInfoUpdated);
		}
		
		/// <summary>
		/// Queries the server to get a list of all current spaces which can be joined.
		/// This function will only be called once and not after the initial query, as it is 'Fetch' and not 'Bind'.
		/// </summary>
		/// <param name="onRecvCurrentJoinableSpaces">Callback which receives the spaces info list.</param>
		public static void FetchJoinableSpaces(Action<List<CavrnusSpaceInfo>> onRecvCurrentJoinableSpaces)
		{
			CavrnusSpaceHelpers.GetCurrentlyAvailableSpaces(onRecvCurrentJoinableSpaces);
		}

		/// <summary>
		/// Queries the server to get a list of all current spaces which can be joined. Changes to this list will continue to be invoked until the yielded handle is Disposed.
		/// A large number of callbacks will occur as the initial list is passed through the provided spaceAdded function.
		/// </summary>
		/// <param name="spaceAdded">Callback invoked when a new available space is found.</param>
		/// <param name="spaceRemoved">Callback invoked when a space is no longer available to be joined.</param>
        public static IDisposable BindJoinableSpaces(Action<CavrnusSpaceInfo> spaceAdded, Action<CavrnusSpaceInfo> spaceRemoved)
		{
			return CavrnusSpaceHelpers.BindAllAvailableSpaces(spaceAdded, spaceRemoved);
		}

        /// <summary>
		/// Checks if there is any active connection to a space
		/// </summary>
		/// <returns>True if there are any active space connections</returns>
        public static bool IsConnectedToAnySpace()
		{
			return CavrnusSpaceConnectionManager.TaggedConnections.Count > 0;
		}

		/// <summary>
		/// Creates a new Space on the server with the provided name. The space will be empty and its journal will have no contents.
		/// </summary>
		/// <param name="spaceName">The name for the new space. Space names do not need to be unique.</param>
		/// <param name="onCreationComplete">Callback invoked when the space has been created and may be joined.</param>
		/// <param name="onFailure">Callback invoked if there is any error, for example, you may not be logged in, or have permissions to create spaces.</param>
		public static void CreateSpace(string spaceName, Action<CavrnusSpaceInfo> onCreationComplete, Action<string> onFailure)
		{
			CavrnusSpaceHelpers.CreateSpace(spaceName.Trim(), onCreationComplete);
		}

		/// <summary>
		/// Joins a live space. This results in the spaces journal being processed, voice and video systems become connected to the space, connected user information,
		/// and live property information being made available and updatable.
		/// </summary>
		/// <param name="joinId">The identifier of the space to join. Typically comes from <see cref="FetchJoinableSpaces"></see> or <see cref="BindJoinableSpaces"></see></param>
		/// <param name="onConnected">Callback invoked when the space has been fully joined. By the time this function is called the space's journal will be processed. Property information will be current and synched up with the space.
		/// The provided parameter of type 'CavrnusSpaceConnection' is used in many other entry points to utilize the live space.</param>
		/// <param name="onFailure">Callback invoked when there is an error.</param>
		/// <param name="config">An optional configuration object. This configuration lets you specify if RTC (voice and video) systems are enabled for this space connection. They also let you supply a tag to associate this space join with.
		/// The tag is local to this machine and not synchronized with the server. Tags are unique to joins; if you specify the same tag as a prior join, the prior joined space will be shut down before connecting to the new one. Space tags here
		/// are meant to be used to distinguish the purpose for joining spaces; for example you may have one space for shared communications between users, and one that represents a virtual room they are in. They are only in one room at a time
		/// so they use the same tag, but the communications room would use a different tag, so that it would stay connected in parallel.</param>
		public static void JoinSpace(string joinId, Action<CavrnusSpaceConnection> onConnected, Action<string> onFailure, CavrnusSpaceConnectionConfig config = null)
		{
			CavrnusSpaceHelpers.JoinSpace(joinId.Trim(), CavrnusSpatialConnector.Instance.SpawnableObjects, onConnected, onFailure, config);
		}

		/// <summary>
		/// Joins a live space. This results in the spaces journal being processed, voice and video systems become connected to the space, connected user information,
		/// and live property information being made available and updatable.
		/// </summary>
		/// <param name="joinId">The identifier of the space to join. Typically comes from <see cref="FetchJoinableSpaces"></see> or <see cref="BindJoinableSpaces"></see></param>
		/// <param name="config">A configuration object. This configuration lets you specify if RTC (voice and video) systems are enabled for this space connection. They also let you supply a tag to associate this space join with.
		/// The tag is local to this machine and not synchronized with the server. Tags are unique to joins; if you specify the same tag as a prior join, the prior joined space will be shut down before connecting to the new one. Space tags here
		/// are meant to be used to distinguish the purpose for joining spaces; for example you may have one space for shared communications between users, and one that represents a virtual room they are in. They are only in one room at a time
		/// so they use the same tag, but the communications room would use a different tag, so that it would stay connected in parallel. The default tag is "", so by default joining a space leaves any prior connected space.</param>
		/// <param name="onConnected">Callback invoked when the space has been fully joined. By the time this function is called the space's journal will be processed. Property information will be current and synched up with the space.
		/// The provided parameter of type 'CavrnusSpaceConnection' is used in many other entry points to utilize the live space.</param>
		/// <param name="onFailure">Callback invoked when there is an error.</param>
		public static void JoinSpaceWithOptions(string joinId, CavrnusSpaceConnectionConfig config, Action<CavrnusSpaceConnection> onConnected, Action<string> onFailure)
		{
			JoinSpace(joinId.Trim(), onConnected, onFailure, config);
		}
		
        /// <summary>
		/// Invokes the provided callback when you begin attempting to join a space, returning the ID of the space being joined.
		/// This is useful if you wish to separate your code that handles content from your code that orchestrates logging in and joining a space.
		/// </summary>
		/// <param name="onBeginLoading">Callback invoked when a space begins being connected.</param>
		/// <param name="tag">See config description in <see cref="JoinSpaceWithOptions"></see>. Tags identify joined spaces."</param>
        public static void AwaitAnySpaceBeginLoading(Action<string> onBeginLoading, string tag = null)
        {
            CavrnusSpaceHelpers.AwaitAnySpaceBeginLoading(onBeginLoading, tag);
        }

		/// <summary>
		/// Invokes the provided callback when you begin attempting to join a space, where the space's join tag matches the provided parameter, returning the ID of the space being joined.
		/// This is useful if you wish to separate your code that handles content from your code that orchestrates logging in and joining a space.
		/// </summary>
		/// <param name="tag">See config description in <see cref="JoinSpaceWithOptions"></see>. Tags identify joined spaces."</param>
		/// <param name="onBeginLoading">Callback invoked when a space begins being connected.</param>
        public static void AwaitSpaceBeginLoadingByTag(string tag, Action<string> onBeginLoading)
        {
	        AwaitAnySpaceBeginLoading(onBeginLoading, tag);
        }

		/// <summary>
		/// Invokes the provided callback immediately if you are already in a space (with matching tag), otherwise triggers as soon as you connect to a space.
		/// This callback occurs when the connection is fully established, which is later than the yield from <see cref="AwaitAnySpaceBeginLoading"></see>. This call will occur after the space's journal has been fully processed
		/// and the connection is ready to be fully utilized.
		/// </summary>
		/// <param name="onConnected">Callback invoked when a space (matching the tag) has been fully connected.</param>
		/// <param name="tag">See config description in <see cref="JoinSpaceWithOptions"></see>. Tags identify joined spaces. Optional."</param>
        public static void AwaitAnySpaceConnection(Action<CavrnusSpaceConnection> onConnected, string tag = null)
        {
	        CavrnusSpaceHelpers.AwaitAnySpaceConnection(onConnected, tag);
        }

		/// <summary>
		/// Invokes the provided callback immediately if you are already in a space (with matching tag), otherwise triggers as soon as you connect to a space.
		/// This callback occurs when the connection is fully established, which is later than the yield from <see cref="AwaitAnySpaceBeginLoading"></see>. This call will occur after the space's journal has been fully processed
		/// and the connection is ready to be fully utilized.
		/// </summary>
		/// <param name="tag">See config description in <see cref="JoinSpaceWithOptions"></see>. Tags identify joined spaces."</param>
		/// <param name="onConnected">Callback invoked when a space (matching the tag) has been fully connected.</param>
        public static void AwaitSpaceConnectionByTag(string tag, Action<CavrnusSpaceConnection> onConnected)
        {
	        AwaitAnySpaceConnection(onConnected, tag);
        }
		
		/// <summary>
		/// Disconnects you from the provided space. You will stop receiving property updates, drop voice and video connections and lose connected user information.
		/// </summary>
		/// <param name="spaceConn">The space to disconnect from.</param>
		public static void ExitSpace(this CavrnusSpaceConnection spaceConn)
		{
			CavrnusSpaceConnectionManager.ExitSpace(spaceConn);
		}

        #endregion

        #region Properties

        // ============================================
        // Color Property Functions
        // ============================================

        /// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineColorPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Color propertyValue)
		{
			CavrnusPropertyHelpers.DefineColorPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
		public static Color GetColorPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetColorPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindColorPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<Color> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToColorProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindColorPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<Color> onPropertyUpdated)
		{
			return containerName.SubBind((c) => BindColorPropertyValue(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<Color> BeginTransientColorPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Color propertyValue)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
		public static void PostColorPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Color propertyValue)
		{
			CavrnusPropertyHelpers.UpdateColorProperty(spaceConn, containerName, propertyName, propertyValue);
		}

		// ============================================
		// Float Property Functions
		// ============================================

		/// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineFloatPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, float propertyValue)
		{
			CavrnusPropertyHelpers.DefineFloatPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
        public static float GetFloatPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetFloatPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindFloatPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<float> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToFloatProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindFloatPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<float> onPropertyUpdated)
		{
			return containerName.SubBind(c=>CavrnusPropertyHelpers.BindToFloatProperty(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<float> BeginTransientFloatPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, float propertyValue)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
        public static void PostFloatPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, float propertyValue)
		{
			CavrnusPropertyHelpers.UpdateFloatProperty(spaceConn, containerName, propertyName, propertyValue);
		}

		// ============================================
		// Bool Property Functions
		// ============================================

		/// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineBoolPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, bool propertyValue)
		{
			CavrnusPropertyHelpers.DefineBooleanPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
        public static bool GetBoolPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetBooleanPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindBoolPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<bool> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToBooleanProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindBoolPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<bool> onPropertyUpdated)
		{
			return containerName.SubBind(c=>CavrnusPropertyHelpers.BindToBooleanProperty(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<bool> BeginTransientBoolPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, bool propertyValue)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
        public static void PostBoolPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, bool propertyValue)
		{
			CavrnusPropertyHelpers.UpdateBooleanProperty(spaceConn, containerName, propertyName, propertyValue);
		}

		// ============================================
		// String Property Functions
		// ============================================

		/// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineStringPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, string propertyValue)
		{
			CavrnusPropertyHelpers.DefineStringPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Defines the metadata and default value of the property /containerName/propertyName.
		/// The metadata is not used directly but can be a source of information to build user interfaces or to guide external integrations. The default value becomes the value of the property when there are otherwise no assigned values to override it.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="displayName">A friendly name designed to be read by users, rather than code.</param>
		/// <param name="description">A friendly description of the property's meaning</param>
		/// <param name="readOnly">A flag that when set should limit the ability to change this property's value.</param>
		/// <param name="enumOptions">A set of values this property should be permitted to be. When present this string property should be interpreted as an enumeration rather than a generic string.</param>
		public static void DefineStringPropertyDefinition(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, string displayName, string description, bool readOnly = false, List<StringEditingEnumerationOption> enumOptions = null)
        {
	        var definition = new StringPropertyMetadata {
		        Name = displayName, 
		        Description = description,
		        Readonly = readOnly,
		        Edit = new StringEditingMetadata
		        {
			        EnumerationOptions = enumOptions
		        }
	        };

	        CavrnusPropertyHelpers.DefineStringPropertyDefinition(spaceConn, containerName, propertyName, definition);
        }

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
        public static string GetStringPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetStringPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindStringPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<string> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToStringProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindStringPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<string> onPropertyUpdated)
		{
			return containerName.SubBind(c=>CavrnusPropertyHelpers.BindToStringProperty(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<string> BeginTransientStringPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, string propertyValue)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
        public static void PostStringPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, string propertyValue)
		{
			CavrnusPropertyHelpers.UpdateStringProperty(spaceConn, containerName, propertyName, propertyValue);
		}

		// ============================================
		// Vector Property Functions
		// ============================================

		/// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineVectorPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Vector4 propertyValue)
		{
			CavrnusPropertyHelpers.DefineVectorPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
        public static Vector4 GetVectorPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetVectorPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindVectorPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<Vector4> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToVectorProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindVectorPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<Vector4> onPropertyUpdated)
		{
			return containerName.SubBind(c=>CavrnusPropertyHelpers.BindToVectorProperty(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<Vector4> BeginTransientVectorPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Vector4 propertyValue)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
        public static void PostVectorPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Vector4 propertyValue)
		{
			CavrnusPropertyHelpers.UpdateVectorProperty(spaceConn, containerName, propertyName, propertyValue);
		}

		// ============================================
		// Transform Property Functions
		// ============================================

		/// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineTransformPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, CavrnusTransformData propertyValue)
		{
			CavrnusPropertyHelpers.DefineTransformPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue.Position, propertyValue.EulerAngles, propertyValue.Scale);
		}

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
        public static CavrnusTransformData GetTransformPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetTransformPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindTransformPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<CavrnusTransformData> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToTransformProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindTransformPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<CavrnusTransformData> onPropertyUpdated)
		{
			return containerName.SubBind(c=>CavrnusPropertyHelpers.BindToTransformProperty(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<CavrnusTransformData> BeginTransientTransformPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, CavrnusTransformData propertyValue, PropertyPostOptions options = null)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue, options);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
        public static void PostTransformPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, CavrnusTransformData propertyValue, PropertyPostOptions options = null)
		{
			CavrnusPropertyHelpers.UpdateTransformProperty(spaceConn, containerName, propertyName, propertyValue.Position, propertyValue.EulerAngles, propertyValue.Scale, options);
		}

		// ============================================
		// JSON Property Functions
		// ============================================

		/// <summary>
		/// Defines the value of the property /containerName/propertyName when it has no assignments.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The property value to be made the default value of this property</param>
        public static void DefineJsonPropertyDefaultValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, JObject propertyValue)
		{
			CavrnusPropertyHelpers.DefineJsonPropertyDefaultValue(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Gets the current value of the property /containerName/propertyName, whether the default or a value assigned through the space journal.
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <returns></returns>
        public static JObject GetJsonPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
		{
			return CavrnusPropertyHelpers.GetJsonPropertyValue(spaceConn, containerName, propertyName);
		}

		/// <summary>
		/// Binds the value of the property /containerName/propertyName, invoking the provided callback with the property's value immediately, and every time thereafter when the value changes.
		/// In the case of animated properties this may be quite frequent.
		/// Unbinding the provided function can be done by Dispose-ing the returned handle.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="onPropertyUpdated">Callback invoked with the value of the property, immediately and whenever it changes.</param>
		/// <returns>An IDisposable handle that can be disposed to remove the callback binding.</returns>
		public static IDisposable BindJsonPropertyValue(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Action<JObject> onPropertyUpdated)
		{
			return CavrnusPropertyHelpers.BindToJsonProperty(spaceConn, containerName, propertyName, onPropertyUpdated);
		}

		//Triggers an event when the property changes, plus an initial event when first bound.
		/*internal static IDisposable BindJsonPropertyValue(this CavrnusSpaceConnection spaceConn, IReadonlySetting<string> containerName, string propertyName, Action<JObject> onPropertyUpdated)
		{
			return containerName.SubBind(c=>CavrnusPropertyHelpers.BindToJsonProperty(spaceConn, c, propertyName, onPropertyUpdated));
		}*/

		/// <summary>
		/// Begins a live property update for the property /containerName/propertyName. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
		/// Live property updates can be updated with UpdateWithNewData().
		/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
		/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
		/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
		/// as they know the user has been disconnected.
		/// 
		/// If the property is not of the requested type, then the current value of the property will be obliterated and the property will become the type requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The initial value to begin the live property update with</param>
		/// <returns>A CavrnusLivePropertyUpdate instance which can be used to update or conclude the live property update.</returns>
		public static CavrnusLivePropertyUpdate<JObject> BeginTransientJsonPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, JObject propertyValue)
		{
			return CavrnusPropertyHelpers.BeginContinuousPropertyUpdate(spaceConn, containerName, propertyName, propertyValue);
		}

		/// <summary>
		/// Updates the property /containerName/propertyName to the value provided, immediately. This change will be stored in the permanent space journal and transmitted to all live users.
		/// 
		/// If the property is not of the requested type, then the prior value of the property will be obliterated and the property will become the type (and value) requested.
		/// If however the property was 'Define'd, it will then be locked into its defined type and this will throw a PropertyTypeMismatchException.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="propertyValue">The value to assign to the property</param>
        public static void PostJsonPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, JObject propertyValue)
		{
			CavrnusPropertyHelpers.UpdateJsonProperty(spaceConn, containerName, propertyName, propertyValue);
		}

        #endregion

        #region Permissions
		// ============================================
        // Permissions Functions
        // ============================================

        /// <summary>
		/// Binds a callback function to be provided whether or not a given action is permitted by the authenticated user. This relates to roles and policies configured using the Cavrnus web console.
		/// The callback will be fired immediately with the current status, but note that depending on when this function is called, role and policy information may not yet be obtained. In this case the function
		/// will be called immediately with the value 'false', but will be called again when policy information is obtained, assuming that policy changes the answer to 'true'.
		/// </summary>
		/// <param name="policy">The policy action being queried, such as 'api:objects:upload'.</param>
		/// <param name="onValueChanged">the callback to be invoked with action permissibility status</param>
		/// <returns></returns>& resolved)
        public static IDisposable BindGlobalPolicy(string policy, Action<bool> onValueChanged)
		{
			return RoleAndPermissionHelpers.EvaluateGlobalPolicy(policy, onValueChanged);
		}

		/// <summary>
		/// Binds a callback function to be provided whether or not a given action is permitted by the authenticated user within the provided space. Memberships to spaces include a policy which may limit or augment
		/// the user's role's capabilities.
		/// The callback will be fired immediately with the current status, but note that depending on when this function is called, role and policy information may not yet be obtained. In this case the function
		/// will be called immediately with the value 'false', but will be called again when policy information is obtained, assuming that policy changes the answer to 'true'.
		/// </summary>
		/// <param name="policy">The policy action being queried, such as 'api:objects:upload'.</param>
		/// <param name="onValueChanged">the callback to be invoked with action permissibility status</param>
		/// <returns></returns>& resolved)
        public static IDisposable BindSpacePolicy(this CavrnusSpaceConnection conn, string policy, Action<bool> onValueChanged)
		{
			return RoleAndPermissionHelpers.EvaluateSpacePolicy(policy, conn, onValueChanged);
		}

		#endregion

		#region SpawnedObjects

		// ============================================
		// Spawned Objects
		// ============================================

		/// <summary>
		/// Adds an object creation to the journal with the given application-specific identifier. The creation uses the 'WellKnownId' creation operation.
		/// A property path will be generated randomly for the object. No property values will be assigned. You can access the property path by inspecting the yielded CavrnusSpawnedObject's 'PropertiesContainerName', for purposes
		/// of reading or writing properties contextual to this object.
		/// As this creation is sent to the journal, other connected and future users will receive an object creation as well.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="uniqueIdentifier">An identifier the application may use to identify which object to create. The Cavrnus system does not interact with this value other than to provide it to the clients.</param>
		/// <param name="onObjectCreated">A callback invoked when the object has been fully created in the journal. This is asynchronous because of the need to wait for the server to reply with an identifier for the object creation.</param>
		/// <returns>The property path identifying the new object. Use this to set properties before waiting for the callback.</returns>
		public static string SpawnObject(this CavrnusSpaceConnection spaceConn, string uniqueIdentifier, Action<CavrnusSpawnedObject, GameObject> onObjectCreated = null)
		{
			var newId = spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.CreateNewUniqueObjectId();
			var creatorId = spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.LocalCommUser.Value.ConnectionId;
			var contentType = new ContentTypeWellKnownId(uniqueIdentifier);

			var createOp = new OpCreateObjectLive(null, PropertyId.FromAbsoluteStack(newId), creatorId, contentType).ToOp();

			if(onObjectCreated != null)
			{
				spaceConn.CurrentSpaceConnection.Value.CreationHandler.SpawnCallbacks.Add(newId, onObjectCreated);
			}

			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(createOp, null);

			return newId;
		}

        /// <summary>
		/// Removes an objects from its' space's journal. This call submits a journal operation which cancels the creation operation for the provided object. Connected users will receive the removal.
		/// Any property assignments within the object's path will be left in the journal, but subsequent joins will not include those properties unless the object is restored.
		/// </summary>
		/// <param name="spawnedObject"></param>
        public static void DestroyObject(this CavrnusSpawnedObject spawnedObject)
		{
			OperationIdLive rootOpId = new OperationIdLive(spawnedObject.CreationOpId);

			var singles = new List<string> { rootOpId.Id };

			var deleteOp = new OpRemoveOpsLive(OpRemoveOpsLive.RemovalTypes.None) { OpsToRemove = singles };

            spawnedObject.spaceConnection.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(deleteOp.ToOp(), null);
		}

		#endregion

		#region Chats

		/// <summary>
		/// Sets up a callback to be invoked with all chat messages when they are added or removed. When this function is called the chatAdded callback will be immediately invoked with all extant messages.
		/// The caller can optionally include chats and transcriptions separately.
		/// Each message comes in the form of an <see cref="IChatViewModel">IChatViewModel</see>.
		/// The binding can be released by disposing of the returned IDisposable handle.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the messages.</param>
		/// <param name="chatAdded">Callback invoked for all extant messages, both immediately and newly created messages after the binding.</param>
		/// <param name="chatRemoved">Callback invoked when a message is deleted, or times out, as often happens in the case of transcriptions.</param>
		/// <param name="includeChats">Include chat-type messages.</param>
		/// <param name="includeTranscriptions">Include transcription-type messages.</param>
		/// <returns></returns>
		public static IDisposable BindChatMessages(this CavrnusSpaceConnection spaceConn, Action<IChatViewModel> chatAdded, Action<IChatViewModel> chatRemoved, bool includeChats = true, bool includeTranscriptions = true)
		{
			IDisposable internalBinding = null;

			var spaceBinding = spaceConn.CurrentSpaceConnection.Bind(scd => {
				internalBinding?.Dispose();
				internalBinding = null;

				if (scd == null)
					return;

				var csv = new ChatStreamView(scd.RoomSystem, new ChatStreamViewOptions { includeChats = includeChats, includeTranscriptions = includeTranscriptions });
				internalBinding = csv.Messages.BindAll(chatAdded, chatRemoved);
			});

			return new DelegatedDisposalHelper(() => {
				internalBinding?.Dispose();
				spaceBinding?.Dispose();
			});
		}

		/// <summary>
		/// Adds a chat message to the provided space. This message is not ephemeral all will be stored in the space's journal, and be present on rejoins at later dates.
		/// </summary>
		/// <param name="spaceConn">The connected space within which to add the chat message.</param>
		/// <param name="message">The text message to be stored.</param>
		public static void PostChatMessage(this CavrnusSpaceConnection spaceConn, string message)
		{
			var lu = spaceConn.CurrentLocalUserSetting.Value;
			var chat = new ContentTypeChatEntry(message, DateTimeCache.UtcNow, lu?.UserAccountId, ChatMessageSourceTypeEnum.Chat);
			var newId = spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.CreateNewUniqueObjectId();

			var op = spaceConn.CurrentSpaceConnection.Value.RoomSystem.LiveOpsSys.Create(new OpCreateObjectLive(null, PropertyDefs.ChatContainer.Push(newId), lu?.UserAccountId, chat));
			op.OpData.CreatorId = lu?.UserAccountId ?? "";
			op.OpData.ExecMode = Operation.Types.OperationExecutionModeEnum.Standard;
			op.PostAndComplete();
		}

		#endregion

		#region Space Users

		/// <summary>
		/// Invokes the provided callback when the provided space connection has fully connected and information about the connecting user (you!) is present.
		/// If the local user information is already obtained, the callback will be invoked immediately.
		/// 
		/// Why? Some functions require the user to be fully connected before being called, such as posting chat messages. This function lets you more easily wait until the space is in a valid state to call such a function.
		/// </summary>
		/// <param name="spaceConnection">The space connection which we are waiting to become fully connected.</param>
		/// <param name="localUserArrived">The callback invoked with the local user's information once it is available.</param>
		public static void AwaitLocalUser(this CavrnusSpaceConnection spaceConnection, Action<CavrnusUser> localUserArrived)
		{
			spaceConnection.AwaitLocalUser(localUserArrived);
		}
		
        /// <summary>
		/// Provides a list of current connected users in the provided space.
		/// </summary>
		/// <param name="spaceConn">The connected space to query for users.</param>
		/// <returns>The list of users as <see cref="CavrnusUser"></see> objects.</returns>
        public static List<CavrnusUser> GetCurrentSpaceUsers(this CavrnusSpaceConnection spaceConn)
        {
            List<CavrnusUser> res = new List<CavrnusUser>();
            foreach (var user in spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.ConnectedUsers)
            {
                res.Add(new CavrnusUser(user, spaceConn));
            }

            return res;
        }

		/// <summary>
		/// Bind callbacks to receive information about all connected users. The added function will be called for all present users both immediately and later when new users join.
		/// The removed callback will be invoked when the user disconnects.
		/// The binding can be stopped by disposing the returned IDisposable handle.
		/// </summary>
		/// <param name="spaceConn">The space whose users you wish to know about.</param>
		/// <param name="userAdded">Callback invoked when a user is present</param>
		/// <param name="userRemoved">Callback invoked when a user leaves</param>
		/// <returns>An IDisposable handle that can be disposed to stop the binding</returns>
        public static IDisposable BindSpaceUsers(this CavrnusSpaceConnection spaceConn, Action<CavrnusUser> userAdded, Action<CavrnusUser> userRemoved)
		{
			return CavrnusSpaceUserHelpers.BindSpaceUsers(spaceConn, userAdded, userRemoved);
		}

		/// <summary>
		/// Bind a callback to receive the user's video texture. The provided texture may have no texture included if the user is not streaming or presenting a camera.
		/// The structure provided also includes a UVRect, which should be used to understand the orientation of the provided texture. Some remote streams, for example, arrive upside down due to their sources.
		/// The texture callback will not be invoked every time the frame changes; only when the underlying texture object changes. For example, it will be called when the stream starts, or when it changes resolution, but not
		/// for every camera frame received.
		/// </summary>
		/// <param name="user">The connected user to receive video textures for.</param>
		/// <param name="userFrameArrived">the callback invoked when the user's video texture changes.</param>
		/// <returns></returns>
        public static IDisposable BindUserVideoFrames(this CavrnusUser user, Action<TextureWithUVs> userFrameArrived)
        {
            return user.VidProvider.SubBind(uvp=>uvp.providedTexture.Bind(frame =>
			{
                if (frame == null)
					return;

                userFrameArrived(frame);
			}));
        }

        #endregion

        #region Voice and Video
        
        /// <summary>
		/// This function sends a message to another user, requesting that they mute their audio input device. Applications generally implement
		/// the response by immediately muting, potentially also informing the user of such.
		/// 
		/// If this call is invoked on yourself, it will throw an ArgumentException. Instead call <see cref="SetLocalUserMutedState"/>.
		/// </summary>
		/// <param name="user">The remote user to request be muted.</param>
		/// <exception cref="ArgumentException">Thrown if called on the local user.</exception>
		public static void RequestRemoteUserMute(this CavrnusUser user)
        {
	        if (user.IsLocalUser) {
		        throw new ArgumentException("User is the local user. Please provide a remote user.");
	        }
	        else {
		        var transient = new TransientEvent {
			        UserMuteRequest = new EvRequestMuteUser {
				        V1 = new EvRequestMuteUser.Types.V1 {
					        Muted = true,
					        ConnectionId = user.ConnectionId
				        }
			        }
		        };
		        
		        user.SpaceConnection.CurrentSpaceConnection.Value.RoomSystem.Comm.SendTransientEvent(transient, false, false);
	        }
        }

        /// <summary>
		/// Mutes or unmutes the local user's audio input / microphone
		/// </summary>
		/// <param name="spaceConnection">The active space connection to mute yourself in.</param>
		/// <param name="muted">Set to true to mute, false to resume audio.</param>
        public static void SetLocalUserMutedState(this CavrnusSpaceConnection spaceConnection, bool muted)
        {
	        spaceConnection.SetLocalUserMuted(muted);//Value.RoomSystem.Comm.LocalCommUser.Value.Rtc.Muted.Value = muted;
		}

        /// <summary>
		/// Sets streaming state for local user. Other connected users in the space use this value to understand if the user is streaming a camera or a window/screen/application.
		/// Streamed cameras often replace profile pictures, and generally this is not done when streaming a window or application.
		/// </summary>
		/// <param name="spaceConnection">The active space connection to which to communicate your stremaing status</param>
		/// <param name="streaming">'true' if streaming content, 'false' if streaming a camera or other stream intended to represent yourself.</param>
        public static void SetLocalUserStreamingState(this CavrnusSpaceConnection spaceConnection, bool streaming)
        {
	        spaceConnection.SetLocalUserStreaming(streaming);//Value.RoomSystem.Comm.LocalCommUser.Value.UpdateLocalUserCameraStreamState(streaming);
		}
        
        /// <summary>
		/// Binds a function to receive a list of available audio input devices. This function will be called promptly with the current options, and called
		/// later if the available devices changes.
		/// The binding can be released by disposing of the returned IDisposable handle.
		/// </summary>
		/// <param name="spaceConnection">An active space connection with RTC enabled. If RTC is not enabled for this connection, or RTC is not supported on your platform, then
		/// there will be no available devices.</param>
		/// <param name="onRecvDevices">A callback invoked with the list of available input devices. It will be called promptly as well as deferredly, when the input devices change.</param>
		/// <returns></returns>
        public static IDisposable FetchAudioInputs(this CavrnusSpaceConnection spaceConnection, Action<List<CavrnusInputDevice>> onRecvDevices)
		{
			return spaceConnection.CurrentRtcContext.BindUntilTrue((ctx) => {
				if (ctx == null) 
					return false;
				
				ctx.FetchAudioInputOptions(res => {
					var devices = new List<CavrnusInputDevice>();
					foreach (var device in res) {
						devices.Add(new CavrnusInputDevice(device.Name, device.Id));
					}

					CavrnusStatics.Scheduler.ExecInMainThread(() => onRecvDevices?.Invoke(devices));
				});
				return true;
			});
		}

		/// <summary>
		/// Selects an audio input device. Fetch the list of options using <see cref="FetchAudioInputs"/>
		/// 
		/// If this function fails it will invoke the onFailure callback, and the device selected will be unchanged. 
		/// </summary>
		/// <param name="spaceConnection">An active space connection with RTC enabled.</param>
		/// <param name="device">The device to select.</param>
		/// <param name="onSuccess">An optional callback, invoked when the device is successfully changed.</param>
		/// <param name="onFailure">An optional callback, invoked when the device fails to change. Failures may be due to an OS lock on the device (if it is in use by another application in exclusive mode), by the device no longer being connected, or other possibilities too numerous to list.</param>
		public static void UpdateAudioInput(this CavrnusSpaceConnection spaceConnection, CavrnusInputDevice device, Action onSuccess = null, Action<string> onFailure = null)
		{
			spaceConnection.CurrentRtcContext.Value.ChangeAudioInputDevice(new Cavrnus.RtcCommon.RtcInputSource() { Id = device.Id, Name = device.Name },
			                                       (s) => {
													   Debug.Log("Changed audio input device to: " + s); 
													   onSuccess?.Invoke();
													},
			                                       err => {
													   Debug.LogError("Failed to change audio input device: " + err);
													   onFailure?.Invoke(err);
													   });
		}

		/// <summary>
		/// Binds a function to receive a list of available video streaming input devices. This function will be called promptly with the current options, and called
		/// later if the available devices changes.
		/// The binding can be released by disposing of the returned IDisposable handle.
		/// 
		/// There are may be a special option present which does not correspond to an input device. The 'Application' stream configures the RTC system to receive video frames from your integration and will not produce frames on its own.
		/// </summary>
		/// <param name="spaceConnection">An active space connection with RTC enabled. If RTC is not enabled for this connection, or RTC is not supported on your platform, then
		/// there will be no available input options.</param>
		/// <param name="onRecvDevices">A callback invoked with the list of available input devices. It will be called promptly as well as deferredly, when the input devices change.</param>
		/// <returns>An IDisposable handle that can be disposed of to release the callback from the RTC system.</returns>
		public static IDisposable FetchVideoInputs(this CavrnusSpaceConnection spaceConnection, Action<List<CavrnusVideoInputDevice>> onRecvDevices)
        {
	        return spaceConnection.CurrentRtcContext.BindUntilTrue(ctx => {
		        if (ctx == null) 
			        return false;
		        
		        ctx.FetchVideoInputOptions(res => {
			        var devices = new List<CavrnusVideoInputDevice>();
			        foreach (var device in res)
				        devices.Add(new CavrnusVideoInputDevice(device.Name, device.Id));
			        
			        CavrnusStatics.Scheduler.ExecInMainThread(() => onRecvDevices?.Invoke(devices));
		        });
		        return true;
	        });
		}

		/// <summary>
		/// Selects an video input source device. Fetch the list of options using <see cref="FetchVideoInputs"/>
		/// 
		/// If this function fails it will invoke the onFailure callback, and the device selected will be unchanged. 
		/// </summary>
		/// <param name="spaceConnection">An active space connection with RTC enabled.</param>
		/// <param name="device">The device to select.</param>
		/// <param name="onSuccess">An optional callback, invoked when the device is successfully changed.</param>
		/// <param name="onFailure">An optional callback, invoked when the device fails to change. Failures may be due to an OS lock on the device (if it is in use by another application in exclusive mode, common with cameras!), by the device no longer being connected, or other possibilities too numerous to list.</param>
		public static void UpdateVideoInput(this CavrnusSpaceConnection spaceConnection, CavrnusVideoInputDevice device, Action onSuccess = null, Action<string> onFailure = null)
		{
			spaceConnection.CurrentRtcContext.Value.ChangeVideoInputDevice(new Cavrnus.RtcCommon.RtcInputSource() { Id = device.Id, Name = device.Name },
												   (s) => {
													   Debug.Log("Changed video input device to: " + s);
													   onSuccess?.Invoke();
												   },
												   err => {
													   Debug.LogError("Failed to change video input device: " + err);
													   onFailure?.Invoke(err);
												   });
		}

		#endregion

		#region Remote Content

		/// <summary>
		/// Queries the Cavrnus server for information about uploaded content with the provided id. The callback will be invoked with content information
		/// as soon as it is acquired. Just once.
		/// This function does not cause the content to be downloaded. Only information/metadata about the content is fetched.
		/// </summary>
		/// <param name="id">The unique identifier for an asset stored within the Cavrnus server.</param>
		/// <param name="onGotContentInfo">Callback invoked with the metadata, once retrieved.</param>
		public static void FetchRemoteContentInfoById(string id, Action<CavrnusRemoteContent> onGotContentInfo)
		{
			if (!IsLoggedIn())
			{
				Debug.LogError("You must be logged in to access uploaded Content");
				return;
			}

			CavrnusContentHelpers.FetchRemoteContentInfoById(id, onGotContentInfo);
		}

		/// <summary>
		/// Issues a download request to the Cavrnus Server to acquire and decrypt content with the given id. Download progress will be reported to the progress callback.
		/// The streamProcessor callback will be called with the Stream and byte-length of the content file. The provided function needs to yield a Task and should probably
		/// be async. When the yielded Task completes, the stream will no longer be valid, so be sure to finish writing it before completing the task.
		/// Note that the read stream may read a few extra padding bytes depending on encryption. If your content is sensitive to extra padding bytes being present, then 
		/// be sure to not read more bytes than the provided length.
		/// 
		/// Extension methods for streams provided in Cavrnus.Base.Serialization.StreamExtensions may be helpful. For example, if you wish to write the content to a file,
		/// the callback might look something like: async (stream,len)=>stream.StreamToStreamAsyncWithMaxLength(filestream, len).
		/// 
		/// The Cavrnus system will cache the underlying encrypted files on disk; subsequent loads will be faster as they only need to decrypt. Because of this it is not
		/// recommended to write the decrypted contents to disk, unless your use for the content requires a file. If it does we recommend placing the file in a temporary
		/// folder which can be cleaned up on shutdown, so that decrypted content is not retained on disk.
		/// </summary>
		/// <param name="id">The content id being requested</param>
		/// <param name="progress">A callback invoked periodically with download progress.</param>
		/// <param name="streamProcessor">The stream processing function. This function will be invoked with the content stream and length, and must process that stream
		/// before concluding. The stream object will be closed once the task completes. As this function needs to return a Task, it will almost always be async.</param>
		public static void FetchFileById(string id, Action<string, float> progress, Func<Stream, long, Task> streamProcessor)
		{
			if(!IsLoggedIn())
			{
				Debug.LogError("You must be logged in to access uploaded Content");
				return;
			}

			
			CavrnusContentHelpers.FetchFileById(id, progress, streamProcessor);
		}

		/// <summary>
		/// Fetches a list of all server stored content the authenticated user can access. Might be a long list depending on your usage.
		/// This call fetches metadata only, and does not download the files.
		/// </summary>
		/// <param name="onCurrentContentArrived"></param>
		public static void FetchAllUploadedContent(Action<List<CavrnusRemoteContent>> onCurrentContentArrived)
		{
			if (!IsLoggedIn())
			{
				Debug.LogError("You must be logged in to access uploaded Content");
				return;
			}

			CavrnusContentHelpers.FetchAllUploadedContent(onCurrentContentArrived);
		}

		/// <summary>
		/// Uploads a file as a new server-stored asset. The filetype/category will attempt to be inferred from the filename.
		/// When the upload is complete, onUploadComplete will be called with the new object's metadata.
		/// The user can optionally supply a string->string map to be included in the metadata for later querying.
		/// </summary>
		/// <param name="localFilePath">The file to upload.</param>
		/// <param name="onUploadComplete">Callback invoked when the upload is complete.</param>
		/// <param name="tags">Optional mapping of additional metadata to be included with the new content object.</param>
		public static void UploadContent(string localFilePath,  Action<CavrnusRemoteContent> onUploadComplete, Dictionary<string, string> tags = null)
		{
			if (!IsLoggedIn())
			{
				Debug.LogError("You must be logged in to access uploaded Content");
				return;
			}

			CavrnusContentHelpers.UploadContent(localFilePath, onUploadComplete, tags);
		}

		#endregion

		#region UserMetadata
		
		/// <summary>
		/// Cavrnus User metadata includes a mapping for custom key/value pairs. This function retrieves the value for a provided key, for the given user.
		/// User metadata is persistent and not specific to a connected space.
		/// </summary>
		/// <param name="user">The user who's metadata to query.</param>
		/// <param name="key">The metadata key.</param>
		/// <returns>The metadata value, if present. "", otherwise.</returns>
		public static string GetUserMetadata(this CavrnusUser user, string key)
		{
			return CavrnusPropertyHelpers.GetStringPropertyValue(user.SpaceConnection, $"{user.ContainerId}/meta/", key);
		}
		
		/// <summary>
		/// Removes a key from user metadata, permanently. 
		/// </summary>
		/// <param name="key">The key to delete.</param>
		/// <param name="onSuccess">Callback invoked after successfully removing the metadata key.</param>
		/// <param name="onFailure">Callback invoked if the process fails, for example, due to lacking permissions.</param>
		public static void DeleteUserMetadata(string key, Action<string> onSuccess = null, Action<string> onFailure = null)
		{
			CavrnusPropertyHelpers.DeleteLocalUserMetadataByKey(key, onSuccess, onFailure);
		}

		/// <summary>
		/// Assigns a value to a metadata key for the authenticated user.
		/// User metadata is persistent and not space-specific so can be used to retain application settings per user independent of which space is joined.
		/// </summary>
		/// <param name="key">The key to add.</param>
		/// <param name="value">The value of the key being added.</param>
		/// <param name="onSuccess">Callback invoked after successfully removing the metadata key.</param>
		/// <param name="onFailure">Callback invoked if the process fails, for example, due to lacking permissions.</param>
		public static void UpdateLocalUserMetadataString(string key, string value, Action<string> onSuccess = null, Action<string> onFailure = null)
		{
			CavrnusPropertyHelpers.UpdateLocalUserMetadata(key, value, onSuccess, onFailure);
		}

		/// <summary>
		/// Assigns a value to a metadata key for the authenticated user, but as a json object rather than as a string.
		/// User metadata is persistent and not space-specific so can be used to retain application settings per user independent of which space is joined.
		/// </summary>
		/// <param name="key">The key to add.</param>
		/// <param name="jValue">The value of the key being added, as a JObject.</param>
		/// <param name="onSuccess">Callback invoked after successfully removing the metadata key.</param>
		/// <param name="onFailure">Callback invoked if the process fails, for example, due to lacking permissions.</param>
		public static void UpdateLocalUserMetadataJson(string key, JObject jValue, Action<string> onSuccess = null, Action<string> onFailure = null)
		{
			CavrnusPropertyHelpers.UpdateLocalUserMetadata(key, jValue.ToString(), onSuccess, onFailure);
		}

		/// <summary>
		/// Binds a callback with the value of the provided user's metadata for the given key. As this is 'Bind', the callback will be called immediately
		/// with the metadata value, and if the metadata value changes at a later time this callback will be invoked again.
		/// The returned IDisposable handle can be disposed of to release the binding.
		/// </summary>
		/// <param name="user">The user who's metadata to watch.</param>
		/// <param name="key">The metadata key being watched.</param>
		/// <param name="onMetadataChanged">Callback invoked immediately and deferredly, with the metadata value corresponding to the key provided. If the user
		/// has no metadata value for that key, the value will be "".</param>
		/// <returns></returns>
		public static IDisposable BindToUserMetadata(this CavrnusUser user, string key, Action<string> onMetadataChanged)
		{
			return user.ContainerIdSetting.SubBind(c => c == null ? null : user.SpaceConnection.BindStringPropertyValue($"{c}/meta/", key, onMetadataChanged));
		}

		/// <summary>
		/// Binds a callback with the value of the provided user's metadata for the given key, if you wish to interpret the value as json.
		/// As this is 'Bind', the callback will be called immediately with the metadata value, and if the metadata value changes at a later 
		/// time this callback will be invoked again.
		/// The returned IDisposable handle can be disposed of to release the binding.
		/// </summary>
		/// <param name="user">The user who's metadata to watch.</param>
		/// <param name="key">The metadata key being watched.</param>
		/// <param name="onMetadataChanged">Callback invoked immediately and deferredly, with the metadata value corresponding to the key provided. If the user
		/// has no metadata value for that key, the value will be an empty json object.</param>
		/// <returns></returns>
		public static IDisposable BindToUserMetadataJson(this CavrnusUser user, string key, Action<JObject> onMetadataChanged)
		{
			return user.ContainerIdSetting.SubBind(c => c == null ? null : user.SpaceConnection.BindJsonPropertyValue($"{c}/meta/", key, onMetadataChanged));
		}

        #endregion

        #region Analytics

		public static async Task<string> FetchSpaceHistory(CavrnusSpaceConnection spaceConn)
		{
			return await CavrnusHistoryHelpers.FetchJournalHistory(spaceConn);
		}

        #endregion
    }

    public class PropertyPostOptions
	{
		public bool smoothed = true;
	}

	public class CavrnusSpaceConnectionConfig
	{
		public string Tag = "";
		public bool IncludeRtc = true;
	}
}