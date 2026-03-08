using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.Base.Core;
using Cavrnus.Base.Graphics;
using Cavrnus.Base.Settings;
using Cavrnus.RtcCommon;
using FM.LiveSwitch;
using Cavrnus.EngineConnector;
using DebugLogProvider = FM.LiveSwitch.Unity.DebugLogProvider;

namespace Cavrnus.RTC
{
	public class RtcSystemUnity : IRtcSystem
	{
		private readonly IUnityScheduler sched;

		private RtcSystemUnityLocal mediaContext;
		private Client liveSwitchClient;
		private Channel liveChannel;
		private SfuUpstreamConnection liveUpStream;

		private IRtcSystemCallback rtcCallback;

		private RtcModeEnum systemSendMode;
		private RtcModeEnum systemRecvMode;
		private RtcModeEnum userSendFilter;
		private RtcModeEnum userRecvFilter;
		private RtcModeEnum liveSendMode;
		private RtcModeEnum liveRecvMode;

		private bool initialized = false;

		private bool disableAec = false;

		private RtcOutputSink currentOutputSink = null;

		public string LocalConnectionId { get; private set; }
		private List<RtcSystemUnityRemoteConnection> remotes = new List<RtcSystemUnityRemoteConnection>();

		public IReadonlySetting<ConnectionStats> ConnectionStats { get; } = new Setting<ConnectionStats>(null);
		public List<RtcSystemUnityRemoteConnection> Remotes => remotes;

		private Setting<object> localVideoTex = new Setting<object>(null);


		public RtcSystemUnity(IUnityScheduler sched, bool disableAec = false)
		{
			this.sched = sched;
			this.disableAec = disableAec;
		}

		private static bool fmInitialized = false;

		public void Initialize(IRtcSystemCallback callback, RtcModeEnum sendMode, RtcModeEnum recvMode, RtcInputSource initialAudioInput,
			RtcOutputSink initialAudioOutput, RtcInputSource initialVideoInput, string fmLicenseKey)
		{
			if (!fmInitialized)
			{
				fmInitialized = true;

				try
				{
					FM.LiveSwitch.License.SetKey(fmLicenseKey);
				}
				catch (Exception e)
				{
					DebugOutput.Error(e.Message + "\n" + e.StackTrace);
				}

				FM.LiveSwitch.Log.DefaultLogLevel = SystemSettings.EnableVerboseLogging ? LogLevel.Debug : LogLevel.Warn;
				FM.LiveSwitch.Log.RegisterProvider(new DebugLogProvider(FM.LiveSwitch.Log.DefaultLogLevel));
			}

			this.rtcCallback = callback;


			this.initialized = true;
			this.systemSendMode = sendMode;
			this.systemRecvMode = recvMode;

			var useAec = !disableAec;
			
			currentOutputSink = initialAudioOutput;

			mediaContext = new RtcSystemUnityLocal();
			if (sendMode.IsAudioEnabled())
				mediaContext.SetCurrentAudioInput(initialAudioInput, () => { }, (e) => { });
			if (sendMode.IsVideoEnabled())
				mediaContext.SetCurrentVideoInput(initialVideoInput, () => { }, (e) => { });
			
			DebugOutput.Info($"initialize with {initialAudioInput?.Name}, {initialAudioOutput?.Name}, {initialVideoInput?.Name}");

			mediaContext.Initialize(sendMode, useAec);
			localVideoTex.Value = mediaContext.LocalMedia.View;
		}

		public void Shutdown()
		{
			if (!this.initialized)
				return;

			this.initialized = false;

			ShutdownRemotes();

			ShutdownUpstream();

			ShutdownChannel();

			ShutdownClient();

			ShutdownMedia();
		}

		private void ShutdownClient()
		{
			ShutdownChannel();

			try
			{
				if (liveSwitchClient != null)
				{
					var cc = liveSwitchClient;
					liveSwitchClient = null;
					cc.OnStateChange -= LiveSwitchClientOnOnStateChange;
					cc.Unregister().WaitForPromise();
					cc.CloseAll().WaitForPromise();
				}
			}
			catch (Exception e)
			{
				DebugOutput.Warning($"Error while shutting down liveswitch client: " + e);
			}
			liveSwitchClient = null;
		}

		private void ShutdownChannel()
		{
			ShutdownRemotes();

			ShutdownUpstream();

			try
			{
				if (liveChannel != null)
				{
					UnhookChannel();
					var lc = liveChannel;
					liveChannel = null;
					lc.CloseAll().WaitForPromise();
				}
			}
			catch (Exception e)
			{
				DebugOutput.Warning($"Error while shutting down liveswitch client: " + e);
			}
			liveChannel = null;
		}

		private void ShutdownUpstream()
		{
			try
			{
				if (liveUpStream != null)
				{
					rtcCallback.DebugMessage("log", "SFU Upstream Connection CLosing.");

					UnhookUpStream(liveUpStream);
					if (liveUpStream.State != ConnectionState.Closed && liveUpStream.State != ConnectionState.Closing)
						liveUpStream.Close().WaitForPromise();
				}
			}
			catch (Exception e)
			{
				DebugOutput.Warning($"Error when shutting down upstream: {e}");
			}
			liveUpStream = null;
		}

		private void ShutdownRemotes()
		{
			foreach (RtcSystemUnityRemoteConnection rtcSystemWindowsRemoteConnection in remotes)
			{
				try
				{
					rtcSystemWindowsRemoteConnection.Shutdown();
				}
				catch (Exception e)
				{
					DebugOutput.Error("Failure while shutting down remote connection: " + e);
				}
			}
			remotes.Clear();
		}

		private void ShutdownMedia()
		{
			try
			{
				mediaContext.Shutdown();
			}
			catch (Exception e)
			{
				DebugOutput.Error("Failure while shutting down media context: " + e);
			}
			localVideoTex.Value = null;
			mediaContext = null;
		}

		public void LocalUserEnter(string localConnectionId, string localUserId, string clientId, RtcAccessInfo rtcAccessInfo, RtcModeEnum sendPermitted, RtcModeEnum recvPermitted)
		{
			LocalConnectionId = localConnectionId;

			userSendFilter = sendPermitted;
			userRecvFilter = recvPermitted;
			liveSendMode = userSendFilter & systemSendMode;
			liveRecvMode = userRecvFilter & systemRecvMode;

			DebugOutput.Info($"Initializing LiveSwitch client at {rtcAccessInfo.liveSwitchEndpoint}, {rtcAccessInfo.liveSwitchApplicationName}, on {this.GetDeviceId()}, conn {localConnectionId}.");

			ShutdownClient();

			liveSwitchClient = new Client(rtcAccessInfo.liveSwitchEndpoint, rtcAccessInfo.liveSwitchApplicationName, localConnectionId, this.GetDeviceId(), clientId, null);
			liveSwitchClient.UserAlias = clientId;
			liveSwitchClient.ExternalId = clientId;
			liveSwitchClient.DisableWebSockets = false;
			DoOpenClient(rtcAccessInfo);
		}

		public void UpdatePermittedModes(RtcModeEnum sendPermitted, RtcModeEnum recvPermitted)
		{
			DebugOutput.Info($"Updating permitted RTC modes to be {sendPermitted},{recvPermitted}.");
			if (userSendFilter != sendPermitted)
			{
				if (liveSendMode != (sendPermitted & systemSendMode))
				{
					userSendFilter = sendPermitted;
					var oldMode = liveSendMode;
					liveSendMode = sendPermitted & systemSendMode;

					// Update upstreams
					DebugOutput.Info($"RTC Send Mode changing to {liveSendMode}.");
					if (liveSendMode != RtcModeEnum.None && oldMode == RtcModeEnum.None) // Activating up stream
					{
						if (liveChannel != null)
							InitializeUpStream(liveChannel);
					}
					else if (liveSendMode == RtcModeEnum.None && oldMode != RtcModeEnum.None) // Shutting down up stream
					{
						ShutdownUpstream();
					}
					else // The only remaining cases here are changing the audio and video options without completely activating or deactivating. 
					{
						// We need to reestablish streams in this case; we have to restart the upstream (or renegotiate it?)
						ShutdownUpstream();
						InitializeUpStream(liveChannel);
					}
				}
				else
					userSendFilter = sendPermitted;
			}
			if (userRecvFilter != recvPermitted)
			{
				if (liveRecvMode != (recvPermitted & systemRecvMode))
				{
					userRecvFilter = recvPermitted;
					liveRecvMode = recvPermitted & systemRecvMode;

					DebugOutput.Info($"RTC Recv Mode changing to {liveRecvMode}.");

					// Update remotes with mode.
					foreach (var r in Remotes)
					{
						r.UpdateMode(liveRecvMode);
					}
				}
				else
					userRecvFilter = recvPermitted;
			}
		}

		private void LiveSwitchClientOnOnStateChange(Client client)
		{
			switch (client.State)
			{
				case ClientState.Unregistered:
					{
						if (initialized)
						{
							rtcCallback.RequestRtcAuthorization();
						}
					}
					break;
			}
		}

		private void DoOpenClient(RtcAccessInfo rtcAccessInfo, int reconnectAttemptsLeft = 8)
		{
			if (liveSwitchClient == null) // cancelled
				return;

			DebugOutput.Log($"DoOpenClient ({reconnectAttemptsLeft} attempts left)");

			var liveSwitchClientClosure = liveSwitchClient;
			liveSwitchClientClosure.Register(rtcAccessInfo.channelAuthToken)
				.Then((channels) =>
				{
					if (liveSwitchClient == liveSwitchClientClosure)
					{
						if (channels == null || channels.Length == 0)
						{
							rtcCallback.Failure("LocalUserEnter", "Received no channels from liveswitch registration. Disconnecting.");

							ShutdownClient();
							return;
						}

						liveSwitchClient.OnStateChange += LiveSwitchClientOnOnStateChange;
						LiveSwitchClientOnOnStateChange(liveSwitchClient);
						if (liveSwitchClient.State != ClientState.Unregistered)
							InitializeChannel(channels[0]);
					}
					else
						rtcCallback.DebugMessage("log", "LiveSwitch Channel Registration returned on a no-longer relevant client. Ignoring.");
				}).Fail((e) =>
				{
					if (reconnectAttemptsLeft > 0)
					{
						rtcCallback.DebugMessage("warning", $"Failed To Connect to Live Switch Client, reconnecting, {reconnectAttemptsLeft} attempts left. Error was {e}");
						sched.BaseScheduler.InsertTask(2.0 + 8 - reconnectAttemptsLeft, () =>
						{
							if (!initialized)
								return;
							DoOpenClient(rtcAccessInfo, reconnectAttemptsLeft - 1);
						});
					}
					else
					{
						rtcCallback.Failure("LocalUserEnter", e.ToString());
						liveSwitchClient = null;
					}
				});
		}

		private void InitializeChannel(Channel channel)
		{
			this.liveChannel = channel;

			DebugOutput.Info($"Initializing LiveSwitch channel {channel.Id}.");

			InitializeUpStream(channel);

			HookChannel();
		}

		private void HookChannel()
		{
			liveChannel.OnRemoteUpstreamConnectionOpen += LiveChannelOnOnRemoteUpstreamConnectionOpen;
			liveChannel.OnRemoteUpstreamConnectionClose += LiveChannelOnOnRemoteUpstreamConnectionClose;

			foreach (var liveChannelRemoteUpstreamConnectionInfo in liveChannel.RemoteUpstreamConnectionInfos)
			{
				LiveChannelOnOnRemoteUpstreamConnectionOpen(liveChannelRemoteUpstreamConnectionInfo);
			}
		}

		private void UnhookChannel()
		{
			liveChannel.OnRemoteUpstreamConnectionOpen -= LiveChannelOnOnRemoteUpstreamConnectionOpen;
			liveChannel.OnRemoteUpstreamConnectionClose -= LiveChannelOnOnRemoteUpstreamConnectionClose;
		}

		private void InitializeUpStream(Channel channel, int reconnectAttempts = 4)
		{
			if (!initialized)
				return;
			if (liveSwitchClient == null || liveSwitchClient.State == ClientState.Unregistered || liveSwitchClient.State == ClientState.Unregistering)
				return;

			if (mediaContext.LocalMedia == null) // Do not open upstream!
				return;
			if (liveSendMode == RtcModeEnum.None) // Also Do Not open upstream!
				return;

			ShutdownUpstream(); // Close any already-existing upstream

			AudioStream audioStream = null;
			VideoStream videoStream = null;
			try
			{
				bool enableAudio = liveSendMode.IsAudioEnabled();

				audioStream = (enableAudio && mediaContext.LocalMedia.AudioTrack != null)
								? new AudioStream(mediaContext.LocalMedia.AudioTrack)
								{
									BandwidthAdaptationPolicy = BandwidthAdaptationPolicy.Enabled,
								}
								: null;

				bool enableVideo = liveSendMode.IsVideoEnabled();

				if (enableVideo)
				{
					videoStream = mediaContext.LocalMedia.VideoTrack != null
								? new VideoStream(mediaContext.LocalMedia.VideoTrack)
								{
									BandwidthAdaptationPolicy = BandwidthAdaptationPolicy.Enabled,
									SimulcastMode = SimulcastMode.RtpStreamId,
								}
								: null;
					liveUpStream = channel.CreateSfuUpstreamConnection(audioStream, videoStream);
				}
				else
					liveUpStream = channel.CreateSfuUpstreamConnection(audioStream);

				liveUpStream.IceGatherPolicy = IceGatherPolicy.All;
				liveUpStream.DisableAutomaticIceServers = false;
				liveUpStream.TrickleIcePolicy = TrickleIcePolicy.NotSupported;

				DebugOutput.Log($"InitializeUpStream ({reconnectAttempts})");

				var liveUpStreamClosure = liveUpStream;
				liveUpStream.Open()
					.Then((result) =>
					{
						if (liveUpStreamClosure == liveUpStream)
						{
							rtcCallback.DebugMessage("log", "SFU Upstream Connection Opened.");
							HookUpStream(liveUpStreamClosure);
						}
					})
					.Fail((err) =>
					{
						if (liveUpStreamClosure == liveUpStream)
						{
								// Ignore! A new upstream was attempted in parallel.
								if (reconnectAttempts > 0)
							{
								rtcCallback.DebugMessage("log", $"SFU Upstream Connection failed to open, reconnection attempt forthcoming. Error was: {err}");
								sched.BaseScheduler.InsertTask(2.0 + (4 - reconnectAttempts) * (4 - reconnectAttempts), () => { InitializeUpStream(channel, reconnectAttempts - 1); });
							}
							else
							{
								rtcCallback.Failure("HookUpStream", err.ToString());
							}
						}
					});
			}
			catch (Exception e)
			{
				if (reconnectAttempts > 0)
				{
					rtcCallback.DebugMessage("log", $"SFU Upstream Connection failed to initialize, reconnection attempt forthcoming. Error was: {e}");
					sched.BaseScheduler.InsertTask(2.0 + (4 - reconnectAttempts) * (4 - reconnectAttempts), () => { InitializeUpStream(channel, reconnectAttempts - 1); });
				}
				else
				{
					rtcCallback.DebugMessage("log", $"SFU Upstream Connection failed to initialize, reconnectAttempts exhausted. Error was: {e}");
					rtcCallback.Failure("OpenUpStream", e.ToString());
				}
			}
		}

		private void HookUpStream(SfuUpstreamConnection liveUpStream)
		{
			liveUpStream.OnStateChange += LiveUpStreamOnOnStateChange;
			LiveUpStreamOnOnStateChange(liveUpStream);
		}

		private void UnhookUpStream(SfuUpstreamConnection liveUpStream)
		{
			liveUpStream.OnStateChange -= LiveUpStreamOnOnStateChange;
		}

		private void LiveUpStreamOnOnStateChange(ManagedConnection managedConnection)
		{
			DebugOutput.Log($"UpStream Connection State: {managedConnection.State}");

			var shouldReconnect = initialized && liveChannel != null;

			switch (managedConnection.State)
			{
				case ConnectionState.Failed:
					if (shouldReconnect)
						ReconnectUpstream();                
					break;
			}
		}

		private void ReconnectUpstream()
		{
			sched.BaseScheduler.InsertTask(2.0, () =>
			{
				try
				{
					ShutdownUpstream();

					if (initialized && liveChannel != null)
					{
						InitializeUpStream(liveChannel);
					}
				}
				catch (Exception ex)
				{
					DebugOutput.Error($"Error reconnecting upstream: {ex}");
				}
			});
		}

		private void LiveChannelOnOnRemoteUpstreamConnectionOpen(ConnectionInfo p)
		{
			sched.ExecInMainThread(() =>
			{
				DebugOutput.Log($"Initializing LiveSwitch Remote Downstream with {p.ClientId}, {p.UserId}, {p.Id}.");

				var newRemote =
					new RtcSystemUnityRemoteConnection(p.UserId, sched.BaseScheduler, rtcCallback, mediaContext);
				remotes.Add(newRemote);
				newRemote.Initialize(liveChannel, liveRecvMode, p, currentOutputSink?.ToFM());

				rtcCallback.ReportRemoteUserEntry(p.UserId, newRemote.CurrentMode);
			});
		}

		private void LiveChannelOnOnRemoteUpstreamConnectionClose(ConnectionInfo p)
		{
			sched.ExecInMainThread(() =>
			{
				var found = remotes.FirstOrDefault(r => r.ConnectionId == p.UserId);
				if (found != null)
				{
					rtcCallback.ReportRemoteUserExit(found.ConnectionId);
					remotes.Remove(found);
					found.Shutdown();
				}
			});
		}

		public void LocalUserExit()
		{
			LocalConnectionId = null;

			ShutdownRemotes();

			ShutdownUpstream();

			ShutdownChannel();

			ShutdownClient();
		}

		public void SetSelfMuting(bool muteSelf)
		{
			if (mediaContext?.LocalMedia == null)
				return;

			mediaContext.LocalMedia.AudioMuted = muteSelf;
		}

		public void SetInputGain(float gain)
		{
			if (mediaContext?.LocalMedia == null)
				return;

			mediaContext.LocalMedia.AudioGain = gain;
		}

		public void ChangeAudioSourceEnabled(bool setEnabled, int requestId)
		{
		}

		public void SetOutputGain(string remoteConnectionId, float gain)
		{
			RtcSystemUnityRemoteConnection found = null;
			lock (remotes)
			{
				found = remotes.FirstOrDefault(r => r.ConnectionId == remoteConnectionId);
			}
			found?.ChangeGain(gain);
		}

		public void SetReceiveQuality(string remoteConnectionId, int maxQualityLevel)
		{
			RtcSystemUnityRemoteConnection found = null;
			lock (remotes)
			{
				found = remotes.FirstOrDefault(r => r.ConnectionId == remoteConnectionId);
			}
			found?.SetReceiveQuality(maxQualityLevel);
		}

		public void ChangeAudioInput(RtcInputSource input, int requestId)
		{
			if (!initialized)
				return;

			DebugOutput.Info($"set audio input to {input?.Name}");

			mediaContext.SetCurrentAudioInput(input, () =>
			{
				rtcCallback.ResponseChangeAudioInputDevice(true, requestId, mediaContext.CurrentAudioInput.Value, "");
			}, (err) =>
			{
				rtcCallback.ResponseChangeAudioInputDevice(false, requestId, mediaContext.CurrentAudioInput.Value, err);
			});
		}

		public void ChangeAudioOutput(RtcOutputSink output, int requestId)
		{
		}

		public void ChangeVideoInput(RtcInputSource input, int requestId)
		{
			mediaContext.SetCurrentVideoInput(input, () =>
			{
				rtcCallback.ResponseChangeVideoInputDevice(true, requestId, mediaContext.CurrentVideoInput.Value, "");
				localVideoTex.Value = mediaContext.LocalMedia.View;

				if (liveUpStream != null && !input.IsBlankSource() && liveUpStream.HasVideo)
				{
					DebugOutput.Info($"Reconnecting upstream for video");
					ReconnectUpstream();
				}
			}, (err) =>
			{
				rtcCallback.ResponseChangeVideoInputDevice(false, requestId, mediaContext.CurrentVideoInput.Value, err);
				localVideoTex.Value = null;
			});
		}

		public void RequestAudioInputs()
		{
			mediaContext.FetchInputOptions((sources) =>
			{
				rtcCallback.ResponseAvailableAudioInputs(sources);
			});
		}

		public void RequestAudioOutputs()
		{
			rtcCallback.ResponseAvailableAudioOutputs(new RtcOutputSink[0]);
		}

		public void RequestVideoInputs()
		{
			mediaContext.FetchVideoInputOptions((sources) =>
			{
				rtcCallback.ResponseAvailableVideoInputs(sources);
			});
		}

		public void RequestPollStatus()
		{
			LocalUserReportInfo l = new LocalUserReportInfo()
			{
				CurrentInput = mediaContext?.CurrentAudioInput?.Value,
				CurrentOutput = currentOutputSink,
				CurrentVideoInput = mediaContext?.CurrentVideoInput?.Value,
				Volume = mediaContext?.CurrentTransmittingVolume?.Value ?? 0f,
			};
			rtcCallback.ReportLocalStatus(l);

			this.liveUpStream?.GetStats()?.Then((ConnectionStats cs) =>
			{
				((Setting<ConnectionStats>)ConnectionStats).Value = cs;
			});

			remotes.ToList().ForEach((u) =>
			{
				if (u == null)
					return;
				var rs = new RemoteUserReportInfo()
				{
					Status = u.ConnectionStatus.Value,
					ConnectionId = u.ConnectionId,
					Volume = u.CurrentReceivingVolume.Value,
					Method = u.ConnectionMethod.Value,
					SentBytes = 0,
					ReceivedBytes = u.TotalBytesReceived.Value,
					LiveTimeSeconds = u.TotalSecondsLive.Value
				};
				rtcCallback.ReportRemoteStatus(rs);

				u.PollStatus();
			});
		}

		public IReadonlySetting<object> GetDirectVideo(string connectionId)
		{
			if (String.IsNullOrWhiteSpace(connectionId) || connectionId == LocalConnectionId)
			{
				return localVideoTex;
			}

			var remote = remotes.FirstOrDefault(r => r.ConnectionId == connectionId);
			if (remote == null)
				return null;

			return remote.VideoTexture;
		}

		public void NotifyVideoStreamImageProcessed(string connectionId, int frame)
		{
		}

		public void ProvideImageToImageVideoSource(IImage2D image)
		{
			(mediaContext?.LocalMedia as LocalCompositeMedia)?.ProvideImageToImageVideoSource(image);
		}
	
		public bool DirectVideoSupport => true;

		private int reqId = 0;
		public int NextUniqueRequestId() => ++reqId;

		public string GetDeviceId()
		{
			// Temporary; should swap based on the BUILD_*
			return "Unity";
		}
	}
}