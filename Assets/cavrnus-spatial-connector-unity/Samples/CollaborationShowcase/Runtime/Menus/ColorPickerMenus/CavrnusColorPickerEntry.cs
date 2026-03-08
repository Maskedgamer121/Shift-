using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
	public class CavrnusColorPickerEntry : MonoBehaviour
    {
        public Color Color{ get; private set; }
        
        [SerializeField] private Image image;
        [SerializeField] private GameObject selectedBorder;
        
        private Action<Color> onSelected;
        
        public void Setup(Color color, Action<Color> onSelected)
        {
            Color = color;
            this.onSelected = onSelected;

            image.color = color;
        }
        
        public void SetSelectionState(bool state)
        {
            selectedBorder.SetActive(state);
        }

        public void Select() => onSelected?.Invoke(Color);
    }
}