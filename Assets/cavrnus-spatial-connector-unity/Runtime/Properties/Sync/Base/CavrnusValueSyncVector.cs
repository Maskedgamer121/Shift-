using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncVector : CavrnusValueSync<Vector4>
	{
		private void Reset() { PropertyName = "Vector"; }
	}
}