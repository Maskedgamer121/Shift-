using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class WidgetUserMic : MonoBehaviour
	{
		[SerializeField] private GameObject speakingGameObject;
		[SerializeField] private GameObject mutedGameObject;

		[SerializeField] private WidgetMicPulse micPulse;

		private CavrnusUser user;
		private CavrnusSpaceConnection spaceConn;
		private readonly List<IDisposable> disposables = new List<IDisposable>();
		
		public void Setup(CavrnusUser user)
		{
			this.user = user;
			spaceConn = user.SpaceConnection;
			micPulse.Setup(user);
			
			var mutedDisposable = user.BindUserMuted(muted =>
			{
				if (muted)
				{
					speakingGameObject.SetActive(false);
					mutedGameObject.SetActive(true);
				}
				else
				{
					speakingGameObject.SetActive(true);
					mutedGameObject.SetActive(false);
				}
			});

			disposables.Add(mutedDisposable);
		}
		
		public void ToggleMic()
		{
			if (user?.IsLocalUser ?? false)
				spaceConn?.SetLocalUserMutedState(!user.GetUserMuted());
			else
				user?.RequestRemoteUserMute();
		}

		private void OnDestroy()
		{
			foreach (var disp in disposables)
				disp.Dispose();
		}
	}
}