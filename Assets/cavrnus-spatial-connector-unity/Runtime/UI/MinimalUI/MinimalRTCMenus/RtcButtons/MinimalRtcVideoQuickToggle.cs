using System;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class MinimalRtcVideoQuickToggle : MonoBehaviour
    {
        public UnityEvent<bool> OnStreamingStateChanged;
        
        [SerializeField] private Button button;

        private CavrnusUser localUser;
        private Action unsubscribeCanStream;
        private CavrnusDeferredDisposable binding;

        private void Start()
        {
            button.onClick.AddListener(ButtonClicked);

            unsubscribeCanStream = CavrnusRtcController.Instance?.Video.BindCanStream(canStream =>
            {
                SetButtonState(canStream);
            });

            binding = CavrnusRtcController.Instance?.Video.BindStreamState(isStreaming =>
            {
                StreamingModeChanged(isStreaming);
            });
        }

        private void ButtonClicked()
        {
            CavrnusRtcController.Instance?.Video.ToggleState();
        }

        public void StreamingModeChanged(bool state)
        {
            SetButtonState(state);
        }

        public void SetButtonState(bool state)
        {
            OnStreamingStateChanged?.Invoke(state);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
            unsubscribeCanStream?.Invoke();
            button.onClick.RemoveListener(ButtonClicked);
        }
    }
}