using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[AddComponentMenu("Cavrnus/DataBinding/Sync/SyncLocalTransform")]
	public class CavrnusPropertySyncLocalTransform : CavrnusValueSyncTransform
	{
		public override CavrnusTransformData GetValue()
		{
			return new CavrnusTransformData(transform.localPosition, transform.localEulerAngles, transform.localScale);
		}

		public override void SetValue(CavrnusTransformData value)
		{
			transform.localPosition = value.Position;
			transform.localEulerAngles = value.EulerAngles;
			transform.localScale = value.Scale;
		}
	}
}