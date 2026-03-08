using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncString : CavrnusValueSync<string>
	{
		private void Reset() { PropertyName = "String"; }
	}
}