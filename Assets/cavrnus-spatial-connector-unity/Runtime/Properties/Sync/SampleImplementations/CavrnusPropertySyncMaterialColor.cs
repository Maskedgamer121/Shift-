using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
    [AddComponentMenu("Cavrnus/DataBinding/Sync/SyncMaterialColor")]
	public class CavrnusPropertySyncMaterialColor : CavrnusValueSyncColor
    {
        [Space]
        [SerializeField] private string colorPropertyName = "_BaseColor";
        public Material TargetMaterial;
        
        public override Color GetValue()
        {
            return TargetMaterial.GetColor(colorPropertyName);
        }
        
        public override void SetValue(Color value) { 
            TargetMaterial.SetColor(colorPropertyName, value);
        }
    }
}