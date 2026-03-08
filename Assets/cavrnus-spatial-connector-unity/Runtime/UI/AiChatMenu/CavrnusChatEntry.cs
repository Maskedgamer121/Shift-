using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusChatEntry : MonoBehaviour
    {
        [Header("Chat Components")]
        [SerializeField] private TextMeshProUGUI tm_message;
        [SerializeField] private TextMeshProUGUI tm_date;

        [Header("Visuals")]
        [SerializeField] private Color localUserColor;
        [SerializeField] private Color otherUserColor;
        [SerializeField] private GameObject spinner;
        

        [Header("Layout Components")]
        [SerializeField] private Image background;
        [SerializeField] private HorizontalOrVerticalLayoutGroup layoutGroup;
        [SerializeField] private RectTransform bubbleContainer;
        
        public void Setup(CavrnusChatEntryData data)
        {
            if (tm_message)
                tm_message.text = data.Message;
            
            if (tm_date)
                tm_date.text = data.Date.ToShortDateString();

            if (data.IsLocalUser)
            {
                spinner.SetActive(false);
                layoutGroup.childAlignment = TextAnchor.MiddleRight;
                background.color = localUserColor;
            }
            else
            {
                spinner.SetActive(true);
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                background.color = otherUserColor;
            }
        }

        public void FinalizeMessage(string msg)
        {
            tm_message.text = msg;
            spinner.SetActive(false);
        }
    }
}