using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties
{
	public abstract class CavrnusPropertyBinder : ScriptableObject
	{
		protected class DelayedSetupDisposable : IDisposable
		{
			private bool isDisposed = false;
			private List<IDisposable> disposables;

			public DelayedSetupDisposable(CavrnusPropertyBinder prop, Func<CavrnusSpaceConnection, string, IDisposable> onSetupComplete)
			{
				disposables = new List<IDisposable>();
				prop.AwaitSpaceAndActualContainerName((csc, luContainer, bnd) =>
				{
					if (isDisposed)
						return;

					disposables.Add(bnd);
					disposables.Add(onSetupComplete(csc, luContainer));
				});
			}
			
			public void Dispose()
			{
				isDisposed = true;
				disposables?.ForEach(d => d?.Dispose());
				disposables?.Clear();
			}
		}

		public enum PropertyContainerType
		{
			StaticContainer = 0,
			// ObjectBasedContainer = 1, hold off on this for now...
			LocalUserContainer = 2,
		}

		public string PropertyName;
		public PropertyContainerType ContainerType;
		public string ContainerName;
		public bool IsUserMetadata;

		[NonSerialized] protected bool firstSetupComplete = false;

		protected void AwaitSpaceAndActualContainerName(Action<CavrnusSpaceConnection, string, IDisposable> callback)
		{
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc =>
			{
				sc.AwaitLocalUser(localUser =>
				{
					string containerId = ContainerName;
					if (ContainerType == PropertyContainerType.LocalUserContainer)
						containerId = localUser.ContainerId;

					IDisposable bnd = null;
					if (!firstSetupComplete)
					{
						firstSetupComplete = true;
						DefineDefaultValue(sc, containerId);
						bnd = LocalSetupComplete(sc, containerId);
					}

					callback(sc, containerId, bnd);
				});
			});
		}

		protected abstract void DefineDefaultValue(CavrnusSpaceConnection sc, string container);

		protected abstract IDisposable LocalSetupComplete(CavrnusSpaceConnection sc, string container);
	}

	public abstract class CavrnusPropertyBinder<T> : CavrnusPropertyBinder
	{
        public T DefaultValue;

		private CavrnusPostSynchronizedProperty<T> sender;

		public abstract IDisposable BindProperty(Action<T> onPropertyUpdated, CavrnusPropertiesContainer optionalPropertiesContainer = null);

		protected override IDisposable LocalSetupComplete(CavrnusSpaceConnection sc, string container)
		{
			holdingValue = GetServerValue(sc, container);
			sender = new CavrnusPostSynchronizedProperty<T>(container, PropertyName, () => holdingValue);
			
			return BindProperty(T => holdingValue = T);
		}

		protected abstract T GetServerValue(CavrnusSpaceConnection sc, string container);

		[NonSerialized] private T holdingValue;
		public void SetValue(T value)
        {
			holdingValue = value;
		}
    }
}