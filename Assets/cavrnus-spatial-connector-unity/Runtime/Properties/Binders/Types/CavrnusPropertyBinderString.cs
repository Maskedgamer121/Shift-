using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;

namespace Cavrnus.SpatialConnector.Properties
{
	public class CavrnusPropertyBinderString : CavrnusPropertyBinder<string>
	{
		public override IDisposable BindProperty(Action<string> onPropertyUpdated, CavrnusPropertiesContainer optionalPropertiesContainer = null)
		{
			return new DelayedSetupDisposable(this, (sc, containerName) =>
			{
				return sc.BindStringPropertyValue(containerName, PropertyName, onPropertyUpdated);
			});
		}

		protected override string GetServerValue(CavrnusSpaceConnection sc, string container)
		{
			return sc.GetStringPropertyValue(container, PropertyName);
		}

		protected override void DefineDefaultValue(CavrnusSpaceConnection sc, string container)
		{
			sc.DefineStringPropertyDefaultValue(container, PropertyName, DefaultValue);
		}
	}
}