using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties
{
	public class CavrnusPropertyBinderColor : CavrnusPropertyBinder<Color>
	{
		public override IDisposable BindProperty(Action<Color> onPropertyUpdated, CavrnusPropertiesContainer optionalPropertiesContainer = null)
		{
			return new DelayedSetupDisposable(this, (sc, containerName) =>
			{
				return sc.BindColorPropertyValue(containerName, PropertyName, onPropertyUpdated);
			});
		}

		protected override Color GetServerValue(CavrnusSpaceConnection sc, string container)
		{
			return sc.GetColorPropertyValue(container, PropertyName);
		}

		protected override void DefineDefaultValue(CavrnusSpaceConnection sc, string container)
		{
			sc.DefineColorPropertyDefaultValue(container, PropertyName, DefaultValue);
		}
	}
}