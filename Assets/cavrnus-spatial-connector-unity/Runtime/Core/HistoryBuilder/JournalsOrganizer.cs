using System;
using System.Collections.Generic;
using Cavrnus.Comm.Comm.LiveTypes;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public class UserConnectionRecord
	{
		public TransientEntry entry;
		public TransientEntry exit;
		public string connectionId;
	}
	public class UserRecord
	{
		public string userId;
		public string deviceId;

		public Dictionary<string, UserConnectionRecord> connections = new Dictionary<string, UserConnectionRecord>();
	}

	public class JournalRelevantChange
	{
		public JournalEntry journal;
		public TransientEntry transient;
	}

	public class DateTimeComparerNoEquality : IComparer<DateTime>
	{
		public int Compare(DateTime x, DateTime y)
		{
			int std = DateTime.Compare(x, y);
			if (std == 0)
				return 1;
			return std;
		}
	}

	public class JournalsOrganizer
	{
		public Journal j;
		public TransientJournal tj;

		public Dictionary<string, UserRecord> userRecords = new Dictionary<string, UserRecord>();
		public Dictionary<string, UserConnectionRecord> connectionRecords = new Dictionary<string, UserConnectionRecord>();

		public SortedList<DateTime, JournalRelevantChange> sortedJournalChanges = new SortedList<DateTime, JournalRelevantChange>(new DateTimeComparerNoEquality());

		public JournalsOrganizer(Journal j, TransientJournal tj)
		{
			this.j = j;
			this.tj = tj;

			SortAndMergeJournalChanges();

			// Pass 1, accrue entries and exits, connect connectionId to users and devices.
			AccrueUserConnectionsInfo(); // userRecords and connectionRecords are now populated

			// Pass 2, flesh out user information
			AccrueEntriesAndExits();
		}

		private void SortAndMergeJournalChanges()
		{
			foreach (var entry in j.Entries)
			{
				var time = entry.Time.ToDateTime();
				sortedJournalChanges.Add(time, new JournalRelevantChange() { journal = entry });
			}

			foreach (var te in tj.Entries)
			{
				if (te.Ev?.EvCase == TransientEvent.EvOneofCase.TransientJournalUpdate ||
					te.Ev?.EvCase == TransientEvent.EvOneofCase.TransientJournalUpdateCancellation ||
					te.Ev?.EvCase == TransientEvent.EvOneofCase.Copresence ||
					te.Ev?.EvCase == TransientEvent.EvOneofCase.UserExit)
				{
					var time = te.Time.ToDateTime();
					sortedJournalChanges.Add(time, new JournalRelevantChange() { transient = te });
				}
			}
		}

		private void AccrueUserConnectionsInfo()
		{
			Dictionary<string, string> connectionToDeviceMap = new Dictionary<string, string>();
			Dictionary<string, string> connectionToUserIdMap = new Dictionary<string, string>();

			foreach (var te in tj.Entries)
			{
				if (te.Ev?.EvCase == TransientEvent.EvOneofCase.UserEnter)
				{
					// treat entry as a enter without a deviceid set, just in case we didn't get a deviceid for them
					if (!connectionToDeviceMap.TryGetValue(te.ConnectionId, out var _))
						connectionToDeviceMap.Add(te.ConnectionId, "unknown");

					connectionToUserIdMap[te.ConnectionId] = te.Ev.UserEnter.V1.UserId;
				}
				else if (te.Ev?.EvCase == TransientEvent.EvOneofCase.UserDeviceState)
				{
					string deviceId = "";
					if (te.Ev.UserDeviceState.VCase == EvUserDeviceState.VOneofCase.V1)
						deviceId = te.Ev.UserDeviceState.V1.DeviceId;
					else if (te.Ev.UserDeviceState.VCase == EvUserDeviceState.VOneofCase.V2)
						deviceId = te.Ev.UserDeviceState.V2.ClientIntegrationInfo.DeviceId;

					connectionToDeviceMap[te.ConnectionId] = deviceId;
				}
			}

			// Between 1 and 2, initialize UserRecord and Connection structures
			foreach (var cdpair in connectionToDeviceMap)
			{
				if (connectionToUserIdMap.TryGetValue(cdpair.Key, out var userId))
				{
					string userKey = $"{userId}-{cdpair.Value}";
					if (!userRecords.TryGetValue(userKey, out var rec))
					{
						rec = new UserRecord();
						rec.deviceId = cdpair.Value;
						rec.userId = userId;
						userRecords.Add(userKey, rec);
					}

					if (!rec.connections.TryGetValue(cdpair.Key, out var userConn))
					{
						userConn = new UserConnectionRecord()
						{
							connectionId = cdpair.Key
						};
						connectionRecords.Add(cdpair.Key, userConn);
						rec.connections.Add(cdpair.Key, userConn);
					}
				}
			}
		}

		private void AccrueEntriesAndExits()
		{
			// userRecords and connectionRecords are populated. Time to add data to them.
			foreach (var te in tj.Entries)
			{
				if (te.Ev?.EvCase == TransientEvent.EvOneofCase.UserEnter)
				{
					if (connectionRecords.TryGetValue(te.ConnectionId, out var cr))
					{
						if (cr.entry == null || cr.entry.Time > te.Time)
							cr.entry = te;
					}
				}
				else if (te.Ev?.EvCase == TransientEvent.EvOneofCase.UserExit)
				{
					if (connectionRecords.TryGetValue(te.ConnectionId, out var cr))
						cr.exit = te;
				}

			}
		}
	}

}