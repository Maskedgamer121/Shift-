using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncFloat : CavrnusValueSync<float>
	{
		private void Reset() { PropertyName = "Float"; }
	}
}