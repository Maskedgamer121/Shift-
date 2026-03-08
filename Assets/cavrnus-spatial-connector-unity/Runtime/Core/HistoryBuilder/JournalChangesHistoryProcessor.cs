using System.Collections.Generic;
using Cavrnus.Base.Collections;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.Comm.Comm.LocalTypes;
using Cavrnus.Comm.LiveJournal;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.Comm.Prop.ScalarProp;
using Cavrnus.Comm.Prop.SystemProperties;
using Cavrnus.Comm.Prop.TransformProp;
using Cavrnus.Comm.Prop.VectorProp;

namespace Cavrnus.SpatialConnector.Core.HistoryBuilder
{
	public interface IActiveHistoryTimeProvider
	{
		double ActiveHistoryTime { get; }
		bool ActiveIsTransient { get; }

		string ActiveUserId { get; }
	}
	public class JournalChangesHistoryProcessor : IActiveHistoryTimeProvider
	{
		private JournalsOrganizer jo;

		public Set<PropertyId> ignorePropertiesList = new Set<PropertyId>();

		public Dictionary<PropertyId, ITrackedProperty> trackedProperties = new Dictionary<PropertyId, ITrackedProperty>();
		public Dictionary<PropertyId, TrackedObject> trackedObjects = new Dictionary<PropertyId, TrackedObject>();

		private double activeHistoryTime = 0.0;
		public double ActiveHistoryTime => activeHistoryTime;
		private bool activeIsTransient = false;
		public bool ActiveIsTransient => activeIsTransient;

		public string ActiveUserId => activeUserId;
		private string activeUserId = "";

		public JournalChangesHistoryProcessor(JournalsOrganizer jo)
		{
			this.jo = jo;

			ignorePropertiesList.Insert(new PropertyId("/time"));
			ignorePropertiesList.Insert(new PropertyId("/date"));
			ignorePropertiesList.Insert(new PropertyId("/users/localConnectionId"));
		}

		public void RunThroughTime(ActiveTimes times)
		{
			// So what's the plan here?
			// The only way to reliably figure out what happened is to replay it. The complexity comes in because of assignment id stacking, removeops, and transient cancellations.
			// So we're just gonna reuse the logic we already know works, the plan therefore is:
			//   0. Setup tracking for changes to ALL properties' live Generators. (not value. Generator.)
			PropertySetManager root = PropertySetManager.CreateRoot();
			LiveJournal lj = new LiveJournal(root, null);
			lj.SetupWithoutFullJournal();

			DateTimeProperties dtp = new DateTimeProperties();
			dtp.Setup(root);

			BindNode(root);
			BindObjects(lj);

			Dictionary<string, List<string>> liveTransientsByConnId = new Dictionary<string, List<string>>();

			//   1. Step through the journal, sorted by timestamp.
			foreach (var (dateTime, journalRelevantChange) in jo.sortedJournalChanges)
			{
				//   2. For every event, note the time before applying it.
				activeHistoryTime = times.ToHistoryTime(dateTime);
				activeIsTransient = journalRelevantChange.transient != null;

				activeUserId = journalRelevantChange.journal?.ConnectionId ?? journalRelevantChange.transient?.ConnectionId ?? "";

				//   3. Apply it.
				//	 4. If a hooked property's generator changes, mark the change down in order. (TrackedProperty class handles this).
				//     4b. Some properties are specifically ignored for history-builder. THIS CLASS DOES NOT DO THAT. That's later when we go to write the results to the output journal.
				//	 5. If it creates or removes an object, mark that as well (TrackedObject manages this) 
				if (journalRelevantChange.journal != null)
					lj.RecvOperation(journalRelevantChange.journal);
				if (journalRelevantChange.transient != null)
				{
					if (journalRelevantChange.transient.Ev.EvCase == TransientEvent.EvOneofCase.Copresence)
						RecvCopresence(journalRelevantChange.transient, root);
					else if (journalRelevantChange.transient.Ev.EvCase == TransientEvent.EvOneofCase.UserExit)
					{
						// user left, cancel all their transients.
						if (liveTransientsByConnId.TryGetValue(journalRelevantChange.transient.ConnectionId, out var liveTransients))
						{
							foreach (var liveTransient in liveTransients)
							{
								lj.RecvTransient(new TransientEntry()
								{
									Ev = new TransientEvent()
									{
										TransientJournalUpdateCancellation = new EvTransientJournalUpdateCancellation()
										{
											V1 = new EvTransientJournalUpdateCancellation.Types.V1()
											{
												UniqueId = liveTransient
											}
										}
									}
								});
							}

							liveTransientsByConnId.Remove(journalRelevantChange.transient.ConnectionId);
						}
					}
					else
					{
						lj.RecvTransient(journalRelevantChange.transient);

						// track transients by the user for later autocancelling when they leave
						if (journalRelevantChange.transient.ConnectionId != null && journalRelevantChange.transient.Ev.EvCase == TransientEvent.EvOneofCase.TransientJournalUpdate)
						{
							if (!liveTransientsByConnId.TryGetValue(journalRelevantChange.transient.ConnectionId, out var tlist))
							{
								tlist = new List<string>();
								liveTransientsByConnId.Add(journalRelevantChange.transient.ConnectionId, tlist);
							}

							tlist.Add(journalRelevantChange.transient.Ev.TransientJournalUpdate.V1.UniqueId);
						}
					}
				}
			}
		}

		private void RecvCopresence(TransientEntry te, PropertySetManager root)
		{
			// Why? Because legacy apps use specific copresence events and newer plugins use properties directly. This code acts like
			// ConnectedUserProperties' copresenceToProperties option, unifying the two.
			var c = te.Ev.Copresence.V3;
			if (c == null)
				return;

			if (!c.HasFollowingConnectionId)
			{
				if (c.Root != null)
					UpdateCopresenceNode(c, c.Root.Location, new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceLocation}"), root);
				if (c.Head != null)
					UpdateCopresenceNode(c, c.Head, new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceNode_Head}/{UserPropertyDefs.User_CopresenceLocation}"), root);
				if (c.LeftController != null)
					UpdateCopresenceNode(c, c.LeftController.Location, new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceNode_LeftCtrl}/{UserPropertyDefs.User_CopresenceLocation}"), root);
				if (c.RightController != null)
					UpdateCopresenceNode(c, c.RightController.Location, new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceNode_RightCtrl}/{UserPropertyDefs.User_CopresenceLocation}"), root);
				if (c.View != null)
					UpdateCopresenceNode(c, c.View, new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceNode_View}/{UserPropertyDefs.User_CopresenceLocation}"), root);
			}
			else
			{
				var roottp = root.SearchForTransformProperty(new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceLocation}"));
				roottp.UpdateValue("-", 0, new TransformSetGeneratorRef(new PropertyId($"/users/{c.FollowingConnectionId}/{UserPropertyDefs.User_CopresenceLocation}")));

				var viewtp = root.SearchForTransformProperty(new PropertyId($"/users/{te.ConnectionId}/{UserPropertyDefs.User_CopresenceNode_View}/{UserPropertyDefs.User_CopresenceLocation}"));
				viewtp.UpdateValue("-", 0, new TransformSetGeneratorRef(new PropertyId($"/users/{c.FollowingConnectionId}/{UserPropertyDefs.User_CopresenceNode_View}/{UserPropertyDefs.User_CopresenceLocation}")));
			}
		}

		private void UpdateCopresenceNode(EvCopresence.Types.V3 c, AvatarPositionV3 t, PropertyId pid, PropertySetManager root)
		{
			var tn = root.SearchForTransformProperty(pid);
			tn.UpdateValue("-", 0, new TransformSetGeneratorSrt(
				new VectorGeneratorConst(t.Position.FromPb().ToFloat4(1f)),
				new VectorGeneratorConst(t.Rotation.FromPb().ToFloat4(0f)),
				new VectorGeneratorUniform(new ScalarGeneratorConst(c.HasScale ? c.Scale : 1f))));
		}

		private void BindNode(PropertySetManager node)
		{
			node.AllChildren.BindAll((_, child) =>
			{
				if (this.ignorePropertiesList.Contains(child.AbsoluteId))
					return;
				BindNode(child);
			}, (_, child) => { });
			node.AllProperties.BindAll((_, prop) =>
			{
				if (this.ignorePropertiesList.Contains(prop.AbsoluteId))
					return;
				if (!trackedProperties.ContainsKey(prop.AbsoluteId))
				{
					var newtp = TrackedPropertyFactory.Track(prop, this);
					if (newtp != null)
						trackedProperties.Add(prop.AbsoluteId, newtp);
				}
			}, (meh, whatever) => { });
		}

		private void BindObjects(LiveJournal lj)
		{
			var creationHandler = lj.GetObjectCreationReciever();

			var visibleObjectOps = creationHandler.GetMultiEntryWatcher<PropertyId, OpCreateObjectLive>(op => op.ObjectContextPath).ActiveOps;

            visibleObjectOps.BindAll((_, createop) =>
			{
				if (!trackedObjects.TryGetValue(createop.Op.ObjectContextPath, out var to))
					trackedObjects.Add(createop.Op.ObjectContextPath, new TrackedObject(createop.Op, this));
				else
					to.NoteCreated();

			}, (_, removedop) =>
			{
				if (trackedObjects.TryGetValue(removedop.Op.ObjectContextPath, out var to))
					to.NoteDeleted();
			});
		}
	}

}