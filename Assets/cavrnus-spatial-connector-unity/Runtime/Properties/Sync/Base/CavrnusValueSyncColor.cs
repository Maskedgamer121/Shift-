using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncColor : CavrnusValueSync<Color>
	{
		private void Reset() { PropertyName = "Color"; }
	}
}