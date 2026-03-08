// Decompiled with JetBrains decompiler
// Type: FM.LiveSwitch.Unity.CavrnusLSAudioClipSource
// Assembly: FM.LiveSwitch.Unity, Version=1.18.7.35868, Culture=neutral, PublicKeyToken=null
// MVID: B1622BEC-3FC2-452D-B1B4-A1E027B7C24F
// Assembly location: E:\Git\cavrnus-package-development\Assets\com.cavrnus.csc\CavrnusSdk\Runtime\Scripts\Core\CavrnusPlugins\RTC\LiveSwitch\FM.LiveSwitch.Unity.dll
// XML documentation location: E:\Git\cavrnus-package-development\Assets\com.cavrnus.csc\CavrnusSdk\Runtime\Scripts\Core\CavrnusPlugins\RTC\LiveSwitch\FM.LiveSwitch.Unity.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace FM.LiveSwitch.Unity
{
	/// <summary>An AudioClip-based audio source.</summary>
	public class CavrnusLSAudioClipSource : FM.LiveSwitch.AudioSource
  {
    private static ILog _Log = Log.GetLogger(typeof (CavrnusLSAudioClipSource));
    private const int LATENCY = 40;
    private GameObject _GameObject;
    private EventBehaviour _CoreBehaviour;
    private string _Device;
    private int _RecordClockRate;
    private int _RecordChannelCount;
    private long _ReadPositionInSamples;
    private long _BaseWritePositionInSamples;
    private int _LastCompactWritePositionInSamples;
    private int _FrameDurationInSamples;
    private long _FrameDurationInTicks;
    private long _SystemTimestamp;
    private int _ClipLengthInSamples;
    private AudioClip _Clip;
    private AudioClip _TestClip;
    private float[] _FloatBuffer;
    private SoundConverter _Converter;
    private IDispatchQueue<AudioFrame> _DispatchQueue;
    private static IDataBufferPool _DataBufferPool = (IDataBufferPool) DataBufferPool.GetTracer(typeof (CavrnusLSAudioClipSource));

    /// <summary>Gets the buffer delay.</summary>
    /// <param name="config">The config.</param>
    public static int GetBufferDelay(AudioConfig config) => 40;

    /// <summary>Gets a label that identifies this class.</summary>
    public override string Label => "Unity AudioClip Source";

    /// <summary>Gets the game object.</summary>
    public GameObject GameObject => this._GameObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:FM.LiveSwitch.Unity.CavrnusLSAudioClipSource" /> class.
    /// </summary>
    public CavrnusLSAudioClipSource()
      : this(FM.LiveSwitch.Opus.Format.DefaultConfig)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:FM.LiveSwitch.Unity.CavrnusLSAudioClipSource" /> class.
    /// </summary>
    public CavrnusLSAudioClipSource(AudioConfig config)
      : base((AudioFormat) new FM.LiveSwitch.Pcm.Format(config))
    {
      this._GameObject = new GameObject();
      this._CoreBehaviour = this._GameObject.AddComponent<EventBehaviour>();
      this.OutputSynchronizable = true;
      this._DispatchQueue = (IDispatchQueue<AudioFrame>) new DispatchQueue<AudioFrame>((Action1<AudioFrame>) (frame =>
      {
        AudioBuffer lastBuffer = frame.LastBuffer;
        try
        {
          this._Converter.ProcessFrame(frame);
        }
        catch (Exception ex)
        {
          CavrnusLSAudioClipSource._Log.Error("Could not raise audio frame.", ex);
        }
        finally
        {
          lastBuffer.Free();
        }
      }));
    }

    /// <summary>Destroys this instance.</summary>
    protected override void DoDestroy()
    {
      if ((UnityEngine.Object) this._CoreBehaviour != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this._CoreBehaviour);
        this._CoreBehaviour = (EventBehaviour) null;
      }
      if ((UnityEngine.Object) this._GameObject != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this._GameObject);
        this._GameObject = (GameObject) null;
      }
      base.DoDestroy();
    }

    /// <summary>Gets the available inputs.</summary>
    public override Future<SourceInput[]> GetInputs()
    {
      Promise<SourceInput[]> inputs = new Promise<SourceInput[]>();
      List<SourceInput> sourceInputList = new List<SourceInput>();
      foreach (string device in Microphone.devices)
        sourceInputList.Add(CavrnusLSAudioClipSource.InputFromDevice(device));
      inputs.Resolve(sourceInputList.ToArray());
      return (Future<SourceInput[]>) inputs;
    }

    private static SourceInput InputFromDevice(string device) => new SourceInput(device, device);

    private static string DeviceFromInput(SourceInput input)
    {
      foreach (string device in Microphone.devices)
      {
        if (device == input.Id)
          return device;
      }
      return (string) null;
    }

    private static bool InputExists(SourceInput input)
    {
      foreach (string device in Microphone.devices)
      {
        if (device == input.Id)
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
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
          SourceInput input;
          for (this._Device = (string) null; this._Device == null; this._Device = CavrnusLSAudioClipSource.DeviceFromInput(input))
          {
            input = this.Input;
            if (input == null || !CavrnusLSAudioClipSource.InputExists(input))
            {
              string[] devices = Microphone.devices;
              input = devices.Length != 0 ? CavrnusLSAudioClipSource.InputFromDevice(((IEnumerable<string>) devices).First<string>()) : throw new Exception("No audio devices found.");
            }
          }

          while (!IsMicrophoneValid())
          {
            CavrnusLSAudioClipSource._Log.Debug("Attempting to start Microphone...");
          }
          
          int minFreq;
          int maxFreq;
          Microphone.GetDeviceCaps(this._Device, out minFreq, out maxFreq);
          if (minFreq > 0 && maxFreq > 0)
            CavrnusLSAudioClipSource._Log.Debug(this.Id, string.Format("Microphone has minimum clock rate of {0}Hz and maximum clock rate of {1}Hz.", (object) minFreq, (object) maxFreq));
          else if (minFreq > 0)
            CavrnusLSAudioClipSource._Log.Debug(this.Id, string.Format("Microphone has minimum clock rate of {0}Hz.", (object) minFreq));
          else if (maxFreq > 0)
            CavrnusLSAudioClipSource._Log.Debug(this.Id, string.Format("Microphone has maximum clock rate of {0}Hz.", (object) maxFreq));
          else
            CavrnusLSAudioClipSource._Log.Debug(this.Id, "Microphone has no minimum or maximum clock rate.");
          this._RecordClockRate = this.Config.ClockRate;
          if (maxFreq > 0 && this._RecordClockRate > maxFreq)
            this._RecordClockRate = maxFreq;
          if (minFreq > 0 && this._RecordClockRate < minFreq)
            this._RecordClockRate = minFreq;
          this._RecordChannelCount = 1;
          int frameDuration = this.FrameDuration;
          this._Clip = Microphone.Start(this._Device, true, this.FrameDuration, this._RecordClockRate);
          this._SystemTimestamp = -1L;
          this._ClipLengthInSamples = this._Clip.samples;
          this._RecordClockRate = this._Clip.frequency;
          this._ReadPositionInSamples = 0L;
          this._BaseWritePositionInSamples = 0L;
          this._LastCompactWritePositionInSamples = 0;
          this._FrameDurationInSamples = SoundUtility.CalculateTimestampDeltaFromDuration(this.FrameDuration, this._RecordClockRate) * this._RecordChannelCount;
          this._FrameDurationInTicks = (long) (this.FrameDuration * Constants.TicksPerMillisecond);
          this._FloatBuffer = new float[this._FrameDurationInSamples];
          CavrnusLSAudioClipSource._Log.Debug(this.Id, string.Format("Microphone started with clock rate of {0}Hz.", (object) this._RecordClockRate));
          this._Converter = new SoundConverter(new AudioConfig(this._RecordClockRate, this._RecordChannelCount), this.Config);
          this._Converter.OnRaiseFrame += (Action1<AudioFrame>) (frame => this.RaiseFrame(frame));
          if (this._RecordClockRate != this.Config.ClockRate || this._RecordChannelCount != this.Config.ChannelCount)
            CavrnusLSAudioClipSource._Log.Debug(this.Id, string.Format("Microphone audio will be resampled from {0}Hz {1}-channel to {2}Hz {3}-channel.", (object) this._RecordClockRate, (object) this._RecordChannelCount, (object) this.Config.ClockRate, (object) this.Config.ChannelCount));
          this._CoreBehaviour.OnUpdate += new Action(this.Update);
          this._CoreBehaviour.OnFixedUpdate += new Action(this.Update);
          promise.Resolve((object) true);
        }
        else
          promise.Reject(new Exception("Microphone access has not been authorized."));
      }
      catch (Exception ex)
      {
        promise.Reject(ex);
      }
      return (Future<object>) promise;
    }

    /// <summary>Stops this instance.</summary>
    protected override Future<object> DoStop()
    {
      Promise<object> promise = new Promise<object>();
      try
      {
        this._CoreBehaviour.OnUpdate -= new Action(this.Update);
        this._CoreBehaviour.OnFixedUpdate -= new Action(this.Update);
        if (this._Device != null)
        {
          Microphone.End(this._Device);
          this._Device = (string) null;
        }
        if (this._Converter != null)
        {
          this._Converter.Destroy();
          this._Converter = (SoundConverter) null;
        }
        promise.Resolve((object) true);
      }
      catch (Exception ex)
      {
        promise.Reject(ex);
      }
      return (Future<object>) promise;
    }

    private void Update()
    {
      if (!Microphone.IsRecording(_Device))
      {
        DoStop();
        DoStart();

        return;
      }
      
      int position = Microphone.GetPosition(this._Device);
      if (position < this._LastCompactWritePositionInSamples)
        this._BaseWritePositionInSamples += (long) this._ClipLengthInSamples;
      this._LastCompactWritePositionInSamples = position;
      long num1 = this._BaseWritePositionInSamples + (long) position;
      if (this._SystemTimestamp == -1L)
        this._SystemTimestamp = ManagedStopwatch.GetTimestamp();
      while (num1 - this._ReadPositionInSamples >= (long) this._FrameDurationInSamples)
      {
        if (this._Clip.GetData(this._FloatBuffer, (int) (this._ReadPositionInSamples % (long) this._ClipLengthInSamples)))
        {
          DataBuffer dataBuffer = CavrnusLSAudioClipSource._DataBufferPool.Take(this._FrameDurationInSamples * 2, true);
          int offset = 0;
          foreach (float num2 in this._FloatBuffer)
          {
            short num3 = SoundUtility.ShortFromFloat(num2);
            dataBuffer.Write16((int) num3, offset);
            offset += 2;
          }
          AudioFrame audioFrame = new AudioFrame(this.FrameDuration, new AudioBuffer(dataBuffer, this._Converter.InputFormat));
          audioFrame.SystemTimestamp = this._SystemTimestamp;
          this._DispatchQueue.Enqueue(audioFrame);
          this._SystemTimestamp += this._FrameDurationInTicks;
          this._ReadPositionInSamples += (long) this._FrameDurationInSamples;
        }
      }
    }

    private bool IsMicrophoneValid()
    {
      if (Microphone.devices.Contains(_Device))
      {
        _TestClip = Microphone.Start(_Device, true, 5, 44100);

        // Wait until the microphone starts recording or timeout after 5 seconds.
        // We need to test over a span of time to ensure mic is indeed working.
        var startTime = Time.time;
        while (!Microphone.IsRecording(_Device))
        {
          if (Time.time - startTime > 5f)
          {
            _Log.Debug("Microphone failed to start within the timeout period.");
            Microphone.End(_Device);
            return false;
          }

          System.Threading.Thread.Sleep(100); // Doing this so we don't completely spam
        }

        _Log.Debug("Microphone found and is valid!");

        Microphone.End(_Device);
        return true;
      }

      _Log.Debug("Microphone device not found.");
      return false;
    }
  }
}