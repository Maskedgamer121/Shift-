using System;
using System.Collections.Generic;
using Cavrnus.Base.Core;
using Cavrnus.Base.Settings;
using Cavrnus.LiveRoomSystem.LiveObjectManagement.ObjectTypeManagers;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class ChatMenuEntry : MonoBehaviour
    {
        [Header("Chat Metadata References")]
        [SerializeField] private TextMeshProUGUI creatorName;
        [SerializeField] private TextMeshProUGUI creationTime;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private Image profilePicImage;
        
        [Header("Chat Hover")]
        [SerializeField] private List<CanvasGroup> extraButtonsCanvasGroup;
        
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        
        private IChatViewModel chat;
        private Action<IChatViewModel> onDelete;
        
        public void Setup(IChatViewModel chat, Action<IChatViewModel> onDelete)
        {
            this.chat = chat;
            this.onDelete = onDelete;

            disposables.Add(chat.CurrentLanguageTranslatedText.Bind(msg => message.text = msg.text ?? msg.liveSource));
            disposables.Add(chat.CreateTime.Bind(msg => creationTime.text = PrettyString.ToPrettyDay(msg.ToLocalTime())));
			disposables.Add(chat.CreatorName.Bind(msg => creatorName.text = msg));
			disposables.Add(chat.CreatorProfilePicUrl.Bind(profilePicUrl =>
			{
                CavrnusStatics.Scheduler.ExecCoRoutine(CavrnusShortcutsLibrary.LoadProfilePic(profilePicUrl, pic =>
                {
					if (profilePicImage == null)
					{
						return;
					}

					profilePicImage.sprite = pic;
					if (pic != null)
						profilePicImage.GetComponent<AspectRatioFitter>().aspectRatio =
						(float)pic.texture.width / (float)pic.texture.height;
				}));
			}));

			extraButtonsCanvasGroup.ForEach(cg => cg.alpha = 0f);
		}
        
        public void RemoveChatButtonClick()
        {
            onDelete?.Invoke(chat);
        }

        private void OnDestroy()
        {
            disposables.ForEach(d => d.Dispose());
        }
    }
}