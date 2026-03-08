using System;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Samples.MobileDemo
{
    public class DeviceSelectorPopupEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI deviceNameTextMeshPro;

        private int id;
        private Action<int> onSelect;
        
        public void Setup(int id, string deviceName, Action<int> onSelect)
        {
            this.id = id;
            this.onSelect = onSelect;

            deviceNameTextMeshPro.text = deviceName;
        }

        public void OnPointerClick()
        {
            onSelect?.Invoke(id);
        }
    }
}