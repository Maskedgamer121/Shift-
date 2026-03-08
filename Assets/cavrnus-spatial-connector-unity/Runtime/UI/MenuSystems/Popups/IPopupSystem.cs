using System;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public struct CavrnusPopupOptions
    {
        public bool Stackable;
    }
    
    public interface ICavrnusPopupSystem
    {
        T Create<T>(GameObject popupPrefab, CavrnusPopupOptions options = new(), Action<T> onCreated = null) where T : Component;
        void DestroyAll();
    }
}