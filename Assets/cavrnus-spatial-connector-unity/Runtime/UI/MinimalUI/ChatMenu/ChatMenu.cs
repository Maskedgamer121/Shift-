using System;
using System.Collections;
using System.Collections.Generic;
using Cavrnus.LiveRoomSystem.LiveObjectManagement.ObjectTypeManagers;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class ChatMenu : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Scrollbar scrollBar;

        [SerializeField] private Button resetButton;
        
        [Header("Chat Entries")]
        [SerializeField] private Transform chatEntryContainer;
        [SerializeField] private GameObject chatEntryPrefab;

        private readonly Dictionary<string, GameObject> createdChats = new Dictionary<string, GameObject>();
        private CavrnusSpaceConnection spaceConn;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        private void Start()
        {
            submitButton.interactable = false;
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                spaceConn.AwaitLocalUser(lu => {
                    this.spaceConn = spaceConn;
                    
                    disposables.Add(spaceConn.BindChatMessages(MessagesOnItemAddedEvent, MessagesOnItemRemovedEvent));
                    
                    submitButton.onClick.AddListener(SubmitChat);
                    
                    inputField.onValueChanged.AddListener(OnInputChanged);
                    inputField.onSubmit.AddListener(OnInputFieldSubmit);

                    if (resetButton) {
                        resetButton.onClick.AddListener(OnResetButtonClicked);
                        resetButton.gameObject.AddComponent<CanvasGroup>().alpha = 0f;
                    }
                });
            });
            
            if (scrollBar)
                scrollBar.onValueChanged.AddListener(ScrollbarValueChanged);
        }

        private void OnEnable()
        {
            CavrnusStatics.Scheduler.ExecCoRoutine(DelayedResetScrollbarRoutine());
        }

        private IEnumerator DelayedResetScrollbarRoutine()
        {
            yield return null;
            
            if (scrollBar)
                scrollBar.value = 0f;
        }

        private void OnResetButtonClicked()
        {
            if (scrollBar)
                scrollBar.value = 0f;
        }

        private void ScrollbarValueChanged(float val)
        {
            SetResetButtonVis(val > 0.4f);
        }

        private bool currentButtonVis = false;
        private void SetResetButtonVis(bool isVis)
        {
            // if (currentButtonVis != isVis) {
            //     gameObject.DoFade(new List<CanvasGroup> {resetButton.gameObject.GetComponent<CanvasGroup>()}, 0.15f, isVis);
            // }

            if (resetButton)
                resetButton.interactable = isVis; 
            
            currentButtonVis = isVis;
        }

        public void OnInputFieldSubmit(string arg0)
        {
            SubmitChat();
        }

        private void OnInputChanged(string input)
        {
            submitButton.interactable = !string.IsNullOrWhiteSpace(input);
        }
        
        private void MessagesOnItemAddedEvent(IChatViewModel item)
        {
            var chatObj = Instantiate(chatEntryPrefab, chatEntryContainer);
            chatObj.GetComponent<ChatMenuEntry>().Setup(item, DeleteChat);
            
            createdChats.Add(item.ObjectProperties.Id, chatObj);
        }
        
        private void DeleteChat(IChatViewModel chat)
        {
            // Not needed atm...
        }

        private void MessagesOnItemRemovedEvent(IChatViewModel item)
        {
            var obj = createdChats[item.ObjectProperties.Id];
            
            Destroy(obj);
            createdChats.Remove(item.ObjectProperties.Id);
        }

        private void SubmitChat()
        {
            if (!string.IsNullOrWhiteSpace(inputField.text) && !string.IsNullOrEmpty(inputField.text))
                spaceConn.PostChatMessage(inputField.text);

            inputField.text = string.Empty;

            StartCoroutine(DoFocus());
        }

        private IEnumerator DoFocus()
        {
            yield return null;

            inputField.ActivateInputField();
        }
        
        private void OnDestroy()
        {
            submitButton.onClick.RemoveListener(SubmitChat);
            inputField.onValueChanged.RemoveListener(OnInputChanged); 
            inputField.onSubmit.RemoveListener(OnInputFieldSubmit);
            
            if (resetButton)
                resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }
    }
}