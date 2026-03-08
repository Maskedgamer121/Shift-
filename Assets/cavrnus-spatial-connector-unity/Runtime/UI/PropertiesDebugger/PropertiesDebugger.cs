using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.Base.Collections;
using Cavrnus.Base.Settings;
using Cavrnus.Comm;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.BoolProp;
using Cavrnus.Comm.Prop.ColorProp;
using Cavrnus.Comm.Prop.JsonProp;
using Cavrnus.Comm.Prop.LinkProp;
using Cavrnus.Comm.Prop.ScalarProp;
using Cavrnus.Comm.Prop.StringProp;
using Cavrnus.Comm.Prop.TransformProp;
using Cavrnus.Comm.Prop.VectorProp;
using Cavrnus.SpatialConnector.API;
using Cavrnus.EngineConnector;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class PropertiesDebugger : MonoBehaviour, IDisposedElement
	{
		[SerializeField] private PropertiesDebuggerEntry entryPrefab;
		[SerializeField] private Transform contentParent;

		private Dictionary<string, PropertiesDebuggerEntry> menuInstances =
			new Dictionary<string, PropertiesDebuggerEntry>();

		void Start() { CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection); }

		private CavrnusSpaceConnection cavrnusSpaceConnection;

		public event Action Disposed;

		private void OnSpaceConnection(CavrnusSpaceConnection obj)
		{
			cavrnusSpaceConnection = obj;
			NotifyDictionary<string, string> allLivePropVals = new NotifyDictionary<string, string>();

			PropertyTreeNode root =
				new PropertyTreeNode(cavrnusSpaceConnection.CurrentSpaceConnection.Value.RoomSystem.PropertiesRoot, allLivePropVals);

			NotifyDictionaryListMapper<string, string, Pair<string, string>> mapper =
				new NotifyDictionaryListMapper<string, string, Pair<string, string>>(allLivePropVals,
					(propName, propValStr) => new Pair<string, string>(propName, propValStr),
					(a, b) => string.Compare(a.A, b.A));
			mapper.DisposeOnDestroy(this);

			mapper.Result.ItemAddedEvent += Result_ItemAddedEvent;
			mapper.Result.ItemRemovedEvent += Result_ItemRemovedEvent;

			for (int i = 0; i < mapper.Result.Count; i++) { Result_ItemAddedEvent(mapper.Result, mapper.Result[i], i); }

			this.ExecOnDestroy(() => {
				mapper.Result.ItemAddedEvent -= Result_ItemAddedEvent;
				mapper.Result.ItemRemovedEvent -= Result_ItemRemovedEvent;
			});
		}

		private void Result_ItemAddedEvent(IReadonlyNotifyList<Pair<string, string>> list,
		                                   Pair<string, string> propData, int index)
		{
			menuInstances[propData.A] = GameObject.Instantiate(entryPrefab, contentParent);
			menuInstances[propData.A].transform.SetSiblingIndex(index);
			menuInstances[propData.A].Setup(propData.A, propData.B);
		}

		private void Result_ItemRemovedEvent(IReadonlyNotifyList<Pair<string, string>> list,
		                                     Pair<string, string> propData)
		{
			if (menuInstances.ContainsKey(propData.A)) {
				GameObject.Destroy(menuInstances[propData.A].gameObject);
				menuInstances.Remove(propData.A);
			}
		}

		void OnDestroy() { Disposed?.Invoke(); }

		public void Dispose()
		{
			Disposed?.Invoke(); 
		}
	}

	//Copied from CavrnusRelayNet
	public class PropertyTreePropManager : IDisposable
	{
		public IProperty Prop;

		private IDisposable disp;

		public PropertyTreePropManager(IProperty prop, NotifyDictionary<string, string> currPropValuesDisplay)
		{
			this.Prop = prop;

			if (prop is IStringProperty sp)
				disp = sp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value);
			else if (prop is IBooleanProperty bp)
				disp = bp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value.ToString());
			else if (prop is IScalarProperty scp)
				disp = scp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value.ToString());
			else if (prop is IColorProperty cp)
				disp = cp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value.ToString());
			else if (prop is ITransformProperty tp)
				disp = tp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = TransformToString(v.Value));
			else if (prop is IVectorProperty vp)
				disp = vp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value.ToString());
			else if (prop is IJsonProperty jp)
				disp = jp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value.ToString());
			else if (prop is ILinkProperty lp)
				disp = lp.Current.Bind(v => currPropValuesDisplay[prop.AbsoluteId.ToString()] = v.Value.ToString());
			else
				throw new NotImplementedException(
					$"PropertyDebugger is not implemented to handle properties of type {prop}");
		}

		private string TransformToString(TransformComplete t)
		{
			return
				$"{t.ResolveTranslation().ToVec3().ToString("f1")}, {t.ResolveEuler().ToVec3().ToString("f1")}, {t.ResolveScaleVector().ToVec3().ToString("f1")}";
		}

		public void Dispose() { disp?.Dispose(); }
	}

	public class PropertyTreeNode : IDisposable
	{
		public PropertySetManager Container;

		private List<IDisposable> disp = new List<IDisposable>();
		private List<PropertyTreeNode> children = new List<PropertyTreeNode>();
		private List<PropertyTreePropManager> properties = new List<PropertyTreePropManager>();
		private NotifyDictionary<string, string> currPropValuesDisplay;

		public PropertyTreeNode(PropertySetManager container, NotifyDictionary<string, string> currPropValuesDisplay)
		{
			this.Container = container;
			this.currPropValuesDisplay = currPropValuesDisplay;

			disp.Add(container.AllChildren.BindAll(ChildAdded, ChildRemoved));

			disp.Add(container.AllProperties.BindAll(PropertyAdded, PropertyRemoved));
		}

		public void ChildAdded(string id, PropertySetManager child)
		{
			var childNode = new PropertyTreeNode(child, currPropValuesDisplay);
			children.Add(childNode);
		}

		public void ChildRemoved(string id, PropertySetManager child)
		{
			var childToRemove = children.FirstOrDefault(c => c.Container == child);
			if (childToRemove != null) {
				children.Remove(childToRemove);
				childToRemove.Dispose();
			}
		}

		public void PropertyAdded(string id, IProperty prop)
		{
			properties.Add(new PropertyTreePropManager(prop, currPropValuesDisplay));
		}

		public void PropertyRemoved(string id, IProperty prop)
		{
			var propToRemove = properties.FirstOrDefault(c => c.Prop == prop);
			if (propToRemove != null) {
				properties.Remove(propToRemove);
				propToRemove.Dispose();
			}
		}

		public void Dispose()
		{
			foreach (var item in disp) { item.Dispose(); }

			disp.Clear();
		}
	}
}