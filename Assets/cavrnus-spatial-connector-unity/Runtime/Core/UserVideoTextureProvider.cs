using System;
using Cavrnus.Base.Collections;
using Cavrnus.Base.Graphics;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LiveApi;
using Cavrnus.RtcCommon;
using Cavrnus.EngineConnector;
using UnityEngine;
using Cavrnus.EngineConnector.Content;

namespace Cavrnus.SpatialConnector.Core
{
	internal class UserVideoTextureProvider
	{
        internal Setting<TextureWithUVs> providedTexture { get; set; } = new Setting<TextureWithUVs>();
		private bool providedTextureNeedsToBeDestroyed = false;

		private IRegistrationSet<TextureWithUVs, int> regSet;

		private bool providerHooked = false;
		private IDisposable directProviderHook = null;
		private readonly ISessionCommunicationUser user;

        internal UserVideoTextureProvider(ISessionCommunicationUser user)
		{
			this.user = user;
			providedTexture.ChangedEvent += ProvidedTextureOnChangedEvent;

            HookVideoProvider();
        }

		private void ProvidedTextureOnChangedEvent(TextureWithUVs newValue, TextureWithUVs oldValue)
		{
			if (oldValue?.Texture != null && providedTextureNeedsToBeDestroyed)
				UnityEngine.Object.Destroy(oldValue?.Texture);
		}

		private IVideoProvider ProviderAccessor
		{
			get
			{
				if (user is ISessionCommunicationLocalUser)
					return ((ISessionCommunicationLocalUser)user).Rtc.VideoProvider;
				if (user is ISessionCommunicationRemoteUser)
					return ((ISessionCommunicationRemoteUser)user).Rtc.VideoProvider;
				return null;
			}
		}

		private void HookVideoProvider()
		{
			if (providerHooked)
				return;

			var provider = ProviderAccessor;
			if (provider == null)
				return;

			Debug.Log($"Hooked image change for {this.user}.");
			providerHooked = true;
			if (provider is IImageBasedVideoProvider ibp)
			{
				ibp.CurrentImageChangedEvent += ProviderOnCurrentImageChangedEvent;
				UpdateForImage(ibp.Current, ibp.FrameIndex);
			}
			else if (provider is IDirectObjectVideoProvider idp)
			{
				idp.VideoObject.Bind(UpdateForTexture);
			}
		}
        private void UpdateForTexture(object val, object old)
		{
            Debug.Log("Updating for Texture " + val);

            if (val is Texture2D tex)
			{
				Debug.Log($"UVTP: arg. {SystemInfo.graphicsUVStartsAtTop}");
				
				#if UNITY_VISIONOS
					providedTexture.Value = new TextureWithUVs(tex, new Rect(0, 1, 1, -1));
				#else
					if (SystemInfo.graphicsUVStartsAtTop)
						providedTexture.Value = new TextureWithUVs(tex, new Rect(0, 1, 1, -1));
					else
						providedTexture.Value = new TextureWithUVs(tex, new Rect(0, 0, 1, 1));
				#endif
			}
			else if (val is TextureWithUVs twu)
				providedTexture.Value = twu;
#if !BUILD_MAGICLEAP && !BUILD_QUEST && !UNITY_VISIONOS
			else if (val is WebCamTexture wct)
				providedTexture.Value = new TextureWithUVs(wct, new Rect(0, 0, 1, 1));
#endif
			else if (val is RenderTexture rt)
			{
				// only true for self-stream in direct-video systems (android atm 1/23)
				if (SystemInfo.graphicsUVStartsAtTop)
					providedTexture.Value = new TextureWithUVs(rt, new Rect(0, 1, 1, -1));
				else
					providedTexture.Value = new TextureWithUVs(rt, new Rect(0, 0, 1, 1));
			}

			providedTextureNeedsToBeDestroyed = false;
		}

		private void UpdateForImage(IImage2D im, int frame)
		{
			Debug.Log("Updating for remote image " + frame);

			if (im == null)
			{
				var oldtex = providedTexture.Value;
				providedTexture.Value = null;

				if (oldtex?.Texture != null)
					UnityEngine.Object.Destroy(oldtex.Texture);

				NotifyImageConsumed(frame);
				return;
			}

			var tex = providedTexture.Value;
			if (im is Image2D i2)
			{
				Texture2D texIm = tex?.Texture as Texture2D;
				TextureConvert.TextureFromImage2D(ref texIm, i2);
				tex = new TextureWithUVs(texIm, new Rect(0, 1, 1, -1));
			}
			else if (im is Image2DUnmanaged iu)
			{
				if (tex?.Texture == null || tex.Texture.width != iu.Resolution.x || tex.Texture.height != iu.Resolution.y)
				{
					if (tex?.Texture != null)
						UnityEngine.Object.DestroyImmediate(tex?.Texture);

					var format = TextureFormat.BGRA32;

#if UNITY_STANDALONE_OSX || UNITY_IOS

					format = TextureFormat.ARGB32;
#elif UNITY_WEBGL
					format = TextureFormat.RGBA32;
#endif

					tex = new TextureWithUVs(new Texture2D(iu.Resolution.x, iu.Resolution.y, format, false, false),
						new Rect(0, 1, 1, -1));
				}
				try
				{
					((Texture2D)tex.Texture).LoadRawTextureData((im as Image2DUnmanaged).ImageDataPtr, im.Resolution.x * im.Resolution.y * 4);
					((Texture2D)tex.Texture).Apply(false, false);
				}
				catch (Exception e)
				{
					Debug.LogError(e.Message);
				}
			}
			else
				throw new ArgumentException("UserVideoTextureProvider.UpdateForImage: type of arg image is unknown.");

			providedTexture.Value = tex;
			providedTextureNeedsToBeDestroyed = true;

			NotifyImageConsumed(frame);
		}

		private void NotifyImageConsumed(int frame)
		{
			if (user is ISessionCommunicationLocalUser)
				((ISessionCommunicationLocalUser)user).Rtc.NotifyVideoImageProcessed(frame);
			if (user is ISessionCommunicationRemoteUser)
				((ISessionCommunicationRemoteUser)user).Rtc.NotifyVideoImageProcessed(frame);
		}

		private void ProviderOnCurrentImageChangedEvent(IImage2D image, int frame)
		{
			if (providerHooked && ProviderAccessor is IImageBasedVideoProvider ibp)
			{
				UpdateForImage(ibp.Current, frame);
			}
		}

		private void UnhookVideoProvider()
		{
			if (providedTexture.Value != null)
			{
				if (providedTexture.Value.Texture != null)
					UnityEngine.Object.DestroyImmediate(providedTexture.Value.Texture);
				providedTexture.Value = null;
			}

			if (providerHooked)
			{
				providerHooked = false;

				directProviderHook?.Dispose();

				var provider = ProviderAccessor;
				if (provider is IImageBasedVideoProvider ibp)
				{
					ibp.CurrentImageChangedEvent -= ProviderOnCurrentImageChangedEvent;
				}
			}
		}

        internal void Shutdown()
		{
			providedTexture.ChangedEvent -= ProvidedTextureOnChangedEvent;
			if (providerHooked)
				UnhookVideoProvider();
			//			UserSettings.Instance.ReduceStreamingVideoQualityReceived.ChangedEvent -= ReduceStreamingVideoQualityReceivedOnChangedEvent;
		}
	}
}
