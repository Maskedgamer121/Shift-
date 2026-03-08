using System.Collections.Generic;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
    [AddComponentMenu("Cavrnus/DataBinding/Sync/SyncMaterialTexture")]
	public class CavrnusPropertySyncMaterialTexture : CavrnusValueSyncString
    {
        [Space] [SerializeField] private Material material;

        [Space] public List<Texture> textures;
        private readonly Dictionary<string, Texture> textureLookup = new Dictionary<string, Texture>();

        private void Awake()
        {
            textures.ForEach(t => textureLookup.Add(t.name, t));
        }

        public override string GetValue()
        {
            if (!material || !material?.mainTexture)
                return "";
            
            return material.mainTexture.name;
        }

        public override void SetValue(string value)
        {
            if (textureLookup.TryGetValue(value, out var found)) material.mainTexture = found;
        }
    }
}