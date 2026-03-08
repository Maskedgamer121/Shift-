using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.PlayerControllers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cavrnus.SpatialConnector.UI
{
    // Keeping this simple, enforce single popup visible at a time.
    public class CavrnusDesktopPopupSystem : ICavrnusPopupSystem
    {
        private List<GameObject> popups;
        private ICavrnusWidgetDisplayer widgetDisplayer;
        
        public CavrnusDesktopPopupSystem(ICavrnusWidgetDisplayer widgetDisplayer)
        {
            this.widgetDisplayer = widgetDisplayer;
            popups = new List<GameObject>();
        }

        public T Create<T>(GameObject popupPrefab, CavrnusPopupOptions options = new(), Action<T> onCreated = null) where T : Component
        {
            if (popupPrefab == null)
            {
                Debug.Log("Popup prefab is null!");
                return null;
            }

            DestroyAll();

            // Super temporary...
            var player = Object.FindFirstObjectByType<CavrnusPlayerFlyController>();
            if (player)
                player.enabled = false;

            var popupGo = Object.Instantiate(popupPrefab);
            var newPopup = popupGo.GetComponent<T>();
            
            widgetDisplayer.Show(popupGo);

            popups.Add(popupGo);

            return newPopup;
        }

        public void DestroyAll()
        {
            popups.ForEach(p =>
            {
                widgetDisplayer.Destroy(p);
            });
            
            popups.Clear();
            
            // Super temporary...
            var player = Object.FindFirstObjectByType<CavrnusPlayerFlyController>();
            if (player)
                player.enabled = true;
        }
    }
}