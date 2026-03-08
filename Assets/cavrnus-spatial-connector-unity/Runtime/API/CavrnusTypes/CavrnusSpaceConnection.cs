using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cavrnus.Base.Collections;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LiveApi;
using Cavrnus.Comm.Comm.NotifyApi;
using Cavrnus.LiveRoomSystem;
using Cavrnus.RtcCommon;
using Cavrnus.SpatialConnector.Setup;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Represents a connection to a live space for a given Tag.
	/// The underlying connection within this helper may change if the user joins a new space with the same tag.
	///
	/// Space functionality is not accessed through this class, but rather through the functions of <see cref="CavrnusFunctionLibrary"/> and <see cref="CavrnusShortcutsLibrary"/>.
	/// </summary>
	public class CavrnusSpaceConnection : IDisposable
	{
		public CavrnusSpaceConnectionConfig Config{ get; private set; }

		internal IReadonlySetting<CavrnusSpaceInfo> CurrentSpaceInfo => currentSpaceInfo;
		private readonly ISetting<CavrnusSpaceInfo> currentSpaceInfo = new Setting<CavrnusSpaceInfo>();

		internal IReadonlySetting<CavrnusSpaceConnectionData> CurrentSpaceConnection => currentSpaceConnection;
		private readonly ISetting<CavrnusSpaceConnectionData> currentSpaceConnection = new Setting<CavrnusSpaceConnectionData>();
		
		internal IReadonlySetting<CavrnusUser> CurrentLocalUserSetting => currentLocalUserSetting;
		private readonly ISetting<CavrnusUser> currentLocalUserSetting = new Setting<CavrnusUser>();
		
		internal IReadonlySetting<IRtcContext> CurrentRtcContext => currentRtcContext;
		private readonly ISetting<IRtcContext> currentRtcContext = new Setting<IRtcContext>();
		
		private readonly List<Action<string>> onLoadingEvents = new();
		private readonly NotifyList<Action<CavrnusSpaceConnection>> onConnectedEvents = new();

		private readonly List<IDisposable> bindings = new ();

		private bool setMute = false;
		private bool setStreaming = false;
		
		public CavrnusSpaceConnection(CavrnusSpaceConnectionConfig config)
		{
			Config = config;
			
			bindings.Add(CurrentSpaceConnection.Bind(async sc =>
			{
				if (sc == null) 
					return;
				
				if (onConnectedEvents.Count > 0)
					onConnectedEvents.ForEach(callback => callback?.Invoke(this));

				onConnectedEvents.Clear();
				
				var lu = await GetLocalUserAsync(sc);
				lu.Rtc.Muted.Value = setMute;
				lu.UpdateLocalUserCameraStreamState(setStreaming);
			}));
		}	
		
		internal void Update(RoomSystem roomSystem, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects, CavrnusSpaceConnectionConfig config, INotifyDataRoom ndr)
		{
			Config = config;
			currentSpaceConnection.Value?.Dispose();
			
			currentSpaceConnection.Value = new CavrnusSpaceConnectionData(roomSystem, spawnableObjects, this);
			currentRtcContext.Value = roomSystem.RtcContext;
			currentSpaceInfo.Value = new CavrnusSpaceInfo(ndr);
		}
		
		private async Task<ISessionCommunicationLocalUser> GetLocalUserAsync(CavrnusSpaceConnectionData scd)
		{
			try {
				var user = await scd.RoomSystem.AwaitLocalUser();

				if (currentLocalUserSetting.Value == null)
					currentLocalUserSetting.Value = new CavrnusUser(user, this);
				else
					currentLocalUserSetting.Value.InitUser(user);

				return user;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return null;
		}
		
		internal IDisposable AwaitLocalUser(Action<CavrnusUser> onLocalUser)
		{
			return currentLocalUserSetting.BindUntilTrue(lu => {
				if (lu == null)
					return false;

				onLocalUser?.Invoke(lu);
				return true;
			});
		}
		
		internal IDisposable BindSpaceInfo(Action<CavrnusSpaceInfo> onUpdated)
		{
			return currentSpaceInfo.Bind(si => {
				if (si == null)
					return;
				onUpdated?.Invoke(si);
			});
		}

		internal IDisposable BindLocalUser(Action<CavrnusUser> onLocalUser)
		{
			return currentLocalUserSetting.Bind(lu => {
				if (lu == null)
					return;
				onLocalUser?.Invoke(lu);
			});
		}

		internal void DoLoadingEvents(string joinId)
		{
			onLoadingEvents.ForEach(le => le?.Invoke(joinId));
		}

		internal void TrackLoadingEvent(Action<string> onLoading)
		{
			onLoadingEvents.Add(onLoading);
		}

		internal void TrackConnectedEvent(Action<CavrnusSpaceConnection> onConnected)
		{
			if (currentSpaceConnection.Value == null)
				onConnectedEvents.Add(onConnected);
			else
				onConnected?.Invoke(this);
		}

		internal IDisposable BindConnection(Action<CavrnusSpaceConnection> onConnection)
		{
			return currentSpaceConnection.Bind((c) =>
			{
				if (c != null) onConnection(this);
			});
		}

		public void SetLocalUserMuted(bool muted)
		{
			this.setMute = muted;
			if (CurrentSpaceConnection.Value == null)
				return;
			if (CurrentSpaceConnection.Value.RoomSystem.Comm.LocalCommUser.Value != null)
				CurrentSpaceConnection.Value.RoomSystem.Comm.LocalCommUser.Value.Rtc.Muted.Value = muted;
			// Otherwise we'll set the state when local user arrives; it is already bound up above
		}

		public void SetLocalUserStreaming(bool streaming)
		{
			this.setStreaming = streaming;
			if (CurrentSpaceConnection.Value == null)
				return;
			if (CurrentSpaceConnection.Value.RoomSystem.Comm.LocalCommUser.Value != null)
				CurrentSpaceConnection.Value.RoomSystem.Comm.LocalCommUser.Value.UpdateLocalUserCameraStreamState(streaming);
			// Otherwise we'll set the state when local user arrives; it is already bound up above
		}
		public void Dispose()
		{
			currentSpaceConnection?.Value?.Dispose();
			bindings?.ForEach(b => b?.Dispose());
			currentRtcContext?.Value?.Shutdown();
		}
	}
}