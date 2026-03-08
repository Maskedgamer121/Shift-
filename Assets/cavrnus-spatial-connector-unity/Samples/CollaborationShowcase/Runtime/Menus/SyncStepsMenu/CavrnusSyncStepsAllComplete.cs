using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
    public class CavrnusSyncStepsAllComplete : MonoBehaviour
    {
        [SerializeField] private string propertyContainer = "ExampleProcedure";
        [SerializeField] private string propertyName = "PropertyNameProgress";

        [SerializeField] private Material incompleteMaterial;
        [SerializeField] private Material completeMaterial;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                spaceConn.BindBoolPropertyValue(propertyContainer, propertyName, isComplete => {
                    GetComponent<Renderer>().material = isComplete ? completeMaterial : incompleteMaterial;
                });
            });
        }
    }
}