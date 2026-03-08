using System;
using System.Threading;
using FM.LiveSwitch;
using FM.LiveSwitch.Unity;
using Unity.Collections;
using UnityEngine;

namespace Cavrnus.RTC
{

	/// <summary>Based off of decompiled LSUnity.TextureSink.</summary>
	public class TextureSink : ViewSink<Texture2D>
    {
        private GameObject _UpdateListenerObject;
        private EventBehaviour _CoreBehaviour;
        private Texture2D _Texture;
        private VideoBuffer _VideoBuffer;
        private bool _TextureMipChain;
        private bool _TextureLinear;

        /// <summary>Gets a label that identifies this class.</summary>
        public override string Label => "Unity RectTransform Sink";

        /// <summary>Gets the view.</summary>
        public override Texture2D View => this._Texture;

        public override LayoutScale ViewScale { get; set; } // ignored?

        /// <summary>
        /// Gets or sets a value indicating whether the view is mirrored.
        /// </summary>
        public override bool ViewMirror { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mipChain is enabled.
        /// </summary>
        public bool TextureMipChain
        {
            get => this._TextureMipChain;
            set => this._TextureMipChain = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether linear is enabled.
        /// </summary>
        public bool TextureLinear
        {
            get => this._TextureLinear;
            set => this._TextureLinear = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FM.LiveSwitch.Unity.RectTransformSink" /> class.
        /// </summary>
        public TextureSink()
          : this(VideoFormat.Rgb)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FM.LiveSwitch.Unity.RectTransformSink" /> class.
        /// </summary>
        public TextureSink(VideoFormat inputFormat)
          : base(inputFormat)
        {
            if (!TextureSink.GetTextureFormat(inputFormat.Name).HasValue)
                throw new Exception("Unsupported format. Supported formats: ARGB, RGBA, BGRA, RGB");
            this.ViewScale = LayoutScale.Contain;
            this.ViewMirror = false;
            this._UpdateListenerObject = new GameObject("RemoteMedia TextureSink Update Listener");
            this._Texture = new Texture2D(0, 0, TextureFormat.RGB24, this._TextureMipChain, this._TextureLinear);
            this._CoreBehaviour = this._UpdateListenerObject.AddComponent<EventBehaviour>();
            this._CoreBehaviour.OnUpdate += new Action(this.Update);
        }

        /// <summary>Destroys this instance.</summary>
        protected override void DoDestroy()
        {
            base.DoDestroy();
            if ((UnityEngine.Object)this._CoreBehaviour != (UnityEngine.Object)null)
            {
                this._CoreBehaviour.OnUpdate -= new Action(this.Update);
                UnityEngine.Object.Destroy((UnityEngine.Object)this._CoreBehaviour);
                this._CoreBehaviour = (EventBehaviour)null;
            }
            if ((UnityEngine.Object)this._UpdateListenerObject != (UnityEngine.Object)null)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)this._UpdateListenerObject);
                this._UpdateListenerObject = (GameObject)null;
            }

            if (this._Texture != null)
            {
                Texture2D.Destroy(this._Texture);
                this._Texture = null;
            }
        }

        private static TextureFormat? GetTextureFormat(string formatName)
        {
            if (formatName == VideoFormat.ArgbName)
                return new TextureFormat?(TextureFormat.ARGB32);
            if (formatName == VideoFormat.RgbaName)
                return new TextureFormat?(TextureFormat.RGBA32);
            if (formatName == VideoFormat.BgraName)
                return new TextureFormat?(TextureFormat.BGRA32);
            return formatName == VideoFormat.RgbName ? new TextureFormat?(TextureFormat.RGB24) : new TextureFormat?();
        }

        /// <summary>Renders the buffer.</summary>
        /// <param name="inputBuffer">The input buffer.</param>
        protected override void RenderBuffer(VideoBuffer inputBuffer) => Interlocked.Exchange<VideoBuffer>(ref this._VideoBuffer, inputBuffer.Keep())?.Free();
    
        private void Update()
        {
	        if (_Texture == null)
		        return;

            VideoBuffer videoBuffer = Interlocked.Exchange<VideoBuffer>(ref this._VideoBuffer, (VideoBuffer)null);
            if (videoBuffer == null)
                return;
            try
            {
                int width = videoBuffer.Width;
                int height = videoBuffer.Height;
                if (this._Texture.width != width || this._Texture.height != height)
                    this._Texture.Reinitialize(width, height);
                int dstIndex = 0;
                NativeArray<byte> rawTextureData = this._Texture.GetRawTextureData<byte>();
                foreach (DataBuffer dataBuffer in videoBuffer.DataBuffers)
                {
                    NativeArray<byte>.Copy(dataBuffer.Data, dataBuffer.Index, rawTextureData, dstIndex, dataBuffer.Length);
                    dstIndex += dataBuffer.Length;
                }
                this._Texture.Apply();
            }
            finally
            {
                videoBuffer.Free();
            }
        }
    }
}