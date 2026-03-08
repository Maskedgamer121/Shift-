using System;
using System.Collections.Generic;
using Cavrnus.Base.Core;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm;
using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.Comm.Comm.NotifyApi;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.LiveRoomSystem;
using Cavrnus.LiveRoomSystem.GameEngineConnector;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;

namespace Cavrnus.SpatialConnector.Core
{
	internal static class CavrnusSpaceHelpers
	{
		internal static async void CreateSpace(string spaceName, Action<CavrnusSpaceInfo> onCreationComplete)
		{
			var roomComm = new RestRoomCommunication(CavrnusStatics.CurrentAuthentication.Endpoint, new FrameworkNetworkRequestImplementation());

			var req = new RestRoomCommunication.CreateRoomRequest {
				name = spaceName, description = "",
				environment = ""
			};

			var space = await roomComm.PostCreateRoomAsync(req);
			INotifyDataRoom notifyRoom = await CavrnusStatics.Notify.RoomsSystem.StartListeningSpecificAsync(space._id);

			onCreationComplete(new CavrnusSpaceInfo(notifyRoom));
		}
		
		internal static async void JoinSpace(string joinId, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects,
		                                     Action<CavrnusSpaceConnection> onConnected, Action<string> onFailure, CavrnusSpaceConnectionConfig config)
		{
			config ??= new CavrnusSpaceConnectionConfig();
			var spaceConnection = CavrnusSpaceConnectionManager.GetSpaceConnectionByTag(config.Tag);
			spaceConnection.DoLoadingEvents(joinId);
			
			var rsOptions = new RoomSystemOptions {
				CopresenceToProperties = false,
				LoadObjectsChats = true,
				LoadContentAssetsMetadata = false,
				LoadContentAssetsTextures = false,
				LoadContentObjectsMetadata = true,
				LoadContentObjectsScripts = true,
				LoadObjectsArTrackers = false,
				LoadContentObjects2DContent = false,
				LoadContentObjectsHoloContent = false,
				TranslationLanguage = null,
				LivePropertiesPermissiveness = ResponseLivePropertyCapabilities.Types.V1.Types.LivePropertyCapabilityEnum.Eager
			};

			var env = new RoomSystemEnvironment {
				PolicyEvaluator = CavrnusStatics.LivePolicyEvaluator,
				RolesMaintainer = CavrnusStatics.Notify.ContextualRoles,
				Scheduler = CavrnusStatics.Scheduler.BaseScheduler,
				NotifySystem = CavrnusStatics.Notify,
				EngineConnector = new EmptyGameEngineConnector(),
				ServerContentManager = CavrnusStatics.ContentManager,
			};
			
			var rs = new RoomSystem(CavrnusStatics.CreateRtcContext(config), env, rsOptions, null, (JournalRequestInfo)null);
			rs.InitializeConnection(CavrnusStatics.CurrentAuthentication.Endpoint, joinId);

			await rs.AwaitJournalProcessed();

			if (rs.SystemStatus.Value.Status == RoomSystemStatusEnum.Closed) {
				onFailure?.Invoke("Space connection is closed!");
				throw new ErrorInfo("Space connection is closed!");
			}

			if (rs.SystemStatus.Value.Status == RoomSystemStatusEnum.Error) {
				onFailure?.Invoke(rs.SystemStatus.Value.ErrorMessage);
				throw new ErrorInfo(rs.SystemStatus.Value.ErrorMessage);
			}

			var lu = await rs.AwaitLocalUser();
			lu.SetupVideoSources(CavrnusStatics.DesiredVideoStream, CavrnusStatics.DesiredVideoStream);
			
			var notifyRoom = await CavrnusStatics.Notify.RoomsSystem.StartListeningSpecificAsync(rs.ConnectedRoom.Value.V1.Id);

			spaceConnection.Update(rs, spawnableObjects, config, notifyRoom);
			onConnected(spaceConnection);
		}
		
		internal static void AwaitAnySpaceBeginLoading(Action<string> onLoading, string tag)
		{
			CavrnusSpaceConnectionManager.GetSpaceConnectionByTag(tag).TrackLoadingEvent(onLoading);
		}

		internal static void AwaitAnySpaceConnection(Action<CavrnusSpaceConnection> onConnected, string tag)
		{
			CavrnusSpaceConnectionManager.GetSpaceConnectionByTag(tag).TrackConnectedEvent(onConnected);
		}
		
		internal static async void GetCurrentlyAvailableSpaces(Action<List<CavrnusSpaceInfo>> onRecvCurrentJoinableSpaces)
		{
			RestRoomCommunication rrc =
				new RestRoomCommunication(CavrnusStatics.CurrentAuthentication.Endpoint, new FrameworkNetworkRequestImplementation());

			var uri = await rrc.GetUserFullRoomsAndInvitesInfoAsync();

			DebugOutput.Info($"Fetched rooms list: {uri.rooms.Length} rooms.");

			var res = new List<CavrnusSpaceInfo>();
			foreach (var room in uri.rooms)
			{
				res.Add(new CavrnusSpaceInfo(room));
			}

			res.Sort((x, y) => DateTime.Compare(x.LastAccessedTime, y.LastAccessedTime));

			onRecvCurrentJoinableSpaces(res);
		}

		internal static IDisposable BindAllAvailableSpaces(Action<CavrnusSpaceInfo> spaceAdded, Action<CavrnusSpaceInfo> spaceRemoved)
		{
			List<IDisposable> disposables = new List<IDisposable>();

			CavrnusStatics.Notify.RoomsSystem.StartListeningAsync();

			var accessFilter = new NotifyDictionaryFiltererDynamic<string, INotifyDataRoom>(CavrnusStatics.Notify.RoomsSystem.RoomsInfo,
				(id, ndr) =>
				{
					return ndr.Access.Value != RoomMetadata.Types.RoomAccess.None && ndr.ConnectedMember.Value != null;
				},
				(id, ndr, hook) => new IDisposable[] { ndr.Access.Hook(hook), ndr.ConnectedMember.Hook(hook) });
			disposables.Add(accessFilter);

			#region Room Type Filter
			var typeFilter = new NotifyDictionaryFilterer<string, INotifyDataRoom>(accessFilter.Result,
				(id, ndr) => ndr.RoomType != "instance");
			disposables.Add(typeFilter);
			#endregion

			var archivedFilter = new NotifyDictionaryFiltererDynamic<string, INotifyDataRoom>(typeFilter.Result,
				(s, ndr) =>
				{
					bool visible = !(ndr.ConnectedMember.Value?.Hidden?.Value ?? false) &&
								   ndr.Access.Value != RoomMetadata.Types.RoomAccess.None;
					return visible;
				},
			(s, entry, hook) => new IDisposable[] { entry.ConnectedMember.Value?.Hidden?.Hook(hook), entry.Access.Hook(hook), });
			disposables.Add(archivedFilter);

			NotifyDictionaryListMapper<string, INotifyDataRoom, CavrnusSpaceInfo> mapper =
				new NotifyDictionaryListMapper<string, INotifyDataRoom, CavrnusSpaceInfo>(archivedFilter.Result,
					(s, ile) => new CavrnusSpaceInfo(ile),
					(a, b) => Comparer<DateTime?>.Default.Compare(b.LastAccessedTime, a.LastAccessedTime));
			disposables.Add(mapper);

			var bnd = mapper.Result.BindAll(spaceAdded, spaceRemoved);
			disposables.Add(bnd);

			return new MultiDisposalHelper(disposables.ToArray());
		}
	}
}