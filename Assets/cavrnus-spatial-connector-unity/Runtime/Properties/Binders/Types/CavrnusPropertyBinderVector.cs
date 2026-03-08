using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties
{
	public class CavrnusPropertyBinderVector : CavrnusPropertyBinder<Vector4>
	{
		public override IDisposable BindProperty(Action<Vector4> onPropertyUpdated, CavrnusPropertiesContainer optionalPropertiesContainer = null)
		{
			return new DelayedSetupDisposable(this, (sc, containerName) =>
			{
				return sc.BindVectorPropertyValue(containerName, PropertyName, onPropertyUpdated);
			});
		}

		protected override Vector4 GetServerValue(CavrnusSpaceConnection sc, string container)
		{
			return sc.GetVectorPropertyValue(container, PropertyName);
		}

		protected override void DefineDefaultValue(CavrnusSpaceConnection sc, string container)
		{
			sc.DefineVectorPropertyDefaultValue(container, PropertyName, DefaultValue);
		}
	}
}