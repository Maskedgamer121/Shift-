using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;

namespace Cavrnus.SpatialConnector.Properties
{
	public class CavrnusPropertyBinderFloat : CavrnusPropertyBinder<float>
	{
		public override IDisposable BindProperty(Action<float> onPropertyUpdated, CavrnusPropertiesContainer optionalPropertiesContainer = null)
		{
			return new DelayedSetupDisposable(this, (sc, containerName) =>
			{
				return sc.BindFloatPropertyValue(containerName, PropertyName, onPropertyUpdated);
			});
		}

		protected override float GetServerValue(CavrnusSpaceConnection sc, string container)
		{
			return sc.GetFloatPropertyValue(container, PropertyName);
		}

		protected override void DefineDefaultValue(CavrnusSpaceConnection sc, string container)
		{
			sc.DefineFloatPropertyDefaultValue(container, PropertyName, DefaultValue);
		}
	}
}