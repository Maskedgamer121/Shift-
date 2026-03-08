using System;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.SpatialConnector.API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class MinimalRtcAudioQuickToggle : MonoBehaviour
    {
        public UnityEvent<bool> OnMuteStateChanged;
        
        [SerializeField] private string containerName = UserPropertyDefs.User_Muted;
        [SerializeField] private Button button;
        
        [SerializeField] private WidgetUserMic widgetUserMic;
        
        private IDisposable binding;
        private CavrnusUser localUser;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(connection => {
                connection.AwaitLocalUser(lu => {
                    localUser = lu;
                    button.onClick.AddListener(ButtonClicked);
                    binding = connection.BindBoolPropertyValue(lu.ContainerId, containerName, SetButtonState);
                    
                    widgetUserMic.Setup(localUser);
                });
            });
        }

        private void ButtonClicked()
        {
            var serverVal = localUser.SpaceConnection.GetBoolPropertyValue(localUser.ContainerId, containerName);
            localUser.SpaceConnection.SetLocalUserMutedState(!serverVal);
            SetButtonState(!serverVal);
        }
        
        public void SetButtonState(bool state)
        {
            OnMuteStateChanged?.Invoke(state);
        }
        
        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}