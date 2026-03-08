using System.Collections.Generic;
using Cavrnus.Base.Settings;
using Cavrnus.SpatialConnector.API;

namespace Cavrnus.SpatialConnector.Core
{
	public static class CavrnusSpaceConnectionManager
    {
        public static readonly NotifyDictionary<string, CavrnusSpaceConnection> TaggedConnections = new();
        
        internal static CavrnusSpaceConnection GetSpaceConnectionByTag(string tag)
        {
            tag ??= "";
            
            if (TaggedConnections.TryGetValue(tag, out var foundConn))
                return foundConn;

            var newConnection = new CavrnusSpaceConnection(new CavrnusSpaceConnectionConfig{Tag = tag});
            TaggedConnections.TryAdd(tag, newConnection);

            return newConnection;
        }
        
        internal static void ExitSpace(CavrnusSpaceConnection spaceConnection)
        {
            TaggedConnections.Remove(spaceConnection.Config.Tag);
            spaceConnection.Dispose();
        }
        
        public static void Shutdown()
        {
            TaggedConnections?.Clear();
        }
    }
}