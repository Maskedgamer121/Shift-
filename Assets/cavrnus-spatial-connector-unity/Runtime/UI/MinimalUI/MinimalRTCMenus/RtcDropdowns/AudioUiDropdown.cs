using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class AudioUiDropdown : RtcUiDropdownBase
    {
        private List<CavrnusInputDevice> foundInputs = new List<CavrnusInputDevice>();
        
        private const string PLAYERPREFS_AUDIOINPUT = "CavrnusAudioInput";

        private IDisposable binding;

        protected override void OnSpaceConnected()
        {
            //If we've already selected audio devices on a previous run, use those
            var savedAudioInput = PlayerPrefs.GetString(PLAYERPREFS_AUDIOINPUT, null);
            
            binding = SpaceConnection.FetchAudioInputs(opts => {
                foundInputs = opts;
            
                Dropdown.ClearOptions();
            
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var opt in opts) 
                    options.Add(new TMP_Dropdown.OptionData(opt.Name));
            
                Dropdown.AddOptions(options);
            
                //If we have a saved selection, pick it
                if (savedAudioInput != null) {
                    var selection = foundInputs.FirstOrDefault(inp => inp.Id == savedAudioInput);
            
                    if (selection != null) {
                        Dropdown.value = foundInputs.ToList().IndexOf(selection);
                        SpaceConnection.UpdateAudioInput(selection);
                    }
                }
                else if (foundInputs.Count > 0) {
                    Dropdown.value = 0;
                    SpaceConnection.UpdateAudioInput(foundInputs[0]);
                }
            });
        }

        protected override void DropdownValueChanged(int inputId)
        {
            base.DropdownValueChanged(inputId);
            
            //Have we finished fetching the options?
            if (foundInputs == null)
                return;

            //Save our selection so we have it on future runs
            PlayerPrefs.SetString(PLAYERPREFS_AUDIOINPUT, foundInputs[inputId].Id);
            PlayerPrefs.Save();

            SpaceConnection.UpdateAudioInput(foundInputs[inputId]);
        }
        
        protected override void OnDestroy()
        {
            binding?.Dispose();
        }
    }
}