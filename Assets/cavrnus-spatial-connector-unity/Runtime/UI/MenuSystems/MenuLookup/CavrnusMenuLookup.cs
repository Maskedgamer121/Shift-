using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    [Serializable]
    public class MenuEntry {
        public string id;
        public GameObject prefab;
    }
    
    public class CavrnusMenuLookup : MonoBehaviour
    {
        [SerializeField] private List<MenuEntry> menuEntries;

        private Dictionary<string, GameObject> menuPrefabs;

        private void Awake()
        {
            menuPrefabs = new Dictionary<string, GameObject>();
            foreach (var entry in menuEntries) {
                print(entry.id);
                menuPrefabs[entry.id] = entry.prefab;
            }
        }

        public GameObject Get(string id) => menuPrefabs[id];
    }
}