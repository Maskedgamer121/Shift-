using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class UserJoinedNotificationEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI userName;
        [SerializeField] private Image joinSprite;
        [SerializeField] private Image exitSprite;
        
        public void Setup(CavrnusUser user, bool isJoining)
        {
            userName.text = $"{user.GetUserName()} {(isJoining ? "joined" : "left")} ";
            
            joinSprite.gameObject.SetActive(isJoining);
            exitSprite.gameObject.SetActive(!isJoining);
            
            gameObject.AddComponent<CavrnusFadeInOutAndDestroy>().Begin(0.3f, 5f);
        }
    }
}