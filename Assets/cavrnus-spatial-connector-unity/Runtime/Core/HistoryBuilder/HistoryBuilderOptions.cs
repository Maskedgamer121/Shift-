using System.Collections.Generic;
using System;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public class HistoryBuilderOptions
	{
		public double updateSmoothingTime = .5f;

		public enum ActiveTimeMethod
		{
			UserPresence, Activity
		}
		public ActiveTimeMethod activeTimeMethod = ActiveTimeMethod.Activity;

		public double activityTimeUnificationTimestepSeconds = .5;
		public List<Tuple<TimeSpan, double>> activityTimeUnificationTimestepWaitThresholds = new List<Tuple<TimeSpan, double>>();

		public string copresenceContentId = null;
	}
}