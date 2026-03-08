using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class FocusModeManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject target;
        [SerializeField] private Toggle focusModeToggle;

        private void Awake()
        {
            SetState(false);
            focusModeToggle.onValueChanged.AddListener(OnToggleClicked);
        }

        private void OnToggleClicked(bool val)
        {
            SetState(val);
        }

        private void OnEnable()
        {
            gameObject.DoFade(new List<CanvasGroup> {canvasGroup}, 0.2f, true);
        }

        public void SetState(bool state)
        {
            target.SetActive(state);
            focusModeToggle.isOn = state;
        }

        private void OnDestroy()
        {
            focusModeToggle.onValueChanged.RemoveListener(OnToggleClicked);
        }
    }
}