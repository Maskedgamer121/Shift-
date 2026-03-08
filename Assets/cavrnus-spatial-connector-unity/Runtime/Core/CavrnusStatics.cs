using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Cavrnus.Base.Collections;
using Cavrnus.Base.Core;
using Cavrnus.Base.Net;
using Cavrnus.Base.Settings;
using Cavrnus.Comm;
using Cavrnus.Comm.Comm;
using Cavrnus.Comm.Comm.NotifyApi;
using Cavrnus.Comm.Content;
using Cavrnus.EngineConnector;
using Cavrnus.RTC;
using Cavrnus.RtcCommon;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.PlatformPermissions;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using static Cavrnus.SpatialConnector.Setup.CavrnusSpatialConnector;
using Debug = UnityEngine.Debug;

namespace Cavrnus.SpatialConnector.Core
{
	public static class CavrnusStatics
	{
		public static UnityScheduler Scheduler
		{
			get {
				if (scheduler == null) {
					scheduler = new GameObject("Scheduler").AddComponent<UnityScheduler>();
					scheduler.Setup();
				}

				return scheduler;
			}
		}

		private static UnityScheduler scheduler = null;
		internal static NotifyCommunication Notify { get; private set; }
		internal static LivePolicyEvaluator LivePolicyEvaluator { get; private set; }
		internal static ServerContentCacheManager ContentManager { get; private set; }

		internal static INetworkRequestImplementation NetworkRequestImpl;

		internal static ISetting<RtcInputSource> DesiredVideoStream = new Setting<RtcInputSource>(null);

		internal static CavrnusAuthentication CurrentAuthentication = null;
		private static IRtcSystem rtcSystem;

		internal static CavrnusSettings CavrnusSettings;

		internal static void Setup(CavrnusSettings settings)
		{
			if (CavrnusSettings != null)
			{
				// We're already running. Throw a Debug.LogError but let things continue.
				Debug.LogError($"Cavrnus Setup cannot be called twice. Make sure you don't have two.");
				//throw new InvalidOperationException($"Cavrnus Setup cannot be called twice.");
				return;
			}

			CavrnusSettings = settings;
			
			HelperFunctions.MainThread = Thread.CurrentThread;
			if (!settings.DisableCavrnusLogs)
				DebugOutput.MessageEvent += DoRecvMessage;
			CollabPaths.FlushTemporaryFilePath();

			HandlePlatformsSetup();

			NetworkRequestImpl = new FrameworkNetworkRequestImplementation();

			ContentManager = new ServerContentCacheManager(new FrameworkNetworkRequestImplementation());

			Notify = new NotifyCommunication(() => new NotifyWebsocket(Scheduler.BaseScheduler), Scheduler.BaseScheduler);
			LivePolicyEvaluator = new LivePolicyEvaluator(Notify.PoliciesSystem.AllPolicies, Notify.PoliciesSystem.IsActive);
	
			if (settings.DisableVoice && settings.DisableVideo) 
				rtcSystem = new RtcSystemUnavailable(); 
			else
				rtcSystem = new RtcSystemUnity(Scheduler, settings.DisableAcousticEchoCancellation);

			Scheduler.ExecOnApplicationQuit(Shutdown);
		}

		internal static RtcContext CreateRtcContext(CavrnusSpaceConnectionConfig config)
		{
			var input = RtcInputSource.FromJson("");
			var output = RtcOutputSink.FromJson("");
			var vidInput = RtcInputSource.FromJson("");
			
			RtcModeEnum sendMode;
			RtcModeEnum recvMode;

			// If SpatialConnector A/V settings are set then those have priority
			if (CavrnusSettings.DisableVideo && CavrnusSettings.DisableVoice)
			{
				sendMode = RtcModeEnum.None;
				recvMode = RtcModeEnum.None;
				
			}
			else if (CavrnusSettings.DisableVideo)
			{
				sendMode = RtcModeEnum.AudioOnly;
				recvMode = RtcModeEnum.AudioOnly;
			}
			else if (CavrnusSettings.DisableVoice)
			{
				sendMode = RtcModeEnum.Video;
				recvMode = RtcModeEnum.Video;
			}
			else
			{
				sendMode = config.IncludeRtc ? RtcModeEnum.AudioVideo : RtcModeEnum.None;
				recvMode = config.IncludeRtc ? RtcModeEnum.AudioVideo : RtcModeEnum.None;
			}
			
			var ctx = new RtcContext(rtcSystem, Scheduler.BaseScheduler);
			ctx.Initialize(input, output, vidInput, sendMode, recvMode);

			return ctx;
		}

		internal static void Shutdown()
		{
			if (CavrnusSettings == null) // already dead
				return;

			CurrentAuthentication = null;
			Notify.Shutdown();
			rtcSystem.Shutdown();
			CavrnusSpaceConnectionManager.Shutdown();
			CavrnusSettings = null;
		}

		private static void DoRecvMessage(string category, string message)
		{
			if (category == "log") return; // Ignore log messages.

			string callstack = "";
			callstack = "\r\n@ " + new StackTrace(1, true).ToString();

			if (category == "print"
			    || category == "log"
			    || category == "info") { Debug.Log(message); }
			else if (category == "debug") { Debug.Log($"{message}\n{callstack}"); }
			else if (category == "warning") {
				string output = $"{message}";
				Debug.LogWarning($"{output}\n{callstack}");
			}
			else if (category == "error" || category == "userError") {
				string output = $"{message}";
				Debug.LogError($"{output}\n{callstack}");
			}
		}

		private static void HandlePlatformsSetup()
		{
			PlatformPermissionsRequestHelper.RequestPermissions(CavrnusSettings.DisableVoice, CavrnusSettings.DisableVideo);
			
			var integrationInfo = new ClientProvidedIntegrationInfo
			{
				ApplicationId = Application.productName,
				ApplicationVersion = Application.version,
				EngineId = "Unity",
				EngineVersion = Application.unityVersion,
				DeviceId = Application.platform.ToString(),
				DeviceMode = "desktop"
			};
			Comm.IntegrationInfo.AssignIntegrationInfo(integrationInfo);
			var pathValidAppName = Application.productName;
			Path.GetInvalidPathChars().ForEach((c) => pathValidAppName = pathValidAppName.Replace(c, '_'));
			Comm.IntegrationInfo.ApplicationStorageFolderName = pathValidAppName;

			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				CollabPaths.SetIOsUnityPaths();
				RtcContext.DeferImagesOnVideoInputChanges = true;
				NetworkListener.SupportsThreadInterrupts = false;
			}
#if UNITY_2022
			if (Application.platform == RuntimePlatform.VisionOS /*TODO: Does this use the same settings as iOS?*/)
			{
				CollabPaths.SetIOsUnityPaths();
				RtcContext.DeferImagesOnVideoInputChanges = true;
				NetworkListener.SupportsThreadInterrupts = false;
			}
#endif
			else if (Application.platform == RuntimePlatform.OSXPlayer ||
			         Application.platform == RuntimePlatform.OSXEditor ||
			         Application.platform == RuntimePlatform.OSXServer)
			{
				CollabPaths.SetMacosPaths();
				RtcContext.DeferImagesOnVideoInputChanges = true;
				NetworkListener.SupportsThreadInterrupts = false;
			}
			else if(Application.platform == RuntimePlatform.Android)
			{
				CollabPaths.SetPathsFromCommonRoot(Application.persistentDataPath);
				NetworkListener.SupportsThreadInterrupts = false;
			}
			else if(Application.platform == RuntimePlatform.WebGLPlayer)
			{
				CollabPaths.SetPathsFromCommonRoot(Application.persistentDataPath);
				NetworkListener.SupportsThreadInterrupts = false;
			}
			else if (Application.platform == RuntimePlatform.WindowsPlayer ||
			         Application.platform == RuntimePlatform.WindowsEditor ||
			         Application.platform == RuntimePlatform.WindowsServer)
			{
				CollabPaths.SetWindowsPaths();
			}
		}
	}
}