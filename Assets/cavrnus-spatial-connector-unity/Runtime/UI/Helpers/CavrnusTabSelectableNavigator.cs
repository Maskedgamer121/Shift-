using System.Collections.Generic;
using Cavrnus.SpatialConnector.PlayerControllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Cavrnus.SpatialConnector.UI
{
	public class CavrnusTabSelectableNavigator : MonoBehaviour
    {
        [SerializeField] private List<Selectable> selectables;
        
        private EventSystem eventSystem;

        private void Start()
        {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }

        private void Update() 
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			if (SimpleCavrnusInput.IsKeyDown(Key.Tab))
#elif ENABLE_LEGACY_INPUT_MANAGER
	        if (SimpleCavrnusInput.IsKeyDown(KeyCode.Tab))
#else
			if (false)
#endif
            {
				for (var i = 0; i < selectables.Count; i++) 
                {
                    if(selectables[i].gameObject == eventSystem.currentSelectedGameObject) 
                    {
                        selectables[(i+1) % selectables.Count].Select();
                        break;
                    }
                }
            }
        }
    }
}