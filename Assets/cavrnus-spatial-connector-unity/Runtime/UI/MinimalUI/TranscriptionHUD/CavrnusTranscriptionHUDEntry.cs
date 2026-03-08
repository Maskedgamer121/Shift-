using System;
using System.Collections;
using System.Collections.Generic;
using Cavrnus.Base.Settings;
using Cavrnus.LiveRoomSystem.LiveObjectManagement.ObjectTypeManagers;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class CavrnusTranscriptionHUDEntry : MonoBehaviour
    {
        public event Action<CavrnusTranscriptionHUDEntry> OnMessageCompleted;
        public event Action<CavrnusTranscriptionHUDEntry> OnDurationExpired;

        public IChatViewModel ChatData{ get; private set; }
        public int Id{ get; private set; }

        [SerializeField] private CanvasGroup cg;
        
        [SerializeField] private TextMeshProUGUI userName;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private Image profilePicImage;
        
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        private void Awake()
        {
            cg.alpha = 0f;
        }

        public void Setup(int id, IChatViewModel chat, float duration)
        {
            Id = id;
            ChatData = chat;
            disposables.Add(chat.CurrentLanguageTranslatedText.Bind(msg => message.text = msg.text ?? msg.liveSource));
            disposables.Add(chat.CreatorName.Bind(msg => userName.text = msg));
            disposables.Add(chat.CreatorProfilePicUrl.Bind(profilePicUrl =>
            {
                CavrnusStatics.Scheduler.ExecCoRoutine(CavrnusShortcutsLibrary.LoadProfilePic(profilePicUrl, pic =>
                {
                    if (profilePicImage == null)
                        return;
                    
                    profilePicImage.sprite = pic;
                    if (pic != null) {
                        profilePicImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)pic.texture.width / (float)pic.texture.height;
                    }
                }));
            }));
            
            disposables.Add(chat.MessageComplete.Bind(isComplete => {
                if (isComplete) {
                    if (gameObject != null) {
                        OnMessageCompleted?.Invoke(this);
                        CavrnusStatics.Scheduler.StartCoroutine(DurationRoutine(duration));
                    }
                }
            }));
            gameObject.DoFade(new List<CanvasGroup> { cg }, 0.3f, true);
        }

        private IEnumerator DurationRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (this != null && gameObject != null)
            {
                OnDurationExpired?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            disposables?.ForEach(disp => disp?.Dispose());
        }
    }
}