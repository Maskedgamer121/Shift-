using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class MinimalRtcButtonsMenu : MonoBehaviour
    {
        private List<MinimalRtcDropdownButton> rtcButtons = new List<MinimalRtcDropdownButton>();

        private MinimalRtcDropdownButton currentActiveDropdownButton;
        
        private void Start()
        {
            GetComponentsInChildren(rtcButtons);
            ResetButtons();
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConnection => {
                spaceConnection.AwaitLocalUser(lu => {
                    for (var i = 0; i < rtcButtons.Count; i++) {
                        rtcButtons[i].Setup(i.ToString());
                        rtcButtons[i].OnDropdownButtonClicked += OnDropdownButtonClicked;
                    }
                });
            });
        }

        private void OnDropdownButtonClicked(MinimalRtcDropdownButton targetDropdownButton, bool dropdownIsOpen)
        {
            // Open target dropdown, close others
            if (!dropdownIsOpen) {
                targetDropdownButton.SetDropdownActiveState(true);
                rtcButtons.ForEach(button => {
                    if (button != targetDropdownButton) {
                        button.SetDropdownActiveState(false);
                    }
                });
            }
            //Target dropdown already open, just close it...
            else {
                targetDropdownButton.SetDropdownActiveState(false);
            }
        }

        private void ResetButtons()
        {
            rtcButtons.ForEach(b => b.SetDropdownActiveState(false));
        }

        private void OnDestroy()
        {
            for (var i = 0; i < rtcButtons.Count; i++)
                rtcButtons[i].OnDropdownButtonClicked -= OnDropdownButtonClicked;
        }
    }
}