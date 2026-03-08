using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class UsersListEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private UnityEvent<bool> onSpeaking;
		
		[Header("UI Components")]
		[SerializeField] private TMP_Text nameText;
		[SerializeField] private Image profilePicImage;
		[SerializeField] private RawImage videoStreamImage;
		
		[SerializeField] private GameObject mutedGameObject;
		[SerializeField] private WidgetUserMic userMic;
		
		[SerializeField] private MiniUserListSpeakingPulse speakingPulse;
		
		[SerializeField] private Button maximizeButton;
		
		private readonly List<IDisposable> disposables = new List<IDisposable>();

		private CavrnusUser user;
		private Action<CavrnusUser> onSelectedUser;
		
		private void Awake()
		{
			maximizeButton.gameObject.SetActive(false);
		}
		
		public void Setup(CavrnusUser user, Action<CavrnusUser> selectedUser)
		{
			this.user = user;
			onSelectedUser = selectedUser;
			userMic.Setup(user);
			
			maximizeButton.onClick.AddListener(OnMaximizeUserClicked);
			
			if (nameText != null) {
				var nameDisposable = user.BindUserName(n => nameText.text = n);
				disposables.Add(nameDisposable);
			}

			var picDisp = CavrnusUserPicStreamHelpers.BindUserPicToImage(user, profilePicImage, profilePicImage.GetComponent<AspectRatioFitter>());
            disposables.Add(picDisp);
            
            var isStreaming = user.BindUserStreaming(isStreaming => videoStreamImage.gameObject.SetActive(isStreaming));
			disposables.Add(isStreaming);
			
			var isSpeaking = user.BindUserSpeaking(isSpeaking => {
				speakingPulse.IsSpeaking = isSpeaking;
				onSpeaking?.Invoke(isSpeaking);
			});
			disposables.Add(isSpeaking);
			
            var videoDisp = CavrnusUserPicStreamHelpers.BindUserStreamToRawImage(user, videoStreamImage, videoStreamImage.GetComponent<AspectRatioFitter>());
			disposables.Add(videoDisp);

			var muted = user.BindUserMuted(isMuted => mutedGameObject.SetActive(isMuted));
			disposables.Add(muted);
		}

		private void OnMaximizeUserClicked()
		{
			onSelectedUser?.Invoke(user);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			maximizeButton.gameObject.SetActive(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			maximizeButton.gameObject.SetActive(false);
		}
		
		public void OnPointerClick(PointerEventData eventData) { }
			
		private void OnDestroy()
		{
			foreach (var disp in disposables) 
				disp?.Dispose();
		}
	}
}