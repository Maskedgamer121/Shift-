using System;
using System.Collections;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.SpatialConnector.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// This static class provides a number of extension methods for types such as CavrnusUser and CavrnusSpaceConnection to assist in implementing common functionality.
	/// </summary>
	public static class CavrnusShortcutsLibrary
    {
		/// <summary>
		/// Bind a function to receive the property container id for a given user. The user's live properties will be contained within this path. Use this function to get
		/// the root path, within which you can watch other properties. For example, if you wished to bind if a user is muted, you need to bind the property at {user container id}/muted, where usercontainerid comes from this function.
		/// The user's container id will only ever change if this user changes between spaces, which will only be true for the local user, if you join a new space using the same tag.
		/// </summary>
		/// <param name="user">The user (either self or remote connection) who's property container to retrieve.</param>
		/// <param name="onContainerIdChanged">Callback invoked with the container id, immediately and when it changes. Changes are rare and only occur when changing spaces with the same tag.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
	    public static IDisposable BindUserContainerId(this CavrnusUser user, Action<string> onContainerIdChanged)
	    {
		    return user.ContainerIdSetting.Bind(onContainerIdChanged);
	    }

		/// <summary>
		/// Retrieve the user's speaking property, a boolean value that indicates that the user is currently loud.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		/// <returns></returns>
		public static bool GetUserSpeaking(this CavrnusUser user)
		{
			return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Speaking);
		}

		/// <summary>
		/// Bind a function to receive the value of the user-speaking property, a boolean property nominally available at /users/{connection id}/speaking.
		/// This property is true when the user's audio input is sufficiently loud. It will change frequently.
		/// </summary>
		/// <param name="user">The user (either self or remote connection) who's property to bind.</param>
		/// <param name="onContainerIdChanged">Callback invoked with the status of the 'speaking' property for this user..</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindUserSpeaking(this CavrnusUser user, Action<bool> onSpeakingChanged)
		{
			return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetBoolPropertyBinding(user.SpaceConnection, c, UserPropertyDefs.User_Speaking, onSpeakingChanged));
        }

		/// <summary>
		/// Retrieve the user's muted property, a boolean value that indicates that the user has muted their audio device.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		/// <returns>The user's mute state.</returns>
		public static bool GetUserMuted(this CavrnusUser user)
        {
            return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Muted);
        }

		/// <summary>
		/// Bind a function to the user's muted property, a boolean value that indicates that the user has muted their audio device.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		/// <param name="onMutedChanged">A callback invoked immediately and later, with the user's live mute state.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindUserMuted(this CavrnusUser user, Action<bool> onMutedChanged)
		{
			return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetBoolPropertyBinding(user.SpaceConnection, c, UserPropertyDefs.User_Muted, onMutedChanged));
		}

		/// <summary>
		/// Retrieve the user's muted property, a boolean value that indicates that the user is streaming content, as opposed to a camera.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		/// <returns>The user's streaming state.</returns>
		public static bool GetUserStreaming(this CavrnusUser user)
		{
			return user.SpaceConnection.GetBoolPropertyValue(user.ContainerId, UserPropertyDefs.User_Streaming);
		}

		/// <summary>
		/// Bind a function to the user's streaming property, a boolean value that indicates that the user is streaming content, as opposed to a camera.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		/// <param name="onMutedChanged">A callback invoked immediately and later, with the user's live streaming state.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindUserStreaming(this CavrnusUser user, Action<bool> onStreamingChanged)
		{
			return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetBoolPropertyBinding(user.SpaceConnection, c, UserPropertyDefs.User_Streaming, onStreamingChanged));
		}

		/// <summary>
		/// Retrieve the user's display name property, a string value that reflects the user's name. This name will be their selected screen name, otherwise their first and last name, otherwise their account id / email.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		public static string GetUserName(this CavrnusUser user)
		{
			return user.SpaceConnection.GetStringPropertyValue(user.ContainerId, UserPropertyDefs.Users_Name);
		}

		/// <summary>
		/// Bind a function to the user's display name property, a string value that reflects the user's name. This name will be their selected screen name, otherwise their first and last name, otherwise their account id / email.
		/// </summary>
		/// <param name="user">The user being queried.</param>
		/// <param name="onMutedChanged">A callback invoked immediately and later, with the user's display name</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindUserName(this CavrnusUser user, Action<string> onNameChanged)
		{
			return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetStringPropertyBinding(user.SpaceConnection, c, UserPropertyDefs.Users_Name, onNameChanged));
		}       

		/// <summary>
		/// Binds a function to receive a Unity Sprite that has been preloaded to the requested user's profile picture. This internally binds the 'profilePicture' user property, which is a url, and loads it in parallel for you.
		/// If the user changes their profile picture the bound function will fire again, until the handle has been disposed of.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="onProfilePicChanged">Callback invoked with a fully loaded Sprite with the user's profile picture.</param>
		/// <returns></returns>
        public static IDisposable BindProfilePic(this CavrnusUser user, Action<Sprite> onProfilePicChanged)
        {
	        return user.ContainerIdSetting.SubBind(c => c == null ? null : 
		        CavrnusPropertyHelpers.GetStringPropertyBinding(user.SpaceConnection, c, UserPropertyDefs.Users_Profile_ProfilePicture, (pp) =>
		        {
			        CavrnusStatics.Scheduler.ExecCoRoutine(LoadProfilePic(pp, onProfilePicChanged));
				}));
        }

		/// <summary>
		/// This coroutine loads the given url into a sprite, then calls the provided callback with that Sprite when complete. It uses UnityWebRequest. This function is also used by BindProfilePic to load profile pictures.
		/// </summary>
		/// <param name="url">The image URL to load.</param>
		/// <param name="onProfilePicChanged">Callback invoked with the loaded image, as a Sprite</param>
		public static IEnumerator LoadProfilePic(string url, Action<Sprite> onProfilePicChanged)
        {
	        if (url == null) yield break;
	        
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                var sprite = Sprite.Create(myTexture as Texture2D, new Rect(Vector2.zero, new Vector2(myTexture.width, myTexture.height)), Vector2.zero);

                onProfilePicChanged(sprite);
            }
        }

		/// <summary>
		/// Generically bind a boolean property belonging to the provided user. This is functionality similar to using <see cref="BindUserContainerId"/> to fetch the user's property container, then calling <see cref="BindBoolPropertyValue"/> to retrieve the contained property. This function just makes it easier.
		/// </summary>
		/// <param name="user">The user to fetch a property of.</param>
		/// <param name="propertyName">The property to fetch. Common examples are "muted", "streaming", "name", etc.</param>
		/// <param name="onPropertyUpdated">Function invoked when the property value changes. It will also be invoked immediately with the current value.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindBoolPropertyValue(this CavrnusUser user, string propertyName, Action<bool> onPropertyUpdated)
        {
	        return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetBoolPropertyBinding(user.SpaceConnection, c, propertyName, onPropertyUpdated));
        }

		/// <summary>
		/// Generically bind a string property belonging to the provided user. This is functionality similar to using <see cref="BindUserContainerId"/> to fetch the user's property container, then calling <see cref="BindStringPropertyValue"/> to retrieve the contained property. This function just makes it easier.
		/// </summary>
		/// <param name="user">The user to fetch a property of.</param>
		/// <param name="propertyName">The property to fetch. Common examples are "name", "profilePicture", etc.</param>
		/// <param name="onPropertyUpdated">Function invoked when the property value changes. It will also be invoked immediately with the current value.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
        public static IDisposable BindStringPropertyValue(this CavrnusUser user, string propertyName, Action<string> onPropertyUpdated)
        {
	        return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetStringPropertyBinding(user.SpaceConnection, c, propertyName, onPropertyUpdated));
        }

		/// <summary>
		/// Generically bind a color property belonging to the provided user. This is functionality similar to using <see cref="BindUserContainerId"/> to fetch the user's property container, then calling <see cref="BindColorPropertyValue"/> to retrieve the contained property. This function just makes it easier.
		/// </summary>
		/// <param name="user">The user to fetch a property of.</param>
		/// <param name="propertyName">The property to fetch. Common examples are "primaryColor", "secondaryColor", etc.</param>
		/// <param name="onPropertyUpdated">Function invoked when the property value changes. It will also be invoked immediately with the current value.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindColorPropertyValue(this CavrnusUser user, string propertyName, Action<Color> onPropertyUpdated)
        {
	        return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetColorPropertyBinding(user.SpaceConnection, c, propertyName, onPropertyUpdated));

		}

		/// <summary>
		/// Generically bind a number/float/scalar property belonging to the provided user. This is functionality similar to using <see cref="BindUserContainerId"/> to fetch the user's property container, then calling <see cref="BindFloatPropertyValue"/> to retrieve the contained property. This function just makes it easier.
		/// </summary>
		/// <param name="user">The user to fetch a property of.</param>
		/// <param name="propertyName">The property to fetch. Common examples are "volume", "audioGain", etc.</param>
		/// <param name="onPropertyUpdated">Function invoked when the property value changes. It will also be invoked immediately with the current value.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindFloatPropertyValue(this CavrnusUser user, string propertyName, Action<float> onPropertyUpdated)
        {
	        return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetFloatPropertyBinding(user.SpaceConnection, c, propertyName, onPropertyUpdated));
		}

		/// <summary>
		/// Generically bind a transform belonging to the provided user. This is functionality similar to using <see cref="BindUserContainerId"/> to fetch the user's property container, then calling <see cref="BindTransformPropertyValue"/> to retrieve the contained property. This function just makes it easier.
		/// </summary>
		/// <param name="user">The user to fetch a property of.</param>
		/// <param name="propertyName">The property to fetch. Common examples are "transform", etc.</param>
		/// <param name="onPropertyUpdated">Function invoked when the property value changes. It will also be invoked immediately with the current value.</param>
		/// <returns>An IDisposable handle to release the binding.</returns>
		public static IDisposable BindTransformPropertyValue(this CavrnusUser user, string propertyName, Action<CavrnusTransformData> onPropertyUpdated)
        {
	        return user.ContainerIdSetting.SubBind(c => c == null ? null : CavrnusPropertyHelpers.GetTransformPropertyBinding(user.SpaceConnection, c, propertyName, onPropertyUpdated));
        }

		/// <summary>
		/// Posts a change to a transform property within the provided space. This is a convenience helper to simpify usage of <see cref="CavrnusFunctionLibrary.PostTransformPropertyUpdate"/>, which exposes the transform as three Vector3s pos (position), rot (euler rotation), scl (scale).
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="pos">The transform position to assign.</param>
		/// <param name="rot">The transform euler rotation to assign.</param>
		/// <param name="scl">The transform scale to assign.</param>
		public static void PostTransformPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Vector3 pos, Vector3 rot, Vector3 scl)
        {
            spaceConn.PostTransformPropertyUpdate(containerName, propertyName, new CavrnusTransformData(pos, rot, scl));
        }

		/// <summary>
		/// Posts a change to a transform property within the provided space. This is a convenience helper to simpify usage of <see cref="CavrnusFunctionLibrary.PostTransformPropertyUpdate"/>, which accepts a Unity Transform object and internally maps that to a Transform property value type.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="containerName">The property path for the property value to assign</param>
		/// <param name="propertyName">The name of the property in tge given path to update</param>
		/// <param name="transform">The Unity transform to extract transform information from.</param>
		public static void PostTransformPropertyUpdate(this CavrnusSpaceConnection spaceConn, string containerName, string propertyName, Transform transform)
        {
            spaceConn.PostTransformPropertyUpdate(containerName, propertyName, new CavrnusTransformData(transform.localPosition, transform.localEulerAngles, transform.localScale));
        }

		/// <summary>
		/// Posts two operations to the connect space's journal. The first creates an object the same as <see cref="CavrnusFunctionLibrary.SpawnObject"/>. The second assigns a transform property to a 'Transform' property belonging to the new object.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="uniqueId">An identifier the application may use to identify which object to create. The Cavrnus system does not interact with this value other than to provide it to the clients.</param>
		/// <param name="pos">A Vector3 position to place the object at.</param>
		/// <param name="onSpawnComplete">A callback invoked when the object has been fully created in the journal. This is asynchronous because of the need to wait for the server to reply with an identifier for the object creation.</param>
		/// <returns>The container path for the newly created object.</returns>
		public static string SpawnObject(this CavrnusSpaceConnection spaceConn, string uniqueId, Vector3 pos, Action<CavrnusSpawnedObject, GameObject> onSpawnComplete = null)
		{
			string instanceContainerId = spaceConn.SpawnObject(uniqueId, onSpawnComplete);

			spaceConn.PostTransformPropertyUpdate(instanceContainerId, "Transform", pos, Vector3.zero, Vector3.one);

			return instanceContainerId;
		}

		/// <summary>
		/// Posts two operations to the connect space's journal. The first creates an object the same as <see cref="CavrnusFunctionLibrary.SpawnObject"/>. The second assigns a transform property to a 'Transform' property belonging to the new object.
		/// This version accepts the transform position Vector3, as well as an Euler angle rotation and scale.
		/// </summary>
		/// <param name="spaceConn">The connected space containing the property.</param>
		/// <param name="uniqueId">An identifier the application may use to identify which object to create. The Cavrnus system does not interact with this value other than to provide it to the clients.</param>
		/// <param name="pos">A Vector3 position to place the object at.</param>
		/// <param name="rot">A Vector3 euler rotation for the assigned transform</param>
		/// <param name="scale">A Vector3 scale for the assigned transform.</param>
		/// <param name="onSpawnComplete">A callback invoked when the object has been fully created in the journal. This is asynchronous because of the need to wait for the server to reply with an identifier for the object creation.</param>
		/// <returns>The container path for the newly created object.</returns>
		public static string SpawnObject(this CavrnusSpaceConnection spaceConn, string uniqueId, Vector3 pos, Vector3 rot, Vector3 scale, Action<CavrnusSpawnedObject, GameObject> onSpawnComplete = null)
        {
			string instanceContainerId = spaceConn.SpawnObject(uniqueId, onSpawnComplete);

			spaceConn.PostTransformPropertyUpdate(instanceContainerId, "Transform", pos, rot, scale);

            return instanceContainerId;
		}
    }
}
