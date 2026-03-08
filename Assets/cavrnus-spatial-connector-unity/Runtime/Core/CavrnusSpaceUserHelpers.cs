using System;
using Cavrnus.Base.Core;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LiveApi;
using Cavrnus.SpatialConnector.API;

namespace Cavrnus.SpatialConnector.Core
{
	internal static class CavrnusSpaceUserHelpers
	{
		internal static IDisposable BindSpaceUsers(CavrnusSpaceConnection spaceConn, Action<CavrnusUser> userAdded, Action<CavrnusUser> userRemoved)
		{
			IDisposable mapBind = null;
			NotifyListMapper<ISessionCommunicationRemoteUser, CavrnusUser> mapper = null;

			var luBind = spaceConn.CurrentLocalUserSetting.Bind((clu, oldlu) =>
			{
				if (oldlu != null)
					userRemoved(oldlu);
				if (clu != null)
					userAdded(clu);
			});

			var spaceRemotesBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				if (sc == null)
					return;
	
				mapper?.Dispose();
				mapBind?.Dispose();
				
				mapper = new NotifyListMapper<ISessionCommunicationRemoteUser, CavrnusUser>(spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.ConnectedUsers);
				mapper.BeginMapping(ru => new CavrnusUser(ru, spaceConn));
				mapBind = mapper.Result.BindAll(userAdded, userRemoved);
			});
			
			return mapper.AlsoDispose(mapBind).AlsoDispose(spaceRemotesBind).AlsoDispose(luBind);
		}
	}
}