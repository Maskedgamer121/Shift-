using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using UnityEngine;
using UnityEngine.Events;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusGatherAiInsights : MonoBehaviour
    {
        public UnityEvent OnStarted;
        public UnityEvent OnFinished;
        
        private CavrnusSpaceConnection spaceConn;
        
        private void Start()
        {
            gameObject.SetActive(false);

            // disable for now.
            return;
            /*if (string.IsNullOrEmpty(CavrnusSpatialConnector.Instance.AdditionalSettings.OpenAiApiKey))
            {
                Debug.LogError("No OpenAI API Key provided.  AI Insights unavailable.");
                return;
            }

            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc =>
            {
                sc.BindSpacePolicy("api:rooms:getRoom", allowed =>
                {
                    gameObject.SetActive(allowed);
                });

                spaceConn = sc;
            });*/
        }
        
        public void CopyAiInsightsToClipboard()
        {
            if (spaceConn == null)
            {
                print("No Space Connected!");
                return;
            }
            
            DoCopyJournal();
        }
        
        private async void DoCopyJournal()
        {
            OnStarted?.Invoke();
            string history = await CavrnusFunctionLibrary.FetchSpaceHistory(spaceConn);

            GUIUtility.systemCopyBuffer = history;
            Debug.Log("Space History Copied to Clipboard!");
            
            OnFinished?.Invoke();
        }
    }
}