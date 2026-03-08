using System;
using System.Linq;
using Cavrnus.Base.Core;
using Cavrnus.Base.Graphics;
using Cavrnus.Base.Settings;
using Cavrnus.RtcCommon;
using FM.LiveSwitch;

namespace Cavrnus.RTC
{
	public class RtcSystemUnityLocal
	{
		public AecContextUnity AecContext { get; set; }

		public LocalMedia LocalMedia { get; private set; }

		public IReadonlySetting<float> CurrentTransmittingVolume { get; private set; } = new Setting<float>(0f);

		public IReadonlySetting<RtcInputSource> CurrentAudioInput { get; private set; } = new Setting<RtcInputSource>(null);
		public IReadonlySetting<RtcInputSource> CurrentVideoInput { get; private set; } = new Setting<RtcInputSource>(null);

		public delegate RtcLocalMedia<Image2D> CreateLocalMediaDelegate(RtcModeEnum mode, AecContext aec);
		private CreateLocalMediaDelegate createMediaDelegate;

		public RtcSystemUnityLocal()
		{
		}

		public void SetCurrentAudioInput(RtcInputSource newInputSource, Action success, Action<string> failure)
		{
			if (LocalMedia != null && !Equals(CurrentAudioInput.Value, newInputSource))
			{
				DebugOutput.Log($"Changing LocalMedia Audio Source Input to {newInputSource}.");
				if (!IsValidAudioInput(newInputSource))
				{
					DebugOutput.Log($"  Audio Source Input '{newInputSource}' is not a provided option; leaving unchanged.");
					failure?.Invoke($"  Audio Source Input '{newInputSource}' is not a provided option; leaving unchanged.");
					return;
				}

				LocalMedia.ChangeAudioSourceInput(newInputSource?.ToFM())
					.Then((o) =>
					{
						((Setting<RtcInputSource>)CurrentAudioInput).Value = newInputSource;
						DebugOutput.Log("Changed audio input to: "+(CurrentAudioInput.Value?.Name ?? "{Default}."));
						success?.Invoke();
					})
					.Fail((e) =>
					{
						DebugOutput.Warning("Failed to change to audio input: "+ (newInputSource?.Name ?? "{Default}. (" + e.ToString()+")"));
						failure?.Invoke(e.ToString());
					});
			}
			else
			{
				((Setting<RtcInputSource>)CurrentAudioInput).Value = newInputSource;
				success?.Invoke();
			}
		}
		
		public void SetCurrentVideoInput(RtcInputSource newInputSource, Action success, Action<string> failure)
		{
			if (LocalMedia != null && !Equals(CurrentVideoInput.Value, newInputSource))
			{
				DebugOutput.Log($"Changing LocalMedia Video Source Input to {newInputSource}.");
				if (!IsValidVideoInput(newInputSource))
				{
					DebugOutput.Log($"  Video Source Input '{newInputSource}' is not an available option; leaving unchanged.");
					failure?.Invoke($"  Video Source Input '{newInputSource}' is not an available option; leaving unchanged.");
					return;
				}

				LocalMedia.ChangeVideoSourceInput(newInputSource?.ToFM())
					.Then((o) =>
					{
						((Setting<RtcInputSource>)CurrentVideoInput).Value = newInputSource;
						DebugOutput.Log("Changed Video input to: " + (CurrentVideoInput.Value?.Name ?? "{Default}."));
						success?.Invoke();
					})
					.Fail((e) =>
					{
						DebugOutput.Warning("Failed to change to Video input: " + (newInputSource?.Name ?? "{Default}. (" + e.ToString() + ")"));
						failure?.Invoke(e.ToString());
					});
			}
			else
			{
				((Setting<RtcInputSource>)CurrentVideoInput).Value = newInputSource;
				success?.Invoke();
			}
		}
	
		public void Initialize(RtcModeEnum mode, bool enableAec)
		{
#if !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_IOS && !UNITY_VISIONOS
			if (enableAec && !RtcRegistrySettings.GetDisableAudioProcessing())
				AecContext = new AecContextUnity();
#endif

			try
			{
				LocalMedia = new LocalCompositeMedia(!mode.IsAudioEnabled(), !mode.IsVideoEnabled(), AecContext);
			}
			catch (Exception e)
			{
				DebugOutput.Info("Local Media initialization failure: " + e.ToString());
				return;
			}

			LocalMedia.OnAudioLevel += LocalMediaOnOnAudioLevel;
			LocalMedia.Initialize();

			if (mode.IsAudioEnabled())
			{
				if (IsValidAudioInput(CurrentAudioInput.Value))
					LocalMedia.ChangeAudioSourceInput(CurrentAudioInput.Value?.ToFM())?.WaitForResult();
				else
				{
					DebugOutput.Log($"Initial audio input '{CurrentAudioInput.Value?.Name}' is not available, leaving audio at default.");
					(CurrentAudioInput as ISetting<RtcInputSource>).Value = null;
				}
			}

			if (mode.IsVideoEnabled())
			{
				if (IsValidVideoInput(CurrentVideoInput.Value))
					LocalMedia.ChangeVideoSourceInput(CurrentVideoInput.Value?.ToFM())?.WaitForResult();
				else
				{
					DebugOutput.Log($"Initial video input '{CurrentAudioInput.Value?.Name}' is not available, leaving video at default (black).");
					(CurrentVideoInput as ISetting<RtcInputSource>).Value = null;
				}
			}
			LocalMedia.Start().WaitForPromise();
		}

		private bool IsValidAudioInput(RtcInputSource input)
		{
			if (input == null)
				return true;
			if (LocalMedia == null)
				return false;
			var inputs = LocalMedia.GetAudioSourceInputs().WaitForResult();
			return inputs.Any(i => i.Id == input.Id);
		}

		private bool IsValidVideoInput(RtcInputSource input)
		{
			if (LocalMedia == null)
				return false;
			if (input == null)
				return true;
			var inputs = LocalMedia.GetVideoSourceInputs().WaitForResult();
			return inputs.Any(i => i.Id == input.Id);
		}

		private void LocalMediaOnOnAudioLevel(double p)
		{
			((ISetting<float>) CurrentTransmittingVolume).Value = (float) p;
		}

		public void Shutdown()
		{
			try
			{
				var mediaImplClosure = LocalMedia;
				LocalMedia = null;
				if (mediaImplClosure != null)
				{
					string msg;
					mediaImplClosure.Stop()
						.Then((o)=>msg="YES!")
						.Fail((err)=>msg=err.ToString())
						.WaitForPromise();
				}
				//				mediaImplClosure?.AudioTrack?.Source?.Stop()?.WaitForResult();
				//				mediaImplClosure?.VideoTrack?.Source?.Stop()?.WaitForResult();
				mediaImplClosure?.Destroy();
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error shutting down local media: " + e.ToString());
			}
			LocalMedia = null;

			try
			{
				AecContext?.Destroy();
			}
			catch (Exception e)
			{
				DebugOutput.Error("Error shutting down AecContext: " + e.ToString());
			}
			AecContext = null;
		}
		
		public void FetchInputOptions(Action<RtcInputSource[]> inputsCb)
		{
			if (LocalMedia == null)
			{
				inputsCb(new RtcInputSource[0]);
				return;
			}
			LocalMedia.GetAudioSourceInputs().Then(inputs =>
			{
				DebugOutput.Log($"Received input options ({inputs.Length})");
				var sourceInputOptions = inputs.Select(i => RtcHelpers.FromFM(i)).ToArray();
				inputsCb(sourceInputOptions);
			}).Fail((e) =>
			{
				DebugOutput.Info($"Failed acquiring input options {e}");
				var sourceInputOptions = new RtcInputSource[0];
				inputsCb(sourceInputOptions);
			});
		}

		public void FetchVideoInputOptions(Action<RtcInputSource[]> inputsCb)
		{
			if (LocalMedia == null)
			{
				inputsCb(new RtcInputSource[0]);
				return;
			}
			LocalMedia.GetVideoSourceInputs().Then(inputs =>
			{
		    	var sourceVideoInputOptions = inputs.Select(i => RtcHelpers.FromFM(i)).ToArray();
				inputsCb(sourceVideoInputOptions);
			}).Fail((e) =>
			{
				DebugOutput.Info($"Failed acquiring video input options {e}");
				var sourceInputOptions = new RtcInputSource[0];
				inputsCb(sourceInputOptions);
			});
		}
	}
}