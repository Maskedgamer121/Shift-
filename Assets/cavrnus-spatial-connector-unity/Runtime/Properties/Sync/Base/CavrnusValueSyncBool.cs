using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncBool : CavrnusValueSync<bool>
	{
		private void Reset() { PropertyName = "Boolean"; }
	}
}