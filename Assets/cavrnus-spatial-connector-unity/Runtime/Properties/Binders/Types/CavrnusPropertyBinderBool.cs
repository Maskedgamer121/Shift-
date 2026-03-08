using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;

namespace Cavrnus.SpatialConnector.Properties
{
	public class CavrnusPropertyBinderBool : CavrnusPropertyBinder<bool>
	{
		public override IDisposable BindProperty(Action<bool> onPropertyUpdated, CavrnusPropertiesContainer optionalPropertiesContainer = null)
		{
			return new DelayedSetupDisposable(this, (sc, containerName) =>
			{
				return sc.BindBoolPropertyValue(containerName, PropertyName, onPropertyUpdated);
			});
		}

		protected override bool GetServerValue(CavrnusSpaceConnection sc, string container)
		{
			return sc.GetBoolPropertyValue(container, PropertyName);
		}

		protected override void DefineDefaultValue(CavrnusSpaceConnection sc, string container)
		{
			sc.DefineBoolPropertyDefaultValue(container, PropertyName, DefaultValue);
		}
	}
}