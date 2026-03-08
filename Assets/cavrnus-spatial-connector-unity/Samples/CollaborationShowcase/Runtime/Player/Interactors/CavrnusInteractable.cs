using UnityEngine;
using UnityEngine.Events;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
	public class CavrnusInteractable : MonoBehaviour
    {
        [SerializeField] private UnityEvent onInteract;

        public void Interact() => onInteract?.Invoke();
    }
}