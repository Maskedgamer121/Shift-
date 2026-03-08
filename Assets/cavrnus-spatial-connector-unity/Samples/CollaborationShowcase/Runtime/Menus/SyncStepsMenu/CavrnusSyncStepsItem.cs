using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
    public class CavrnusSyncStepsItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI stepName;
        
        [Header("Status Components")]
        [SerializeField] private Button completeButton;
        [SerializeField] private GameObject completeCheckmark;

        private CavrnusSyncStepsMenu.StepInfo stepInfo;

        private IDisposable binding;
        private Action<CavrnusSyncStepsMenu.StepInfo> stepCompleted;

        public void Setup(CavrnusSyncStepsMenu.StepInfo stepInfo, Action<CavrnusSyncStepsMenu.StepInfo> stepCompleted)
        {
            stepName.text = stepInfo.StepName;
            this.stepInfo = stepInfo;
            this.stepCompleted = stepCompleted;

            binding = stepInfo.BindStepStatusUpdate(isComplete => {
                completeButton.gameObject.SetActive(!isComplete);
                completeCheckmark.gameObject.SetActive(isComplete);
                stepCompleted?.Invoke(stepInfo);
            });
        }

        public bool IsComplete()
        {
            return stepInfo.IsComplete();
        }

        public void ClickComplete()
        {
            stepInfo.PostStepCompleteState(true);
        }

        public void ResetItem()
        {
            stepInfo.PostStepCompleteState(false);
        }

        private void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}