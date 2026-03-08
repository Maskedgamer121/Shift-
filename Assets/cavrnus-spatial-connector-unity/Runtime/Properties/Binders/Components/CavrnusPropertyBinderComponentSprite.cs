using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
    [AddComponentMenu("Cavrnus/DataBinding/Binders/BindSprite")]

    [RequireComponent(typeof(Image))]
    public class CavrnusPropertyBinderComponentSprite : CavrnusPropertyBinderComponent
    {
        [Header("Cavrnus Property")]
        [SerializeField] private CavrnusPropertyBinderString propertyBinderString;
        
        [Space]
        [SerializeField] private Image image;
        [SerializeField] private List<Sprite> sprites;
        
        private readonly Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();
        
        private IDisposable binding;
        
        private void Start()
        {
            sprites.ForEach(t => spriteLookup.Add(t.name, t));
            binding = propertyBinderString.BindProperty(val =>
            {
                if (spriteLookup.TryGetValue(val, out var found))
                {
                    if (image != null)
                        image.sprite = found;
                }
            });
        }
        
        private void OnDestroy() => binding?.Dispose();
    }
}