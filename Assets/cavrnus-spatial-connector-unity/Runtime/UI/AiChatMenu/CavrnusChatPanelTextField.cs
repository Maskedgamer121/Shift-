using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusChatPanelTextField : MonoBehaviour
    {
        public event Action<CavrnusChatEntryData> OnNewLocalUserChatSubmitted;
        
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;

        [SerializeField] private Vector2 minMaxHeight;

        private void Awake()
        {
            submitButton.interactable = false;
            
            inputField.onSubmit.AddListener(OnChatSubmitted);
            inputField.onValueChanged.AddListener(OnChatChanged);
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }

        private void OnChatChanged(string msg)
        {
            submitButton.interactable = !String.IsNullOrEmpty(msg);
        }

        private void OnSubmitButtonClicked()
        {
            CreateChat();
        }

        private void OnChatSubmitted(string msg)
        {
            CreateChat();
        }

        private void CreateChat()
        {
            CavrnusChatEntryData data = new CavrnusChatEntryData
            {
                IsLocalUser = true,
                Message = inputField.text,
                Date = DateTime.Now
            };
            
            inputField.text = "";
            submitButton.interactable = false;

            OnNewLocalUserChatSubmitted?.Invoke(data);
        }
    }
}