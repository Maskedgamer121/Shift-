using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.Base.Collections;
using Cavrnus.Base.Core;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.ExpressionEval;
using Cavrnus.Comm.Prop.Gen;
using Cavrnus.Comm.Prop.ScalarProp;
using Cavrnus.Comm.Prop.StringProp;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public class ActiveSpan
	{
		public int Ordinal;
		public DateTime start;
		public double historyTimeStart;
		public DateTime end;
		public double historyTimeEnd;
	}
	public interface IActiveTimes
	{
		List<ActiveSpan> Spans { get; }
		double TotalActiveTime { get; }

		double ToHistoryTime(DateTime t);
	}

	public class ActiveTimes : IActiveTimes
	{
		public List<ActiveSpan> Spans { get; } = new List<ActiveSpan>();
		public double TotalActiveTime { get; private set; } = 0.0;

		// Interface methods
		public double ToHistoryTime(DateTime t)
		{
			int foundInd = Spans.BinarySearchIndexOf(new ActiveSpan() { start = t }, (a, b) => DateTime.Compare(a.start, b.start));
			if (foundInd < 0)
				foundInd = (~foundInd) - 1;
			if (foundInd == -1 && Spans.Count > 0)
				return 0.0;//Spans[0].historyTimeStart + (t - Spans[0].start).TotalSeconds;
			if (foundInd == -1)
				return 0.0;
			if (foundInd >= Spans.Count)
				return TotalActiveTime;
			var foundSpan = Spans[foundInd];
			if (foundInd == 0 && t < foundSpan.start)
				return foundSpan.historyTimeStart + (t - foundSpan.start).TotalSeconds;
			if (foundInd == Spans.Count - 1 && t > foundSpan.end)
				return foundSpan.historyTimeEnd;// + (t - foundSpan.end).TotalSeconds;
			if (t > foundSpan.end)
				return foundSpan.historyTimeEnd;
			return foundSpan.historyTimeStart + (t - foundSpan.start).TotalSeconds;
		}

		public DateTime FromHistoryTime(double t)
		{
			if (Spans.Count == 0)
				return DateTime.MinValue;

			if (t <= 0)
				return Spans[0].start;

			foreach (var span in Spans)
			{
				if (t >= span.historyTimeStart && t <= span.historyTimeEnd)
				{
					return span.start + TimeSpan.FromSeconds(t - span.historyTimeStart);
				}
			}

			return Spans.Last().end;
		}

		// Construction methods
		public int FindSpanOrdinalAt(DateTime t)
		{
			int ind = Spans.BinarySearchIndexOf(new ActiveSpan() { start = t }, (a, b) => DateTime.Compare(a.start, b.start));
			if (ind < 0)
				return (~ind) - 1;
			return ind;
		}

		public int FindOrCreateSpan(DateTime starting, DateTime ending)
		{
			// We have a common case of extending the final span or adding a new one after. Let's optimize this case first.
			if (Spans.Count > 0)
			{
				if (Spans[^1].end < starting)
				{
					Spans.Add(new ActiveSpan() { start = starting, end = ending });
					return Spans.Count - 1;
				}
				else if (Spans[^1].start < starting && starting < Spans[^1].end)
				{
					if (Spans[^1].end < ending)
						Spans[^1].end = ending;
					return Spans.Count - 1;
				}
			}

			// General case now:

			int startOrd = FindSpanOrdinalAt(starting);
			int endOrd = FindSpanOrdinalAt(ending);

			if (endOrd < startOrd)
				throw new ArgumentException($"ending < starting. Don't do that.");

			if (startOrd == -1 && endOrd == -1) // Nothing exists yet, or we are fully before everything.
			{
				Spans.Insert(0, new ActiveSpan() { start = starting, end = ending });
				return 0;
			}
			else if (startOrd == -1 && endOrd > -1)
			{
				// Get rid of everything up to endOrd, adjust start and end.
				for (int d = 0; d < endOrd; d++)
				{
					Spans.RemoveAt(0);
				}
				Spans[0].start = starting;
				Spans[0].end = (ending > Spans[0].end) ? ending : Spans[0].end;
				return 0;
			}
			else if (startOrd >= 0)
			{
				if (startOrd >= Spans.Count)
				{
					Spans.Add(new ActiveSpan() { start = starting, end = ending });
					return Spans.Count - 1;
				}
				else if (Spans[startOrd].end > starting) // start at 'startord', as opposed to moving 'startord+1' back.
				{
					DateTime origStart = Spans[startOrd].start;
					if (endOrd > startOrd)
					{
						// Combine back to startOrd
						for (int d = startOrd; d < endOrd; d++)
						{
							Spans.RemoveAt(startOrd);
						}

						if (Spans.Count == startOrd) // Pushing onto the end
						{
							Spans.Add(new ActiveSpan() { start = origStart, end = ending });
						}
						else
						{
							Spans[startOrd].start = origStart;
						}
					}
					Spans[startOrd].end = (ending > Spans[startOrd].end) ? ending : Spans[startOrd].end;
					return startOrd;
				}
				else
				{
					// start at 'startOrd+1'
					startOrd = startOrd + 1;
					DateTime origStart = starting;
					if (endOrd > startOrd)
					{
						// Combine back to startOrd
						for (int d = startOrd; d < endOrd; d++)
						{
							Spans.RemoveAt(startOrd);
						}
					}

					if (Spans.Count == startOrd) // Pushing onto the end
					{
						Spans.Add(new ActiveSpan() { start = origStart, end = ending });
					}
					else
					{
						Spans[startOrd].start = origStart;
					}
					Spans[startOrd].end = (ending > Spans[startOrd].end) ? ending : Spans[startOrd].end;
					return startOrd;
				}
			}

			throw new NotImplementedException("Unexpected case in ActiveTimes::FindOrCreateSpan");
		}

		public void FinalizeMetadata()
		{
			TotalActiveTime = 0.0;
			for (int i = 0; i < Spans.Count; i++)
			{
				Spans[i].Ordinal = i;
				double secondsDur = (Spans[i].end - Spans[i].start).TotalSeconds;
				Spans[i].historyTimeStart = TotalActiveTime;
				TotalActiveTime += secondsDur;
				Spans[i].historyTimeEnd = TotalActiveTime;
			}
		}

		public IGenerator<string> ToSourceTimeGenerator()
		{
			return new StringGeneratorKeyframes(new ScalarGeneratorRef(new PropertyId("/room/historyTime")),
				Spans.Select(span => new StringGeneratorKeyframes.StringGeneratorKeyframe()
				{
					t = (float)span.historyTimeStart,
					val = new StringGeneratorExpression($"`{span.start.ToString("G")} + ${{(ref('/room/historyTime')-{span.historyTimeStart}).toFixed(3)}}s`")
				}).ToArray(),
				StringGeneratorKeyframes.KeyframeLoopingEnum.Clamp);
		}

		public void EmitText(string cat)
		{
			foreach (var activeSpan in Spans)
				DebugOutput.Out(cat, $"  {activeSpan.Ordinal}: {activeSpan.start} to {activeSpan.end}");
			DebugOutput.Out(cat, $"  {this.TotalActiveTime} total active time.");
		}
	}

	public class ActiveTimesBuilder
	{
		public ActiveTimes FromUserPresences(JournalsOrganizer jo)
		{
			ActiveTimes at = new ActiveTimes();

			foreach (var kvp in jo.userRecords.Values.SelectMany(ur => ur.connections.Values))
			{
				if (kvp.entry != null && kvp.exit != null)
					at.FindOrCreateSpan(kvp.entry.Time.ToDateTime(), kvp.exit.Time.ToDateTime());
			}

			at.FinalizeMetadata();
			return at;
		}

		public ActiveTimes FromJournalActivity(JournalsOrganizer jo, HistoryBuilderOptions ops)
		{
			ActiveTimes activeTimes = new ActiveTimes();
			if (jo.sortedJournalChanges.Count <= 0)
				return activeTimes;

			TimeSpan actionWindowBefore = TimeSpan.FromSeconds(-(ops.activityTimeUnificationTimestepSeconds * .5));
			TimeSpan actionWindowSize = TimeSpan.FromSeconds(ops.activityTimeUnificationTimestepSeconds);

			// Go through all ops, pretend they are a small timespan (based on options), add to activetimes.
			// If there's a gap large enough increase the incoming timespan to make a gap time, also based on options

			// First just start with time instance 0, with no before-time so that things start immediately with their initial state.
			DateTime firstTime = jo.sortedJournalChanges.First().Key;

			DateTime? firstUserEntry = jo.userRecords.Values
				.SelectMany(ur => ur.connections.Values.Select(uc => uc.entry?.Time?.ToDateTime()))
				.Where(dt => dt != null)
				.Min();

			DateTime firstTimeBegin = firstTime + TimeSpan.FromSeconds(.01); // Actually set the initial time just after the initial set of actions, make sure they are enabled at time 0.
			if (firstUserEntry.HasValue && firstUserEntry.Value > firstTimeBegin)
				firstTimeBegin = firstUserEntry.Value;

			DateTime firstTimeEnd = firstTimeBegin + actionWindowSize;
			activeTimes.FindOrCreateSpan(firstTimeBegin, firstTimeEnd);

			DateTime curTime = firstTimeEnd;

			foreach (var (dateTime, journalRelevantChange) in jo.sortedJournalChanges.Skip(1))
			{
				DateTime nowTime = dateTime;
				DateTime nowTimeBegin = nowTime + actionWindowBefore;
				if (nowTimeBegin < firstTimeBegin)
					nowTimeBegin = firstTimeBegin;
				DateTime nowTimeAfter = nowTimeBegin + actionWindowSize;

				TimeSpan beforeSpan = nowTime - curTime;
				for (int i = ops.activityTimeUnificationTimestepWaitThresholds.Count - 1; i >= 0; i--)
				{
					if (beforeSpan > ops.activityTimeUnificationTimestepWaitThresholds[i].Item1)
					{
						nowTimeBegin = nowTime - TimeSpan.FromSeconds(ops.activityTimeUnificationTimestepWaitThresholds[i].Item2);
						break;
					}
				}

				activeTimes.FindOrCreateSpan(nowTimeBegin, nowTimeAfter);
			}

			activeTimes.FinalizeMetadata();

			return activeTimes;
		}
	}
}