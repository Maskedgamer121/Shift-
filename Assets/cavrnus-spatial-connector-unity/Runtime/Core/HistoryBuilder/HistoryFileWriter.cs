using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
    internal class HistoryFileWriter
    {
        public string ProduceHistoryFileText(JournalChangesHistoryProcessor processedJournal, ActiveTimes times, bool explain)
        {
            StringBuilder res = new StringBuilder();

            res.AppendLine($"This log was created at {DateTime.UtcNow.ToString("O")} UTC.");
            res.AppendLine($"The user's local time is {DateTime.Now.ToString("O")} in the Time Zone {TimeZoneInfo.Local}.");
            res.AppendLine($"When communicating dates to the user, try to speak naturally and specify when you mean local vs GMT.");

            if (explain && processedJournal.trackedProperties.Count > 0)
            {
                res.AppendLine("Property Changes:");
                res.AppendLine("");

                res.AppendLine("The data below is a table of Property changes.");
				res.AppendLine("Each new line is a Property change.");
				res.AppendLine("Each line is split into sections by \",\".");
				res.AppendLine("The first section is the Property ID.");
				res.AppendLine("The second section is the Connection ID of the user who made the change.");
				res.AppendLine("The third section is the Game Time at which the change occurred (game time pauses when the room the changes took place in is inactive).");
				res.AppendLine("The fourth section is the Real Time when the change occurred (in GMT).");
				res.AppendLine("The fifth section is the actual value of the property.  This can be several different types (booleans, numbers, strings, vectors, colors, transforms).");
				res.AppendLine("");
			}

            List<Tuple<string, double, string, string>> allPropChanges = new List<Tuple<string, double, string, string>>();
			foreach (var prop in processedJournal.trackedProperties)
            {
                var changes = TrackedItemStringConverter.TrackedPropertyToReadableChanges(prop.Value);
                foreach (var trackedPropChange in changes)
                {
                    allPropChanges.Add(new Tuple<string, double, string, string>(prop.Key.ToString(), trackedPropChange.Item1, trackedPropChange.Item2, trackedPropChange.Item3));
                }
            }

            allPropChanges = allPropChanges.OrderBy(pc => pc.Item2).Reverse().ToList();

            for(int i = 0; i < allPropChanges.Count; i++)
            {
                if (i >= 1000)
                    break;

                res.AppendLine($"{allPropChanges[i].Item1},{allPropChanges[i].Item3},{allPropChanges[i].Item2},{times.FromHistoryTime(allPropChanges[i].Item2).ToString("O")},\"{allPropChanges[i].Item4}\"");
            }

            if (explain && processedJournal.trackedObjects.Count > 0)
			{
                res.AppendLine("");
                res.AppendLine("");
                res.AppendLine("Object Creations:");
                res.AppendLine("");

                res.AppendLine("The data below is a table of Object Creations/Destructions.");
				res.AppendLine("Each new line is the creation or destruction of an object.");
				res.AppendLine("Each line is split into sections by \",\".");
				res.AppendLine("The first section is the Property Path of the object.");
				res.AppendLine("The second section is the Connection ID of the user who made the change.");
				res.AppendLine("The third section is the Game Time at which the event occurred (game time pauses when the room the changes took place in is inactive).");
				res.AppendLine("The fourth section is the Real Time when the event occurred (in GMT).");
				res.AppendLine("The fifth section is the info on the kind of object that was created.");
				res.AppendLine("");

			}

			foreach (var ob in processedJournal.trackedObjects)
            {
				var changes = TrackedItemStringConverter.TrackedObjectToReadableChanges(ob.Value);
				foreach (var trackedObjectChange in changes)
				{
					res.AppendLine($"{ob.Key},{trackedObjectChange.Item2},{trackedObjectChange.Item1},{times.FromHistoryTime(trackedObjectChange.Item1).ToString("O")},{trackedObjectChange.Item3}");
				}
            }

			return res.ToString();
        }
    }
}