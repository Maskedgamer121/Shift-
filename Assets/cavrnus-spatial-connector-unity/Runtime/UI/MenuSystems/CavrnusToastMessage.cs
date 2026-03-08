using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusToastMessage : MonoBehaviour
    {
        public UnityEvent OnDefault;
        public UnityEvent OnInProgress;
        public UnityEvent OnFinished;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image backgroundImage;

        [Header("State Colors")]
        [SerializeField] private Color inProgressColor = Color.blue;
        [SerializeField] private Color finishedColor = Color.green;
        
        [Header("Auto-Hide Settings")]
        [SerializeField] private float hideAfterSeconds = 2.5f;
        [SerializeField] private bool autoHideOnFinished = true;

        private Coroutine hideCoroutine;

        private void Awake()
        {
            OnDefault?.Invoke();
            SetVisibility(false);
        }

        public void SetVisibility(bool vis)
        {
            gameObject.SetActive(vis);
        }

        public void SetHeaderMessage(string message)
        {
            headerText.gameObject.SetActive(!String.IsNullOrWhiteSpace(message));
            headerText.text = message;
        }

        public void SetBodyMessage(string message)
        {
            bodyText.gameObject.SetActive(!String.IsNullOrWhiteSpace(message));
            bodyText.text = message;
        }

        public void Hide()
        {
            StopHideTimer();
            SetVisibility(false);
        }
        
        public void SetState(string state)
        {
            switch (state.ToLowerInvariant())
            {
                case "started":
                    SetVisibility(true);
                    OnInProgress?.Invoke();
                    backgroundImage.color = inProgressColor;
                    break;

                case "finished":
                    SetVisibility(true);
                    OnFinished?.Invoke();
                    backgroundImage.color = finishedColor;

                    if (autoHideOnFinished)
                    {
                        StopHideTimer();
                        hideCoroutine = StartCoroutine(HideAfterDelay());
                    }
                    break;
            }
        }
        
        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(hideAfterSeconds);
            gameObject.SetActive(false);
        }

        private void StopHideTimer()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
        }
    }
}