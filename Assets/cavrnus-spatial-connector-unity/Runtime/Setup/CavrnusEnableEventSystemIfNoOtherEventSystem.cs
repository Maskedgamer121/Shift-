using UnityEngine;
using UnityEngine.EventSystems;

namespace Cavrnus.SpatialConnector.Setup
{
	public class CavrnusEnableEventSystemIfNoOtherEventSystem : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
			if (Object.FindFirstObjectByType<EventSystem>() == null)
			{
				// Enable our eventsystem child.
				gameObject.transform.Find("EventSystem").gameObject.SetActive(true);
			}
		}
	}
}
