using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[RequireComponent(typeof(CavrnusPropertiesContainer))]
	public abstract class CavrnusValueSyncTransform : CavrnusValueSync<CavrnusTransformData>
	{
		private void Reset() { PropertyName = "Transform"; }
	}
}