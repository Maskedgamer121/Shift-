using System;
using System.Linq;
using System.Threading;
using Cavrnus.Base.Core;
using Cavrnus.Base.Scheduler;
using Cavrnus.Base.Settings;
using Cavrnus.RtcCommon;
using FM.LiveSwitch;
using UnityEngine;

namespace Cavrnus.RTC
{
	public class RtcSystemUnityRemoteConnection
	{
		private readonly IScheduler scheduler;
		private readonly IRtcSystemCallback rtcCallback;
		private readonly RtcSystemUnityLocal mediaContext;

		public RemoteMedia RemoteMedia { get; private set; }
		private ConnectionInfo connectionInfo;
		private Channel liveChannel;
		private SfuDownstreamConnection liveDownStream;

		private SinkOutput lastAssignedSink = null;

		public string ConnectionId { get; private set; }
		public RtcModeEnum CurrentMode { get; private set; }

		public IReadonlySetting<RtcConnectionStatus> ConnectionStatus { get; private set; } = new Setting<RtcConnectionStatus>(RtcCommon.RtcConnectionStatus.New);
		public IReadonlySetting<RtcConnectionMethod> ConnectionMethod { get; private set; } = new Setting<RtcConnectionMethod>(RtcCommon.RtcConnectionMethod.Unknown);

		public IReadonlySetting<float> CurrentGain { get; private set; } = new Setting<float>(1f);
		public IReadonlySetting<float> CurrentReceivingVolume { get; private set; } = new Setting<float>(0f);

		public IReadonlySetting<long> TotalBytesReceived { get; private set; } = new Setting<long>(0);
		public IReadonlySetting<float> TotalSecondsLive { get; private set; } = new Setting<float>(0);

		public IReadonlySetting<Texture> VideoTexture { get; private set; } = new Setting<Texture>(null);

		private IScheduledHandle restartDownstreamTask = null;

		public IReadonlySetting<ConnectionStats> ConnectionStats { get; } = new Setting<ConnectionStats>(null);

		public RtcSystemUnityRemoteConnection(string connectionId, IScheduler scheduler, IRtcSystemCallback callback, RtcSystemUnityLocal mediaContext)
		{
			this.ConnectionId = connectionId;
			this.scheduler = scheduler;
			this.rtcCallback = callback;
			this.mediaContext = mediaContext;

			((Setting<RtcConnectionStatus>)ConnectionStatus).Value = RtcConnectionStatus.New;
		}

		public void Initialize(Channel channel, RtcModeEnum recvMode, ConnectionInfo c, SinkOutput initialOutput)
		{
			lock (this)
			{
				this.connectionInfo = c;
				this.CurrentMode = recvMode;

				if (!c.HasAudio)
					this.CurrentMode &= ~RtcModeEnum.Audio;
				if (!c.HasVideo)
					this.CurrentMode &= ~RtcModeEnum.Video;

				liveChannel = channel;
				lastAssignedSink = initialOutput;
			
				InitializeDownStream();
			}
		}


		public void UpdateMode(RtcModeEnum newMode)
		{
			if (CurrentMode == newMode)
				return;

			var oldmode = CurrentMode;
			CurrentMode = newMode;
			if (oldmode == RtcModeEnum.None && CurrentMode != RtcModeEnum.None)
			{
				InitializeDownStream();
			}
			else if (oldmode != RtcModeEnum.None && CurrentMode == RtcModeEnum.None)
			{
				ShutdownDownstreamAndMedia();
			}
			else
			{
				// Well, reconnect the remote.
				ShutdownDownstreamAndMedia();
				InitializeDownStream();
			}
		}

		private void InitializeDownStream(int reconnectAttemptsRemaining = 4)
		{
			if (liveChannel == null)
				return;
			if (CurrentMode == RtcModeEnum.None)
			{
				((Setting<RtcConnectionStatus>)ConnectionStatus).Value = RtcConnectionStatus.Disabled;

				return; // Eh, why bother?
			}

			lock (this)
			{
				RemoteMedia = new RemoteMedia(!CurrentMode.IsAudioEnabled(), !CurrentMode.IsVideoEnabled(), mediaContext.AecContext);

				DebugOutput.Log($"InitializeDownStream ({reconnectAttemptsRemaining})");

				((Setting<RtcConnectionStatus>)ConnectionStatus).Value = RtcConnectionStatus.Initializing;

				RemoteMedia.OnAudioLevel += RemoteMediaOnOnAudioLevel;

				RemoteMedia.Initialize();

				RemoteMedia.AudioSinkOutput = lastAssignedSink;

				RemoteMedia.AudioGain = CurrentGain.Value;

				((Setting<Texture>)VideoTexture).Value = RemoteMedia.ViewSink?.View;

				AudioStream audioStream = null;
				VideoStream videoStream = null;
				try
				{
					audioStream = (CurrentMode.IsAudioEnabled() && RemoteMedia.AudioTrack != null)
									? new AudioStream(null, RemoteMedia.AudioTrack)
									: null;
					if (audioStream != null)
						audioStream.BandwidthAdaptationPolicy = BandwidthAdaptationPolicy.Enabled;

					videoStream = (CurrentMode.IsVideoEnabled() && RemoteMedia.VideoTrack != null)
									? new VideoStream(null, RemoteMedia.VideoTrack)
									: null;
					if (videoStream != null)
					{
						videoStream.BandwidthAdaptationPolicy = BandwidthAdaptationPolicy.Enabled;
						videoStream.SimulcastMode = SimulcastMode.RtpStreamId;

						if (connectionInfo.VideoStream.SendEncodings != null)
						{
							// TODO: If the stream is already being lowered by a second listener, setting this seems to have no effect; the first stream is deactivated and this doesn't go through. Later reconfiguring seems to do the trick but is a little heavy handed.
							connectionInfo.VideoStream.SendEncodings = connectionInfo.VideoStream.SendEncodings
								.Select(enc => { enc.Deactivated = false; return enc; })
								.ToArray();
							videoStream.RemoteEncoding = connectionInfo.VideoStream.SendEncodings.FirstOrDefault();
						}
					}

					liveDownStream = liveChannel.CreateSfuDownstreamConnection(connectionInfo, audioStream, videoStream);
					var liveDownStreamClosure = liveDownStream;
					liveDownStream.DisableAutomaticIceServers = false;
					liveDownStream.IceGatherPolicy = IceGatherPolicy.All;
					liveDownStream.TrickleIcePolicy = TrickleIcePolicy.NotSupported;


					liveDownStream.Open()
						.Then((blarg) =>
						{
							if (liveDownStreamClosure == liveDownStream)
							{
								HookDownStream(liveDownStream);
								rtcCallback.DebugMessage("log", "DownStream Connection opened.");
							}
						})
						.Fail((err) =>
						{
							liveDownStream?.Close();
							liveDownStream = null;
							RemoteMedia?.Destroy();
							RemoteMedia = null;

							((Setting<Texture>)VideoTexture).Value = null;

							if (reconnectAttemptsRemaining > 0)
							{
								rtcCallback.DebugMessage("log", $"Failed to open SFU Downstream, attempting to reconnect. Error: {err}");
								scheduler.InsertTask(2 + (4 - reconnectAttemptsRemaining)*(4 - reconnectAttemptsRemaining), () => InitializeDownStream(reconnectAttemptsRemaining - 1));
							}
							else
								rtcCallback.Failure("RemoteDownstreamOpen", err.ToString());
						});
				}
				catch (Exception e)
				{
					liveDownStream?.Close();
					liveDownStream = null;
					RemoteMedia?.Destroy();
					RemoteMedia = null;
					((Setting<Texture>)VideoTexture).Value = null;
					if (reconnectAttemptsRemaining > 0)
					{
						rtcCallback.DebugMessage("log", $"Failed to CreateSfuDownstreamConnection, attempting to reconnect. Error: {e}");
						scheduler.InsertTask(2 + (4 - reconnectAttemptsRemaining)*(4 - reconnectAttemptsRemaining), () => InitializeDownStream(reconnectAttemptsRemaining - 1));
					}
					else
						rtcCallback.Failure("RemoteDownstreamOpen", e.ToString());
				}
			}
		}

		
		private void HookDownStream(SfuDownstreamConnection liveDownStream)
		{
			liveDownStream.OnStateChange += LiveDownStreamOnOnStateChange;
			LiveDownStreamOnOnStateChange(liveDownStream);
		}

		private void UnhookDownStream(SfuDownstreamConnection liveDownStream)
		{
			liveDownStream.OnStateChange -= LiveDownStreamOnOnStateChange;
		}

		private void LiveDownStreamOnOnStateChange(ManagedConnection managedConnection)
		{
			if (!ReferenceEquals(managedConnection, liveDownStream))
				return;

			DebugOutput.Out("log", $"{this.ConnectionId} DownStream Connection State: {managedConnection.State}");
			((ISetting<RtcConnectionStatus>)ConnectionStatus).Value = managedConnection.State.ToStatus();

			switch (managedConnection.State)
			{
				case ConnectionState.Failed:
					// Reconnect!
					if (liveChannel != null)
					{
						restartDownstreamTask = scheduler.InsertTask(2.0, () =>
						{
							lock (this)
							{
								if (liveDownStream != managedConnection)
									return;
								if (liveDownStream != null)
								{
									UnhookDownStream(liveDownStream);
									liveDownStream.Close();
									liveDownStream = null;
								}
								if (RemoteMedia != null)
								{
									RemoteMedia.Destroy();
									RemoteMedia = null;
								}
							}
							if (liveChannel != null)
							{
								InitializeDownStream();
							}
						});
					}
					break;
			}
		}

		private void RemoteMediaOnOnAudioLevel(double p)
		{
			((ISetting<float>) CurrentReceivingVolume).Value = (float)p;
		}

		public void SetReceiveQuality(int maxQualityLevel)
		{
			lock (this)
			{
				if (this.liveDownStream == null || this.connectionInfo == null)
					return; // shut down already.

				var encodings = connectionInfo?.VideoStream?.SendEncodings;
				if (encodings != null && encodings.Length > 1)
				{
					int selectedInd = System.Math.Min(maxQualityLevel, encodings.Length - 1);
					DebugOutput.Out("info",
						$"Changing connection {this.ConnectionId} to encoding {maxQualityLevel}: {encodings[selectedInd]}");

					var conf = this.liveDownStream.Config;
					if (conf != null)
					{
						conf.RemoteVideoEncoding = encodings[selectedInd];
						this.liveDownStream.Update(conf);
					}
				}
			}
		}

		public void ShutdownDownstreamAndMedia()
		{
			lock (this)
			{
				if (liveDownStream != null)
				{
					UnhookDownStream(liveDownStream);

					liveDownStream.Close();
					liveDownStream = null;
				}

				if (RemoteMedia != null)
				{
					var rm = RemoteMedia;
					RemoteMedia = null;

					((Setting<Texture>)VideoTexture).Value = null;

					rm.Destroy();
				}

				((ISetting<RtcConnectionStatus>)ConnectionStatus).Value = RtcCommon.RtcConnectionStatus.Disabled;
			}
		}
		public void Shutdown()
		{
			lock (this)
			{
				ShutdownDownstreamAndMedia();

				liveChannel = null;
			}
		}

		public void ChangeGain(float vol)
		{
			((ISetting<float>)CurrentGain).Value = Mathf.Clamp(vol, 0f, 10f);
			if (RemoteMedia != null)
				RemoteMedia.AudioGain = CurrentGain.Value;
		}

		public Future<object> ChangeOutputDevice(SinkOutput sink)
		{
			lastAssignedSink = sink;
			if (RemoteMedia != null)
				return RemoteMedia.ChangeAudioSinkOutput(sink);
			else
			{
				var p = new Promise<object>();
				p.Resolve(null);
				return p;
			}
		}

		private DateTime lastPollTime = DateTimeCache.UtcNow;
		public void PollStatus()
		{
			bool locked1 = false;
			try
			{
				if (Monitor.TryEnter(this))
				{
					locked1 = true;
					if (liveDownStream == null)
						return;

					if ((DateTimeCache.UtcNow - lastPollTime).TotalSeconds < 1f)
						return;

					lastPollTime = DateTimeCache.UtcNow;

					liveDownStream.GetStats().Then((ConnectionStats cs) =>
					{
						bool locked2 = false;
						try
						{
							if (Monitor.TryEnter(this))
							{
								locked2 = true;
								lastPollTime = DateTimeCache.UtcNow;

								if (cs.MediaStreams[0].Receiver == null)
									return;

								((ISetting<ConnectionStats>)this.ConnectionStats).Value = cs;
								((ISetting<long>)this.TotalBytesReceived).Value = cs.AudioStream?.Receiver?.BytesReceived ?? 0 + cs.VideoStream?.Receiver?.BytesReceived ?? 0;

								RtcConnectionMethod newMethod = RtcConnectionMethod.Unknown;
								if (cs.IsHost)
									newMethod = RtcConnectionMethod.Local;
								else if (cs.IsReflexive)
									newMethod = RtcConnectionMethod.Routable;
								else if (cs.IsRelayed)
									newMethod = RtcConnectionMethod.Relayed;
								((ISetting<RtcConnectionMethod>) this.ConnectionMethod).Value = newMethod;
							}
						}
						finally
						{
							if (locked2)
								Monitor.Exit(this);
						}
					});
				}
			}
			finally
			{
				if (locked1)
					Monitor.Exit(this);
			}
		}
	}
}
