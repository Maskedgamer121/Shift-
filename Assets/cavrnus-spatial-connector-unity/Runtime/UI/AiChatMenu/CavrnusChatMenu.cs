using System;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.Setup;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusChatMenu : MonoBehaviour
    {        
        [SerializeField] private CavrnusChatConversationWidget conversationWidget;
        [SerializeField] private CavrnusChatPanelTextField panelTextField;

        private CavrnusSpaceConnection spaceConn;

        private void Awake()
        {
            // moved to mcps instead. DELETE ME?

            //panelTextField.OnNewLocalUserChatSubmitted += LocalUserChatSubmitted;

           // CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => spaceConn = sc);
        }

        private void LocalUserChatSubmitted(CavrnusChatEntryData userRequest)
        {
            /*if (spaceConn == null)
                return;

            conversationWidget.AddChat(userRequest);
            
            var request = new GameObject("ChatGPTRequest").AddComponent<ChatGPTRequest>();
            var aiMsg = new CavrnusChatEntryData
            {
                IsLocalUser = false,
                Message = "Generating response...",
                Date = DateTime.Now
            };

            string finalMsg = userRequest.Message;
            var history = await CavrnusFunctionLibrary.FetchSpaceHistory(spaceConn);
            finalMsg += $"\nAnswer the above question using this data:";
            finalMsg += $"\n{history}";

            Debug.Log($"Requesting {finalMsg}");

            var aiEntry = conversationWidget.AddChat(aiMsg);
            var response = await request.RequestChatGptResponse(CavrnusSpatialConnector.Instance.AdditionalSettings.OpenAiApiKey, finalMsg);

            aiEntry.FinalizeMessage(response);*/
        }

        private void OnDestroy()
        {
           // panelTextField.OnNewLocalUserChatSubmitted -= LocalUserChatSubmitted;
        }
    }
}