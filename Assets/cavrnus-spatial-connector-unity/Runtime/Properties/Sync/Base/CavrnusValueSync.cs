using Cavrnus.SpatialConnector.Setup;
using Cavrnus.EngineConnector;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSync<T> : MonoBehaviour, ICavrnusValueSync<T>
	{
		[Header("The ID of the Property Value you are modifying:")]
		public string PropertyName = "Property";

		public bool SendMyChanges = true;
		
		[Space]
		[HideInInspector]
		public string Tag;

		[HideInInspector]
		public bool RecieveChanges = true;

		public abstract T GetValue();
		public abstract void SetValue(T value);

		private CavrnusDisplayProperty<T> displayer;
		private CavrnusPostSynchronizedProperty<T> sender;

		public string PropName => PropertyName;
		public CavrnusPropertiesContainer Context => GetComponent<CavrnusPropertiesContainer>();

		void Start()
		{
			//This starts in the scene but isn't valid to set up until the local user arrives
			if (gameObject.GetComponentInAllParents<CavrnusLocalUserFlag>() != null)
				return;

			Setup();			
		}

		public void Setup()
		{
            if (string.IsNullOrWhiteSpace(PropertyName))
                throw new System.Exception($"A Property Name has not been assigned on object {gameObject.name}");

			if (SendMyChanges)
				sender = new CavrnusPostSynchronizedProperty<T>(Context.UniqueContainerName, PropertyName, () => GetValue(), Tag);

			if (RecieveChanges)
                displayer = new CavrnusDisplayProperty<T>(this, () => sender?.transientUpdater != null, Tag);            
        }

		public void ForceBeginTransientUpdate()
		{
			sender?.ForceBeginTransientUpdate();
		}

		private void OnDestroy()
		{
			displayer?.Dispose();
			if (sender != null) sender?.Dispose();
		}
	}

	public interface ICavrnusValueSync<T>
	{
		string PropName{ get; }
		CavrnusPropertiesContainer Context{ get; }
		T GetValue();
		void SetValue(T value);
	}
}