using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
    [AddComponentMenu("Cavrnus/DataBinding/Sync/SyncLightIntensity")]
	public class CavrnusPropertySyncLightIntensity : CavrnusValueSyncFloat
    {
        public override float GetValue()
        {
            return GetComponent<Light>().intensity;
        }

        public override void SetValue(float value)
        {
            GetComponent<Light>().intensity = value;
        }
    }
}