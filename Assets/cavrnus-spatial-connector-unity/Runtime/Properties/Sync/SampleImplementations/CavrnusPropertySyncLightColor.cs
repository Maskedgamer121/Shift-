using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
    [AddComponentMenu("Cavrnus/DataBinding/Sync/SyncLightColor")]
	public class CavrnusPropertySyncLightColor : CavrnusValueSyncColor
    {
        public override Color GetValue()
        {
            return GetComponent<Light>().color;
        }

        public override void SetValue(Color value)
        {
            GetComponent<Light>().color = value;
        }
    }
}