using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusChatConversationWidget : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private CavrnusChatEntry chatEntryPrefab;
        
        [Header("Components")]
        [SerializeField] private Transform parent;
        [SerializeField] private ScrollRect scrollRect;
        
        private List<CavrnusChatEntry> chatEntriesLookup = new List<CavrnusChatEntry>();

        public CavrnusChatEntry AddChat(CavrnusChatEntryData chatData)
        {
            var newChatEntry = Instantiate(chatEntryPrefab, parent);
            newChatEntry.Setup(chatData);
            
            chatEntriesLookup.Add(newChatEntry);
            
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;

            return newChatEntry;
        }
        
        private void OnDestroy()
        {
            chatEntriesLookup?.Clear();
        }
    }
}