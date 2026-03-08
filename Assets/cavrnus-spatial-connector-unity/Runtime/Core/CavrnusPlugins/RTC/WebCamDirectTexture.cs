using System;
using System.Collections.Generic;
using System.Linq;
using FM.LiveSwitch;
using FM.LiveSwitch.Unity;
using UnityEngine;

#if !UNITY_MAGICLEAP && !BUILD_ANDROID_OCULUS_QUEST && !UNITY_VISIONOS

namespace Cavrnus.RTC
{
	/// <summary>A WebCamTexture-based video source. Decompiled from LS and modified for use.</summary>
	public class WebCamDirectTextureSource : CameraSourceBase
    {
        private static ILog _Log = Log.GetLogger(typeof(WebCamTextureSource));
        private GameObject _UpdateListenerObject;
        private EventBehaviour _EventBehaviour;
        private WebCamDevice? _Device;
        private Color32[] _CameraPixels;
        private byte[] _OutputBytes;
        private bool _CameraVerticalMirror;
        private bool _LastCameraVerticalMirror;
        private int _CameraRotation;
        //private int _LastCameraRotation;
        private bool _LastViewMirror;
       // private LayoutScale _LastViewScale = LayoutScale.Stretch;
        private double _LastOuterAspectRatio;
        private double _LastInnerAspectRatio;
        private int _Width = -1;
        private int _Height = -1;
        private int _Stride = -1;
        private volatile bool _RaisingFrame;

        /// <summary>Gets a label that identifies this class.</summary>
        public override string Label => "Unity WebCamTexture Source";

        /// <summary>Gets the view.</summary>
        public Texture View => this.WebCamTexture;

        /// <summary>Gets the device.</summary>
        public WebCamDevice? Device => this._Device;

        /// <summary>Gets the game object.</summary>
        public GameObject GameObject => this._UpdateListenerObject;

        /// <summary>Gets or sets the view scale.</summary>
        public LayoutScale ViewScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the view is mirrored.
        /// </summary>
        public bool ViewMirror { get; set; }

        /// <summary>Gets the underlying WebCamTexture.</summary>
        public WebCamTexture WebCamTexture { get; private set; }

        /// <summary>
        /// Gets whether the device list is sorted to prefer
        /// front-facing cameras.
        /// </summary>
        public static bool PreferFrontFacing { get; private set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FM.LiveSwitch.Unity.WebCamTextureSource" /> class.
        /// </summary>
        public WebCamDirectTextureSource(VideoConfig targetConfig)
          : base(VideoFormat.Rgba, targetConfig)
        {
            this.ViewScale = LayoutScale.Contain;
            this._UpdateListenerObject = new GameObject("WebcamDirectTexture");
            this._UpdateListenerObject.AddComponent<CanvasRenderer>();
            this._EventBehaviour = this._UpdateListenerObject.AddComponent<EventBehaviour>();
        }

        /// <summary>Destroys this instance.</summary>
        protected override void DoDestroy()
        {
            if ((UnityEngine.Object)this._EventBehaviour != (UnityEngine.Object)null)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)this._EventBehaviour);
                this._EventBehaviour = (EventBehaviour)null;
            }
            if ((UnityEngine.Object)this._UpdateListenerObject != (UnityEngine.Object)null)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)this._UpdateListenerObject);
                this._UpdateListenerObject = (GameObject)null;
            }
            base.DoDestroy();
        }

        /// <summary>Gets the available inputs.</summary>
        public override Future<SourceInput[]> GetInputs()
        {
            Promise<SourceInput[]> promise = new Promise<SourceInput[]>();
            promise.Resolve(GetCameraInputs().ToArray());
            return (Future<SourceInput[]>)promise;
        }

        public static List<SourceInput> GetCameraInputs()
        {
	        return GetDevices().Select(d => InputFromDevice(d)).ToList();
        }

        private static WebCamDevice[] GetDevices() => PreferFrontFacing ? ((IEnumerable<WebCamDevice>)WebCamTexture.devices).OrderByDescending<WebCamDevice, bool>((Func<WebCamDevice, bool>)(x => x.isFrontFacing)).ToArray<WebCamDevice>() : WebCamTexture.devices;

        private static SourceInput InputFromDevice(WebCamDevice device) => new SourceInput(device.name, device.name);

        private WebCamDevice? DeviceFromInput(SourceInput input)
        {
            foreach (WebCamDevice device in GetDevices())
            {
                if (input?.Id == null || device.name == input.Id)
                    return new WebCamDevice?(device);
            }
            return new WebCamDevice?();
        }

        private bool InputExists(SourceInput input)
        {
            foreach (WebCamDevice device in GetDevices())
            {
                if (device.name == input.Id)
                    return true;
            }
            return false;
        }

        /// <summary>Starts this instance.</summary>
        protected override Future<object> DoStart()
        {
            Promise<object> promise = new Promise<object>();
            try
            {
                if (Application.HasUserAuthorization(UserAuthorization.WebCam))
                {
                    SourceInput input;
                    for (this._Device = new WebCamDevice?(); !this._Device.HasValue; this._Device = this.DeviceFromInput(input))
                    {
                        input = this.Input;
                        if (input == null || !this.InputExists(input))
                        {
                            WebCamDevice[] devices = GetDevices();
                            input = devices.Length != 0 ? InputFromDevice(((IEnumerable<WebCamDevice>)devices).First<WebCamDevice>()) : throw new Exception("No video devices found.");
                        }
                    }
                    WebCamDevice webCamDevice = this._Device.Value;
                    this.ViewMirror = webCamDevice.isFrontFacing;
                    //this._LastCameraRotation = -1;
                    this._Width = -1;
                    this._Height = -1;
                    this._Stride = -1;
                    this.WebCamTexture = new WebCamTexture(webCamDevice.name, this.TargetConfig.Width, this.TargetConfig.Height, (int)this.TargetConfig.FrameRate);
                    this.WebCamTexture.Play();
                    this._EventBehaviour.OnUpdate += new Action(this.Update);
                    promise.Resolve((object)true);
                }
                else
                    promise.Reject(new Exception("Camera access has not been authorized."));
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return (Future<object>)promise;
        }

        /// <summary>Stops this instance.</summary>
        protected override Future<object> DoStop()
        {
            Promise<object> promise = new Promise<object>();
            try
            {
                this._EventBehaviour.OnUpdate -= new Action(this.Update);
                if ((UnityEngine.Object)this.WebCamTexture != (UnityEngine.Object)null)
                {
                    this.WebCamTexture.Stop();
                    this.WebCamTexture = (WebCamTexture)null;
                }
                promise.Resolve((object)true);
            }
            catch (Exception ex)
            {
                promise.Reject(ex);
            }
            return (Future<object>)promise;
        }

        private void ApplyCameraVerticalMirror()
        {
        }

        private void ApplyCameraRotation()
        {
        }

        private void ApplyViewMirror()
        {
        }

        private void ApplyViewScale()
        {
        }

        private void Update()
        {
            if (!this.WebCamTexture.didUpdateThisFrame)
                return;
            if (this._Width == -1 || this._Height == -1)
            {
                this._Width = this.WebCamTexture.width;
                this._Height = this.WebCamTexture.height;
                this._Stride = this._Width * 4;
                this.Config = new VideoConfig(this._Width, this._Height, (double)this.WebCamTexture.requestedFPS);
                this._CameraPixels = new Color32[this._Width * this._Height];
                this._OutputBytes = new byte[this._Stride * this._Height];
                WebCamDirectTextureSource._Log.Debug(this.Id, string.Format("Camera started with resolution {0}x{1}.", (object)this._Width, (object)this._Height));
            }
            this.ApplyCameraVerticalMirror();
            this.ApplyCameraRotation();
            this.ApplyViewMirror();
            this.ApplyViewScale();
            if (this._RaisingFrame)
                return;
            this.WebCamTexture.GetPixels32(this._CameraPixels);
            int width = this._Width;
            int height = this._Height;
            int orientation = (360 - this._CameraRotation) % 360;
            this._RaisingFrame = true;
            ManagedThread.Dispatch((Action0)(() =>
            {
                try
                {
                    // ISSUE: explicit non-virtual call
                    this._CameraPixels.AsByteArray((Action<byte[]>)(inputBytes =>
                    {
	                    this.RaiseFrame(new VideoFrame(
		                    new VideoBuffer(width, height, DataBuffer.Wrap(inputBytes), this.OutputFormat)
		                    {
			                    Orientation = orientation,
			                    VerticallyMirrored = true
		                    }));
                    }));
                }
                catch (Exception ex)
                {
                    WebCamDirectTextureSource._Log.Error(this.Id, "Could not raise frame.", ex);
                }
                finally
                {
                    this._RaisingFrame = false;
                }
            }));
        }
    }
}
#endif