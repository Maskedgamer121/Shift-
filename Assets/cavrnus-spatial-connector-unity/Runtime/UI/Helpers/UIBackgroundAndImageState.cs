using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class UIBackgroundAndImageState : MonoBehaviour
    {
        [Serializable]
        private class GraphicsGroup
        {
            public string Category;
            public List<Graphic> Targets;

            [Space]
            public Color EnabledColor = Color.black;
            public Color DisabledColor = Color.black;
        }
        
        [SerializeField] private List<GraphicsGroup> targets;
        
        [SerializeField] private GameObject iconContainer;
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite enabledIcon;
        [SerializeField] private Sprite disabledIcon;

        [SerializeField] private bool rotateIcon;
        [SerializeField] private float rotationAmount;
        
        public void SetOn()
        {
            if (iconImage != null)
                iconImage.sprite = enabledIcon;

            if (rotateIcon && iconContainer)
                iconContainer.transform.Rotate(Vector3.forward, rotationAmount);
            
            foreach (var t in targets) {
                foreach (var target in t.Targets) {
                    target.color = t.EnabledColor;
                }
            }
        }

        public void SetOff()
        {
            if (iconImage != null)
                iconImage.sprite = disabledIcon;
            
            if (rotateIcon && iconContainer)
                iconContainer.transform.Rotate(Vector3.forward, -rotationAmount);
            
            foreach (var t in targets) {
                foreach (var target in t.Targets) {
                    target.color = t.DisabledColor;
                }
            }
        }

        public void SetState(bool val)
        {
            if (val)
                SetOn();
            else
                SetOff();
        }
    }
}