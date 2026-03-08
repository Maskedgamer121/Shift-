using System.Linq;
using Cavrnus.SpatialConnector.Core;
using TMPro;

namespace Cavrnus.SpatialConnector.UI
{
	public class VideoUiDropdown : RtcUiDropdownBase
    {
        private CavrnusDeferredDisposable disposable;
        
        protected override void OnSpaceConnected()
        {
            disposable = CavrnusRtcController.Instance.Video.FetchVideoDevices(foundDevices =>
            {
                Dropdown.ClearOptions();
                var options = foundDevices.Select(opt => new TMP_Dropdown.OptionData(opt.Name)).ToList();
                Dropdown.AddOptions(options);
                
                var device = CavrnusRtcController.Instance.Video.CurrentDeviceIndex;
                if (device!= null)
                    Dropdown.SetValueWithoutNotify(device.Value);
            });
        }

        protected override void DropdownValueChanged(int inputId)
        {
            base.DropdownValueChanged(inputId);
            
            CavrnusRtcController.Instance.Video.SetDeviceByIndex(inputId);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            disposable?.Dispose();
        }
    }
}