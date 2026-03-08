using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
    [AddComponentMenu("Cavrnus/DataBinding/Sync/SyncWorldTransform")]
	public class CavrnusPropertySyncWorldTransform : CavrnusValueSyncTransform
    {
        public override CavrnusTransformData GetValue()
        {
            return new CavrnusTransformData(transform.position, transform.eulerAngles, transform.lossyScale);
        }

        public override void SetValue(CavrnusTransformData value)
        {
            transform.position = value.Position;
            transform.eulerAngles = value.EulerAngles;
            transform.localScale = value.Scale;
        }
    }
}