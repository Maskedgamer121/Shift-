using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class MaximizedUserManager : MonoBehaviour
    {
        public event Action<bool> OnVisChanged; 
        
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image profilePicImage;
        [SerializeField] private RawImage videoStreamImage;
        [SerializeField] private Button buttonClose;
        
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        private void Awake()
        {
            SetVis(false);
            
            buttonClose.onClick.AddListener(ButtonCloseClicked);
        }

        private void ButtonCloseClicked()
        {
            SetVis(false);
        }

        public void SetVis(bool vis)
        {
            gameObject.SetActive(vis);
            OnVisChanged?.Invoke(vis);
        }

        public void LoadUser(CavrnusUser user)
        {
            DisposeBindings();
            
            if (nameText != null) {
                var nameDisposable = user.BindUserName(n => nameText.text = n);
                disposables.Add(nameDisposable);
            }
			
            var picDisp = CavrnusUserPicStreamHelpers.BindUserPicToImage(user, profilePicImage, profilePicImage.GetComponent<AspectRatioFitter>());
            disposables.Add(picDisp);
            
            var isStreaming = user.BindUserStreaming(isStreaming => videoStreamImage.gameObject.SetActive(isStreaming));
            disposables.Add(isStreaming);
			
            var videoDisp = CavrnusUserPicStreamHelpers.BindUserStreamToRawImage(user, videoStreamImage, videoStreamImage.GetComponent<AspectRatioFitter>());
            disposables.Add(videoDisp);
            
            SetVis(true);
        }

        private void DisposeBindings()
        {
            disposables?.ForEach(disp => disp?.Dispose());
        }

        private void OnDestroy()
        {
            DisposeBindings();
        }
    }
}