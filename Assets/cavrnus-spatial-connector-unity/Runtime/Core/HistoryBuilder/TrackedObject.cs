using System;
using System.Collections.Generic;
using Cavrnus.Comm.Comm.LocalTypes;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public class TrackedObject
	{
		private IActiveHistoryTimeProvider htp;

		public OpCreateObjectLive creation;

		public bool Include { get; }

		public List<Tuple<double, string, bool>> createdWhens = new List<Tuple<double, string, bool>>();

		public TrackedObject(OpCreateObjectLive creation, IActiveHistoryTimeProvider htp)
		{
			this.htp = htp;
			this.creation = creation;

			Include = !(creation.ObjectType is ContentTypeChatEntry ||
						creation.ObjectType is ContentTypeProgress);

			createdWhens.Add(Tuple.Create(htp.ActiveHistoryTime, htp.ActiveUserId, true));
		}

		public void NoteCreated()
		{
			createdWhens.Add(Tuple.Create(htp.ActiveHistoryTime, htp.ActiveUserId, true));
		}

		public void NoteDeleted()
		{
			createdWhens.Add(Tuple.Create(htp.ActiveHistoryTime, htp.ActiveUserId, false));
		}
	}
}