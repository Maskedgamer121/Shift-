using System;
//Bringing over Core for convenient scheduler/wildcard transient access.  Think of it like bringing in any external tool.
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using Cavrnus.EngineConnector;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	public class CavrnusPostSynchronizedProperty<T> : IDisposable
	{
		public CavrnusLivePropertyUpdate<T> transientUpdater = null;
		private T lastPostedTransientValue;

		private CavrnusSpaceConnection spaceConn;

		private string containerName;
		private string propertyName;
		private Func<T> getVal;
		private IDisposable disp;
		const float pollingTime = .02f;

		public CavrnusPostSynchronizedProperty(string containerName, string propertyName, Func<T> getVal, string tag = "")
		{
			this.containerName = containerName;
			this.propertyName = propertyName;
			this.getVal = getVal;

			CavrnusFunctionLibrary.AwaitSpaceConnectionByTag(tag, sc => spaceConn = sc);

			disp = CavrnusStatics.Scheduler.ExecInMainThreadRepeating(pollingTime, TryUpdate);
		}

		//Only really for CoPresence
		public void ForceBeginTransientUpdate()
		{
			if (spaceConn == null) {
				Debug.LogError("SpaceConn is null!!!");
				return;
			}
			transientUpdater = CavrnusPropertyHelpers.BeginContinuousPropertyUpdate<T>(spaceConn,
					containerName, propertyName,
					getVal());
		}

		const float endChangeTimeGap = .3f;
		float lastChangeTime;

		private void TryUpdate()
		{
			//Don't do anything until we've loaded the space
			if (spaceConn == null) return;

			T currPropertyValue;

			if(transientUpdater == null)
			{
				if (typeof(T) == typeof(Color))
				{
					currPropertyValue = (T)(object)spaceConn.GetColorPropertyValue(
						containerName, propertyName);
				}
				else if (typeof(T) == typeof(string))
				{
					currPropertyValue = (T)(object)spaceConn.GetStringPropertyValue(
						containerName, propertyName);
				}
				else if (typeof(T) == typeof(bool))
				{
					currPropertyValue = (T)(object)spaceConn.GetBoolPropertyValue(
						containerName, propertyName);
				}
				else if (typeof(T) == typeof(Vector4))
				{
					currPropertyValue = (T)(object)spaceConn.GetVectorPropertyValue(
						containerName, propertyName);
				}
				else if (typeof(T) == typeof(float))
				{
					currPropertyValue = (T)(object)spaceConn.GetFloatPropertyValue(
						containerName, propertyName);
				}
				else if (typeof(T) == typeof(CavrnusTransformData))
				{
					currPropertyValue = (T)(object)spaceConn.GetTransformPropertyValue(
						containerName, propertyName);
				}
				else
				{
					throw new Exception($"Property value of type {typeof(T)} is not supported by CavrnusPostSynchronizedProperty yet!");
				}
			}
			else
			{
				currPropertyValue = lastPostedTransientValue;
			}

			T currUnityVal = getVal();

			bool hasChange = !Equals(currPropertyValue, currUnityVal);

			if (typeof(T) == typeof(Color))
			{
				Color serverVal = (Color)(object)currPropertyValue;
				Color unityVal = (Color)(object)currUnityVal;
				hasChange = !serverVal.r.AlmostEquals(unityVal.r, .0001f) || !serverVal.g.AlmostEquals(unityVal.g, .0001f) || !serverVal.b.AlmostEquals(unityVal.b, .0001f);
			}
			else if (typeof(T) == typeof(string))
			{
				string serverVal = currPropertyValue as string;
				string unityVal = currUnityVal as string;
				hasChange = !Equals(unityVal, serverVal);
			}
			else if (typeof(T) == typeof(bool))
			{
				bool serverVal = (bool)(object)currPropertyValue;
				bool unityVal = (bool)(object)currUnityVal;
				hasChange = serverVal != unityVal;
			}
			else if (typeof(T) == typeof(Vector4))
			{
				Vector4 serverVal = (Vector4)(object)currPropertyValue;
				Vector4 unityVal = (Vector4)(object)currUnityVal;
				hasChange = !serverVal.x.AlmostEquals(unityVal.x, .0001f) || !serverVal.y.AlmostEquals(unityVal.y, .0001f) || !serverVal.z.AlmostEquals(unityVal.z, .0001f) || !serverVal.w.AlmostEquals(unityVal.w, .0001f);
			}
			else if (typeof(T) == typeof(float))
			{
				float serverVal = (float)(object)currPropertyValue;
				float unityVal = (float)(object)currUnityVal;
				hasChange = !serverVal.AlmostEquals(unityVal, .0001f);
			}
			else if (typeof(T) == typeof(CavrnusTransformData))
			{
				var serverVal = currPropertyValue as CavrnusTransformData;
				var unityVal = currUnityVal as CavrnusTransformData;

				if (serverVal == null || unityVal == null) {
					hasChange = true;
				}
				else {
					hasChange = !serverVal.Position.AlmostEquals(unityVal.Position, .001f) || !EulersAreEqual(serverVal.EulerAngles, unityVal.EulerAngles, .1f) || !serverVal.Scale.AlmostEquals(unityVal.Scale, .001f);
				}
			}

			if (!hasChange) 
			{
				bool isUserProperty = containerName.StartsWith("/users/");
                //This change has timed out, time to finalize it
                if (!isUserProperty && transientUpdater != null && Time.time - lastChangeTime > endChangeTimeGap) 
				{
					lastPostedTransientValue = currUnityVal;

					// Debug.Log("Finish transient! + " + currUnityVal);

					transientUpdater.UpdateWithNewData(lastPostedTransientValue);
					transientUpdater.Finish();
					transientUpdater = null;
				}

				//Our transform is synchronized w/ the server, so don't do any posting
				return;
			}

			lastChangeTime = Time.time;

			if (transientUpdater == null)
			{
				// Debug.Log("Begin transient! + " + currUnityVal);

				lastPostedTransientValue = currUnityVal;
				transientUpdater = CavrnusPropertyHelpers.BeginContinuousPropertyUpdate<T>(spaceConn,
					containerName, propertyName,
					lastPostedTransientValue);
			}
			else
			{
				// Debug.Log("Update transient! + " + currUnityVal);

				lastPostedTransientValue = currUnityVal;
				transientUpdater.UpdateWithNewData(lastPostedTransientValue); 
			}
		}

		private bool EulersAreEqual(Vector3 a, Vector3 b, float lambda)
		{
			var angle = Quaternion.Angle(Quaternion.Euler(a), Quaternion.Euler(b));

			return angle < lambda;
		}

		public void Dispose() { disp?.Dispose(); }
	}
}