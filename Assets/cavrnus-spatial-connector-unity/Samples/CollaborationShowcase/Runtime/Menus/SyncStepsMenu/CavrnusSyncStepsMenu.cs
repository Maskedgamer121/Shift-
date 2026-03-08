using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
    public class CavrnusSyncStepsMenu : MonoBehaviour
    {
        [Serializable]
        public class StepInfo
        {
            public string StepName;

            public CavrnusSpaceConnection SpaceConn{ get; private set; }
            private string containerName;
            private string propertyName;

            public void Setup(CavrnusSpaceConnection spaceConn, string containerName, string propertyName)
            {
                SpaceConn = spaceConn;
                this.containerName = containerName;
                this.propertyName = propertyName;
            }

            public IDisposable BindStepStatusUpdate(Action<bool> onUpdate)
            {
                return SpaceConn.BindBoolPropertyValue(containerName, propertyName, onUpdate);
            }
            
            public void PostStepCompleteState(bool state)
            {
                SpaceConn.PostBoolPropertyUpdate(containerName, propertyName, state);
            }

            public bool IsComplete()
            {
                return SpaceConn.GetBoolPropertyValue(containerName, propertyName);
            }
        }

        [SerializeField] private string propertyContainer = "StepsSync";
        [SerializeField] private string propertyNameProgress = "Steps";
        [SerializeField] private string propertyNameComplete = "IsComplete";
        [SerializeField] private string propertyNameReset = "Reset";
        [SerializeField] private List<StepInfo> steps;

        [Space]
        [SerializeField] private GameObject toolTip;
        [SerializeField] private GameObject syncStepsItemPrefab;
        [SerializeField] private Transform syncStepsItemContainer;

        private CavrnusSpaceConnection spaceConn;

        private List<CavrnusSyncStepsItem> syncUiItems = new List<CavrnusSyncStepsItem>();

        private CavrnusLivePropertyUpdate<string> resetProcedureUpdater;
        private readonly List<IDisposable> bindings = new List<IDisposable>();
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConn = sc;
                
                sc.DefineBoolPropertyDefaultValue(propertyContainer, propertyNameProgress, false);
                bindings.Add(sc.BindBoolPropertyValue(propertyContainer, propertyNameProgress, isComplete => {
                    toolTip.SetActive(isComplete);
                }));

                //Reset
                bindings.Add(sc.BindStringPropertyValue(propertyContainer, propertyNameReset, s => {
                    if (s == "Reset") {
                        syncUiItems.ForEach(stepsItem => stepsItem.ResetItem());
                        spaceConn?.PostBoolPropertyUpdate(propertyContainer, propertyNameComplete, false);
                    }
                }));

                for (var index = 0; index < steps.Count; index++) {
                    var stepInfo = steps[index];
                    stepInfo.Setup(sc, propertyContainer, $"Step{index + 1}");

                    var go = Instantiate(syncStepsItemPrefab, syncStepsItemContainer);
                    var item = go.GetComponent<CavrnusSyncStepsItem>();
                    item.Setup(stepInfo, StepCompleted);
                    syncUiItems.Add(item);
                }
            });
        }

        private void StepCompleted(StepInfo obj)
        {
            if (syncUiItems.Count == 0) 
                return;
            
            // Here we can check if all steps are completed
            var allStepsComplete = syncUiItems.All(s => s.IsComplete());
            if (allStepsComplete)
                spaceConn?.PostBoolPropertyUpdate(propertyContainer, propertyNameComplete, true);
        }

        public void ResetSteps()
        {
            // Overall progress reset
            spaceConn?.BeginTransientStringPropertyUpdate(propertyContainer, propertyNameReset, "Reset").Cancel();
        }
    }
}