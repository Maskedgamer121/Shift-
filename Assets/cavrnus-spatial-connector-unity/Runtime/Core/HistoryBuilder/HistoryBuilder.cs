using System.Collections.Generic;
using System.Threading.Tasks;
using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.Comm.Comm.RestApi;
using Operation = Cavrnus.Comm.Comm.LiveTypes.Operation;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public class HistoryBuilder
	{
		private RestApiEndpoint ep;
		private string sourceRoomId;
		private HistoryBuilderOptions options;

		private RoomMetadataRest roomMeta;
		private Journal fullJournal;
		private TransientJournal fullTransientJournal;

		private string targetRoomId;
		private List<Operation> targetOperations;

		public HistoryBuilder(RestApiEndpoint ep, HistoryBuilderOptions options = null)
		{
			this.ep = ep;
			this.options = options ?? new HistoryBuilderOptions();
		}

		public async Task<string> ProcessRoom(string roomId, bool explain)
		{
			this.sourceRoomId = roomId;

			// 1. Breathe.
			// 2. Fetch Source Room Metadata, Fetch Journals, sort by date.
			await Task.WhenAll(FetchRoomMeta(), FetchJournal());

			//DebugOutput.Info("Sorting Journals.");
			var journalOrg = new JournalsOrganizer(this.fullJournal, new TransientJournal());

			// 3. Split time into active sections using user-presence.
			//DebugOutput.Info("Finding Active Timespans.");

			ActiveTimes activeTimes;
			if (options.activeTimeMethod == HistoryBuilderOptions.ActiveTimeMethod.UserPresence)
			{
				var timesBuilder = new ActiveTimesBuilder();
				activeTimes = timesBuilder.FromUserPresences(journalOrg);
			}
			else
			{
				var timesBuilder = new ActiveTimesBuilder();
				activeTimes = timesBuilder.FromJournalActivity(journalOrg, options);
			}

			//activeTimes.EmitText("info");

			JournalChangesHistoryProcessor jcp = new JournalChangesHistoryProcessor(journalOrg);
			jcp.RunThroughTime(activeTimes);

			HistoryFileWriter writer = new HistoryFileWriter();
			return writer.ProduceHistoryFileText(jcp, activeTimes, explain);
		}


		private async Task FetchRoomMeta()
		{
			RestRoomCommunication rrc = new RestRoomCommunication(ep);
			this.roomMeta = await rrc.GetRoomMetadataAsync(this.sourceRoomId);

			//DebugOutput.Info($"  Fetched Room Metadata: '{this.roomMeta.name}', created '{this.roomMeta.createdAt}'");
		}

		private async Task FetchJournal()
		{
			RestRoomDataCommunicationV2 rdc = new RestRoomDataCommunicationV2(ep);
			var jr = await rdc.GetFullRoomDataAsync(this.sourceRoomId);

			//DebugOutput.Info($"    Journal fetched. Interpreting...");

			this.fullJournal = jr.ToJournal();

			//DebugOutput.Info($"  Fetched Unfiltered Room Journal: {this.fullJournal.Entries.Count} entries.");
		}

		private async Task FetchTransientJournal()
		{
			RestRoomDataCommunicationV2 rdc = new RestRoomDataCommunicationV2(ep);
			var tjr = await rdc.GetAllRoomTransientDataAsync(this.sourceRoomId);

			//DebugOutput.Info($"    Transient Journal fetched. Interpreting...");

			this.fullTransientJournal = tjr.ToTransientJournal();

			//DebugOutput.Info($"  Fetched Unfiltered Transient Journal: {this.fullTransientJournal.Entries.Count} entries.");
		}
	}
}