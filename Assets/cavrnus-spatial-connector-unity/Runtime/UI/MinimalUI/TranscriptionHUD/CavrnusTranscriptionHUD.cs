using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.Base.Collections;
using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.LiveRoomSystem.LiveObjectManagement.ObjectTypeManagers;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class CavrnusTranscriptionHUD : MonoBehaviour
    {
        public event Action<bool> OnTranscriptionPropertyEnabled;
        
        [SerializeField] private Transform entriesContainer;
        [SerializeField] private CavrnusTranscriptionHUDEntry entryPrefab;

        [SerializeField] private float duration = 8f;
        [SerializeField] private int maxEntriesVisible = 5;

        private Dictionary<int, CavrnusTranscriptionHUDEntry> visibleEntries = new Dictionary<int, CavrnusTranscriptionHUDEntry>();
        private List<IDisposable> binds = new List<IDisposable>();

        private bool isActive;
        
        private void Awake()
        {
            entriesContainer.gameObject.SetActive(false);
        }

        public void SetVis(bool vis)
        {
            entriesContainer.gameObject.SetActive(vis);

            if (!vis) {
                visibleEntries.ForEach(entry => {
                    if (entry.Value != null && entry.Value.gameObject != null)
                        Destroy(entry.Value.gameObject);
                });
                
                visibleEntries.Clear();
            }

            isActive = vis;
        }

        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                binds.Add(spaceConn.BindChatMessages(MessageAdded, MessagedRemoved));
                binds.Add(spaceConn.BindBoolPropertyValue("room/transcription", "enabled", isEnabled => {
                    if (!isEnabled)
                        gameObject.SetActive(false);
                    
                    OnTranscriptionPropertyEnabled?.Invoke(isEnabled);
                }));
                
            });
        }

        private int countId;
        private void MessageAdded(IChatViewModel chat)
        {
            if (!isActive) return;
            
            if (chat.ChatType != ChatMessageSourceTypeEnum.Transcription) 
                return;
                
            var entry = Instantiate(entryPrefab, entriesContainer);
            entry.Setup(countId, chat, duration);
            
            entry.OnMessageCompleted += OnMessageCompleted;
            entry.OnDurationExpired += OnMessageDurationExpired;

            if (visibleEntries.Count >= maxEntriesVisible) {
                var oldestEntry = visibleEntries[visibleEntries.Count - 1];
                oldestEntry.OnMessageCompleted -= OnMessageCompleted;
                oldestEntry.OnDurationExpired -= OnMessageDurationExpired;
                Destroy(oldestEntry.gameObject);
                visibleEntries.Remove(countId);
            }
            
            visibleEntries.Add(countId, entry);
            
            // Here we sort and adjust the sibling indices
            SortEntries();
            
            countId++;
        }
    
        private void OnMessageCompleted(CavrnusTranscriptionHUDEntry obj)
        {
            if (!isActive) return;

            SortEntries();
        }
        
        private void MessagedRemoved(IChatViewModel chat)
        {
            if (!isActive) return;

            if (chat.ChatType != ChatMessageSourceTypeEnum.Transcription) 
                return;
        }
        
        private void OnMessageDurationExpired(CavrnusTranscriptionHUDEntry entry)
        {
            if (!isActive) return;

            Destroy(entry.gameObject);
            visibleEntries.Remove(entry.Id);
        }
        
        private void SortEntries()
        {
            if (!isActive) return;

            visibleEntries.ToList().Sort((a, b) => DateTime.Compare(a.Value.ChatData.CreateTime.Value, b.Value.ChatData.CreateTime.Value));
            for (var i = 0; i < visibleEntries.Count; i++) {
                if (visibleEntries.ContainsKey(i)) {
                    if (visibleEntries[i] != null && visibleEntries[i].gameObject != null)
                        visibleEntries[i].transform.SetSiblingIndex(i);
                }
            }
        }

        private void OnDestroy()
        {
            binds?.ForEach(b => b?.Dispose());
        }
    }
}