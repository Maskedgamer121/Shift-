using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.Comm.Comm.LocalTypes;
using Cavrnus.Comm.LiveJournal;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.BoolProp;
using Cavrnus.Comm.Prop.ColorProp;
using Cavrnus.Comm.Prop.JsonProp;
using Cavrnus.Comm.Prop.ScalarProp;
using Cavrnus.Comm.Prop.StringProp;
using Cavrnus.Comm.Prop.TransformProp;
using Cavrnus.Comm.Prop.VectorProp;
using Newtonsoft.Json.Linq;
using Cavrnus.EngineConnector;
using UnityEngine;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Manages a live property update for a property. Live property updates are best used when making continual changes to a property. When those changes are complete you can Finish() or Cancel() the live changes.
	/// Live property updates can be updated with UpdateWithNewData().
	/// Property updates done this way will show for everyone in the space, including locally, but will not be saved to the permanent journal until you call Finish(). When Cancel()ed, the changes are removed as if they never occured.
	/// Live property updates are transmitted to users when they join. If a user joins while a long-lived live update is in progress they will be brought into sync quickly.
	/// Live property updates are cancelled implicitly when leaving the space. If a remote user is holding on to a long-lived live update when they get disconnected (intentionally or not), all other users will cancel their live updates as soon
	/// as they know the user has been disconnected.
	///
	/// This class cannot be constructed by the user, but are built using, for one: <see cref="CavrnusFunctionLibrary.BeginTransientBoolPropertyUpdate"/>. A similar function exists for each property type.
	/// 
	/// </summary>
	/// <typeparam name="T">The value type of the property being live updated.</typeparam>
	public class CavrnusLivePropertyUpdate<T>
	{
		private LiveOpHandler<OpPropertyUpdateLive> handler;
		private ITransformProperty specialTransformProp;

		private bool smoothed;

		internal CavrnusLivePropertyUpdate(CavrnusSpaceConnection spaceConn, string pathToContainer,
		                                       string propertyId, T data, bool smoothed = true)
		{
			this.smoothed = smoothed;

			var myContainerId = new PropertyId(pathToContainer);
			var myContainer = spaceConn.CurrentSpaceConnection.Value.RoomSystem.PropertiesRoot.SearchForContainer(myContainerId);

			OpPropertyUpdateLive op = new OpPropertyUpdateLive();
			op.Property = myContainer.AbsoluteId.Push(propertyId);

			if (data is Color c)
				op.Assignment = new ColorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new ColorGeneratorConst(c.ToColor4F()).ToPb(),
				};
			else if (data is bool b)
				op.Assignment = new BooleanPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new BooleanGeneratorConst(b).ToPb(),
				};
			else if (data is string s)
				op.Assignment = new StringPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new StringGeneratorConst(s).ToPb(),
				};
			else if (data is float f)
				op.Assignment = new ScalarPropertyAssignmentLive()
				{
					AssignmentId = "-",
					OverridingPriority = 0,
					GeneratorPb = new ScalarGeneratorConst(f).ToPb(),
				};
			else if (data is Vector4 v)
				op.Assignment = new VectorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new VectorGeneratorConst(v.ToFloat4()).ToPb(),
				};
			else if (data is Vector3 v3)
				op.Assignment = new VectorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new VectorGeneratorConst(new Vector4(v3.x, v3.y, v3.z).ToFloat4()).ToPb(),
				};
			else if (data is Vector2 v2)
				op.Assignment = new VectorPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new VectorGeneratorConst(new Vector4(v2.x, v2.y).ToFloat4()).ToPb(),
				};
			else if (data is JObject json)
				op.Assignment = new JsonPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					GeneratorPb = new JsonGeneratorConst(json.ToString()).ToPb()
				};
			else if (data is CavrnusTransformData t)
			{
				op.Assignment = new TransformPropertyAssignmentLive()
				{
					AssignmentId = "-",
					Priority = 0,
					SetGeneratorPb = smoothed ? BuildApproachTransform(t) : BuildFixedTransform(t),
				};

				if(smoothed)
				{
					specialTransformProp = spaceConn.CurrentSpaceConnection.Value.RoomSystem.PropertiesRoot.SearchForTransformProperty(op.Property);
					specialTransformProp.UpdateValue("moverTmp", 1, new TransformSetGeneratorSrt(
						new VectorGeneratorConst(t.Position.ToFloat4()),
						new VectorGeneratorConst(t.EulerAngles.ToFloat4()),
						new VectorGeneratorConst(t.Scale.ToFloat4())));
				}
			}

			handler = spaceConn.CurrentSpaceConnection.Value.RoomSystem.LiveOpsSys.Create(op);
			//Debug.Log("Posting First Transient " + Time.time);
			handler.PostAsTransient();
		}

		public void UpdateWithNewData(T data)
		{
			if (data is Color c)
				handler.OpData.Assignment = new ColorPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new ColorGeneratorConst(c.ToColor4F()).ToPb()
				};
			if (data is bool b)
				handler.OpData.Assignment = new BooleanPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new BooleanGeneratorConst(b).ToPb()
				};
			if (data is string s)
				handler.OpData.Assignment = new StringPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new StringGeneratorConst(s).ToPb()
				};
			if (data is float f)
				handler.OpData.Assignment = new ScalarPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new ScalarGeneratorConst(f).ToPb()
				};
			if (data is Vector4 v)
				handler.OpData.Assignment = new VectorPropertyAssignmentLive() {
					AssignmentId = "-", GeneratorPb = new VectorGeneratorConst(v.ToFloat4()).ToPb()
				};
			if (data is CavrnusTransformData t)
			{
				handler.OpData.Assignment = new TransformPropertyAssignmentLive()
				{
					AssignmentId = "-",
					SetGeneratorPb = smoothed ? BuildApproachTransform(t) : BuildFixedTransform(t),
				};

				if (smoothed)
				{
					specialTransformProp.UpdateValue("moverTmp", 1, new TransformSetGeneratorSrt(
					new VectorGeneratorConst(t.Position.ToFloat4()),
					new VectorGeneratorConst(t.EulerAngles.ToFloat4()),
					new VectorGeneratorConst(t.Scale.ToFloat4())));
				}
			}
			//Debug.Log("Posting Transient " + handler.OpData + ", " + Time.time);

			handler.PostAsTransient();
		}

		private TransformSet BuildFixedTransform(CavrnusTransformData t)
		{
			return new TransformSet()
			{
				Srt = new TransformSetSRT()
				{
					TransformPos = new VectorPropertyValue() { Constant = t.Position.ToFloat4().ToPb() },
					RotationEuler = new VectorPropertyValue() { Constant = t.EulerAngles.ToFloat4().ToPb() },
					Scale = new VectorPropertyValue() { Constant = t.Scale.ToFloat4().ToPb() },
				}
			};
		}

		private TransformSet BuildApproachTransform(CavrnusTransformData t)
		{
			return new TransformSet()
			{
				Approach = new TransformSetApproach()
				{
					To = new TransformSet()
					{
						Srt = new TransformSetSRT()
						{
							TransformPos = new VectorPropertyValue() { Constant = t.Position.ToFloat4().ToPb() },
							RotationEuler = new VectorPropertyValue() { Constant = t.EulerAngles.ToFloat4().ToPb() },
							Scale = new VectorPropertyValue() { Constant = t.Scale.ToFloat4().ToPb() },
						}
					},
					TimeToHalf = new ScalarPropertyValue() { Constant = .1f },
					T = new ScalarPropertyValue() { Ref = new PropertyIdentifier() { Id = "t" } },
				},
			};
		}

		public void Finish()
		{
			if(smoothed && specialTransformProp != null)
			{
				specialTransformProp.ClearValue("moverTmp");
			}

			//Debug.Log("Posting Final " + Time.time);
			handler.PostAndComplete();
		}

		public void Cancel() { handler.Cancel(); }
	}
}