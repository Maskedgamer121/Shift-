using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cavrnus.SpatialConnector.API;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
    internal static class CavrnusHistoryHelpers
    {
        internal static async Task<string> FetchJournalHistory(CavrnusSpaceConnection spaceConn)
        {
            HistoryBuilderOptions options = new HistoryBuilderOptions();
            options.activityTimeUnificationTimestepSeconds = 2.0;

            options.activityTimeUnificationTimestepWaitThresholds = new List<Tuple<TimeSpan, double>>()
            {
                Tuple.Create(TimeSpan.FromDays(1), 3.0),
            };

            HistoryBuilder builder = new HistoryBuilder(CavrnusStatics.CurrentAuthentication.Endpoint, options);
            string res = await builder.ProcessRoom(spaceConn.CurrentSpaceInfo.Value.Id, true);

            return res;
        }
    }
}