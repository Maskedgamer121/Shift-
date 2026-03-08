using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.UI;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.MobileDemo
{
    public class VideoSelectorPopup : MonoBehaviour
    {
        [SerializeField] private DeviceSelectorPopupEntry entryPrefab;
        [SerializeField] private Transform entryParent;
        
        private List<CavrnusVideoInputDevice> videoInputs;
        private CavrnusSpaceConnection spaceConnection;
        private CavrnusDeferredDisposable disposable;
        
        private void Start() => CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection);
        
        private void OnSpaceConnection(CavrnusSpaceConnection sc)
        {
            spaceConnection = sc;
            
            CavrnusRtcController.Instance.Video.FetchVideoDevices(opts => {
                videoInputs = opts;
                for (var i = 0; i < opts.Count; i++) {
                    var go = Instantiate(entryPrefab, entryParent, true);
                    go.Setup(i, videoInputs[i].Name, DeviceSelected);
                }
            });
        }
        
        private void DeviceSelected(int id)
        {
            if (videoInputs == null) {
                spaceConnection.SetLocalUserStreamingState(false);
                return;
            }
            
            CavrnusRtcController.Instance.Video.SetDeviceByIndex(id);
            PopupCanvas.Instance.Close();
        }
    }
}