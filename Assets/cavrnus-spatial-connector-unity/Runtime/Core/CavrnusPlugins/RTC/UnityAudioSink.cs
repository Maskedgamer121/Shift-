// Decompiled with JetBrains decompiler
// Type: FM.LiveSwitch.Unity.AudioClipSink
// Assembly: FM.LiveSwitch.Unity, Version=1.12.3.46193, Culture=neutral, PublicKeyToken=null
// MVID: 2583CD92-3F1E-4E7C-87AE-B423E8600AFD
// Assembly location: C:\Cavrnus\cavrnus-client\src\Unity\AppCollab\Assets\Liveswitch\Plugins\FM.LiveSwitch.Unity.dll

using System;
using FM.LiveSwitch;
using FM.LiveSwitch.Unity;
using UnityEngine;

namespace Cavrnus.RTC
{
    /// <summary>An AudioClip-based audio sink, modified from decompiled LS examples.</summary>
	public class UnityAudioSink : AudioSink
    {
        private const int LATENCY = 100;

        /// <summary>
        /// Gets the buffer delay.
        /// </summary>
        /// <param name="config">The config.</param>
        public static int GetBufferDelay(AudioConfig config)
        {
            return LATENCY;
        }

        /// <summary>
        /// Gets a label that identifies this class.
        /// </summary>
        public override string Label
        {
            get { return "Unity AudioClip Sink"; }
        }

        /// <summary>
        /// Gets the game object.
        /// </summary>
        public GameObject GameObject
        {
            get { return _GameObject; }
        }

        private GameObject _GameObject;
        private EventBehaviour _CoreBehaviour;

        private long _WritePositionInSamples;
        private int _CompactWritePositionInSamples;
        private int _LatencyInSamples;
        private bool _WriteBuffering;

        private UnityEngine.AudioSource _Source;
        private AudioClip _Clip;
        private float[] _FloatBuffer;
        private float[] _FloatBufferFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioClipSink" /> class.
        /// </summary>
        public UnityAudioSink(AudioConfig config)
            : base(new FM.LiveSwitch.Pcm.Format(config))
        {
            _GameObject = new GameObject();

            _CoreBehaviour = _GameObject.AddComponent<EventBehaviour>();
            _CoreBehaviour.OnUpdate += Update;

            var clipLengthInFrames = Config.ClockRate * 1; // seconds
            _Clip = AudioClip.Create(Label, clipLengthInFrames, Config.ChannelCount, Config.ClockRate, false);

            var clipLengthInSamples = clipLengthInFrames * Config.ChannelCount;
            _FloatBuffer = new float[clipLengthInSamples];

            _WritePositionInSamples = 0;
            _CompactWritePositionInSamples = 0;
            _LatencyInSamples = Config.ClockRate * Config.ChannelCount * LATENCY / 1000;
            _WriteBuffering = true;

            _Source = _GameObject.AddComponent<UnityEngine.AudioSource>();
            _Source.clip = _Clip;
            _Source.loop = true;
        }


        private long _ReadPositionInSamples = 0L;
        private void Pcmreadercallback(float[] data)
        {
	        if (this._ReadPositionInSamples + data.Length > this._FloatBuffer.Length) // two copies, end then begining.
	        {
		        Array.Copy(this._FloatBuffer, this._ReadPositionInSamples, data, 0L, this._FloatBuffer.Length - this._ReadPositionInSamples);
		        long remaining = data.Length - (this._FloatBuffer.Length - this._ReadPositionInSamples);
		        Array.Copy(this._FloatBuffer, 0L, data, this._FloatBuffer.Length - this._ReadPositionInSamples, remaining);
		        this._ReadPositionInSamples = remaining;
	        }
	        else
	        {
		        Array.Copy(this._FloatBuffer, this._ReadPositionInSamples, data, 0L, data.Length);
		        this._ReadPositionInSamples += data.Length;
	        }
        }


        /// <summary>
        /// Destroys this instance.
        /// </summary>
        protected override void DoDestroy()
        {
            if (_Source != null)
            {
                _Source.Stop();
                _Source.loop = false;
                _Source.clip = null;
                _Source.time = 0;

                UnityEngine.Object.Destroy(_Source);
                _Source = null;
            }

            if (_CoreBehaviour != null)
            {
                _CoreBehaviour.OnUpdate -= Update;
                UnityEngine.Object.Destroy(_CoreBehaviour);
                _CoreBehaviour = null;
            }

            if (_GameObject != null)
            {
                UnityEngine.Object.Destroy(_GameObject);
                _GameObject = null;
            }
        }

        /// <summary>
        /// Processes a frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="inputBuffer">The input buffer.</param>
        protected override void DoProcessFrame(AudioFrame frame, AudioBuffer inputBuffer)
        {
            Write(inputBuffer.DataBuffer);
        }

        private void Update()
        {
            if (_WriteBuffering)
            {
                if (_WritePositionInSamples < _LatencyInSamples)
                {
                    // not yet
                    return;
                }

                _WriteBuffering = false;
                _Source.Play();
            }

            _Clip.SetData(_FloatBuffer, 0);
        }

        private void Write(DataBuffer dataBuffer)
        {
            if (_FloatBufferFrame == null)
            {
                _FloatBufferFrame = new float[dataBuffer.Length / 2];
            }
            var floatBufferOffset = 0;
            for (var i = 0; i < dataBuffer.Length; i += 2)
            {
                var shortSample = (short)dataBuffer.Read16Signed(i);
                _FloatBufferFrame[floatBufferOffset] = SoundUtility.FloatFromShort(shortSample);
                floatBufferOffset++;
            }

            if (_CompactWritePositionInSamples + _FloatBufferFrame.Length <= _FloatBuffer.Length)
            {
                Array.Copy(_FloatBufferFrame, 0, _FloatBuffer, _CompactWritePositionInSamples, _FloatBufferFrame.Length);
                _CompactWritePositionInSamples += _FloatBufferFrame.Length;
            }
            else
            {
                int length1 = _FloatBuffer.Length - _CompactWritePositionInSamples;
                int length2 = _FloatBufferFrame.Length - length1;

                Array.Copy(_FloatBufferFrame, 0, _FloatBuffer, _CompactWritePositionInSamples, length1);
                Array.Copy(_FloatBufferFrame, length1, _FloatBuffer, 0, _FloatBufferFrame.Length - length2);

                _CompactWritePositionInSamples = length2;
            }

            _WritePositionInSamples += _FloatBufferFrame.Length;
        }
    }
}