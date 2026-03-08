using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Core;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
    public class CavrnusVideoController
    {
        private const string PLAYERPREFS_VIDEOOUTPUT = "CavrnusVideoOutput";
        private const string OFF_DEVICE_ID = "black"; // black is the ID of the default black/off device
        private const string PROPERTYDEF_STREAMING = "streaming";
                
        private event Action<bool> CanStreamChanged;
        public CavrnusVideoInputDevice CurrentDevice { get; private set; }
        public int? CurrentDeviceIndex => DevicesList?.IndexOf(CurrentDevice);
        public bool IsStreaming { get; private set; }
        public bool CanStreamState { get; private set; }
        
        private CavrnusSpaceConnection spaceConnection;
        
        private List<CavrnusVideoInputDevice> DevicesList = new();
        private Dictionary<string, CavrnusVideoInputDevice> DevicesDictionary = new();

        public CavrnusDeferredDisposable BindStreamState(Action<bool> onStreamStateChanged)
        {
            var dd = new CavrnusDeferredDisposable();
            
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn => {
                spaceConn.AwaitLocalUser(lu => {
                    spaceConn.DefineBoolPropertyDefaultValue(lu.ContainerId, PROPERTYDEF_STREAMING, false);
                    dd.Set(spaceConn.BindBoolPropertyValue(lu.ContainerId, PROPERTYDEF_STREAMING, onStreamStateChanged));
                });
            });
            
            onStreamStateChanged?.Invoke(IsStreaming);
            
            return new CavrnusDeferredDisposable();
        }
        
        public Action BindCanStream(Action<bool> onStreamableStateChanged)
        {
            void Handle(bool state) => onStreamableStateChanged?.Invoke(CanStreamState);
            CanStreamChanged += Handle;
            
            onStreamableStateChanged?.Invoke(CanStreamState);

            return () => CanStreamChanged -= Handle;
        }

        public void ToggleState()
        {
            if (spaceConnection == null)
            {
                Debug.Log("SpaceConnection is null! Can't set state");
                return;
            }

            spaceConnection.AwaitLocalUser(lu =>
            {
                var serverVal = lu.SpaceConnection.GetBoolPropertyValue(lu.ContainerId, PROPERTYDEF_STREAMING);
                SetState(!serverVal);
            });
        }
        
        public void SetState(bool state)
        {
            if (spaceConnection == null)
            {
                Debug.Log("SpaceConnection is null! Can't set state");
                return;
            }
        
            if (state && CanStreamState)
            {
                spaceConnection.UpdateVideoInput(CurrentDevice);
                spaceConnection.SetLocalUserStreamingState(true);
            }
            else
            {
                spaceConnection.SetLocalUserStreamingState(false);
            }
        }
        
        public void SetDeviceByIndex(int deviceIndex)
        {
            if (spaceConnection == null)
            {
                Debug.Log($"SpaceConnection is null! Can't set device: {deviceIndex}");
                return;
            }
            
            var foundDevice = DevicesList[deviceIndex];
            if (foundDevice == null)
            {
                SetCanStream(false);
                spaceConnection.SetLocalUserStreamingState(false);
                
                Debug.Log($"Device: {deviceIndex} not found! Turning streaming off...");
                return;
            }
            
            PlayerPrefs.SetString(PLAYERPREFS_VIDEOOUTPUT, DevicesList[deviceIndex].Id);
            PlayerPrefs.Save();

            CurrentDevice = foundDevice;
            spaceConnection.UpdateVideoInput(DevicesList[deviceIndex]);
            
            if (IsBlackDevice(foundDevice))
            {
                SetCanStream(false);
                spaceConnection.SetLocalUserStreamingState(false);
            }
            else
            {
                SetCanStream(true);
                spaceConnection.SetLocalUserStreamingState(true);
            }
        }

        public CavrnusDeferredDisposable FetchVideoDevices(Action<List<CavrnusVideoInputDevice>> onFoundDevices)
        {
            var dd = new CavrnusDeferredDisposable();
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
            {
                spaceConnection = spaceConn;
                
                DevicesDictionary = new();
                DevicesList = new();
                
                dd.Set(spaceConnection.FetchVideoInputs(foundDevices =>
                {
                    SetCanStream(false);

                    foundDevices.ForEach(vid =>
                    {
                        if (!IsValidDevice(vid)) 
                            return;

                        if (IsSavedDevice(vid))
                        {
                            CurrentDevice = vid;
                            SetCanStream(true);
                        }
                    
                        RenameDevice(vid);
                    
                        DevicesList.Add(vid);
                        DevicesDictionary.Add(vid.Id, vid);
                    });
                
                    onFoundDevices?.Invoke(DevicesList);
                }));
            });

            return dd;
        }

        private bool IsSavedDevice(CavrnusVideoInputDevice device)
        {
            return String.Equals(device.Id, PlayerPrefs.GetString(PLAYERPREFS_VIDEOOUTPUT));
        }
        
        private void RenameDevice(CavrnusVideoInputDevice device)
        {
            if (IsBlackDevice(device))
                device.Name = "Off";
        }

        private bool IsBlackDevice(CavrnusVideoInputDevice device)
        {
            return String.Equals(device.Id, OFF_DEVICE_ID, StringComparison.OrdinalIgnoreCase);
        }
        
        private bool IsValidDevice(CavrnusVideoInputDevice device)
        {
            return !String.Equals(device.Id, "application", StringComparison.OrdinalIgnoreCase);
        }

        private void SetCanStream(bool state)
        {
            CanStreamState = state;
            CanStreamChanged?.Invoke(state);
        }
    }
}