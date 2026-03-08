using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	public class CavrnusDisplayProperty<T> : IDisposable
	{
		private List<IDisposable> disposables = new List<IDisposable>();

		private ICavrnusValueSync<T> sync;

		private Func<bool> claimedByLocalTransient;

		public CavrnusDisplayProperty(ICavrnusValueSync<T> sync, Func<bool> claimedByLocalTransient, string tag = "")
		{
			this.sync = sync;
			this.claimedByLocalTransient = claimedByLocalTransient;

			CavrnusFunctionLibrary.AwaitSpaceConnectionByTag(tag, OnSpaceConnection);
		}

		private void OnSpaceConnection(CavrnusSpaceConnection spaceConn)
		{
			if (typeof(T) == typeof(Color)) 
			{
                spaceConn.DefineColorPropertyDefaultValue(sync.Context.UniqueContainerName, sync.PropName, (Color)(object)sync.GetValue());

                var bndDisp = spaceConn.BindColorPropertyValue(sync.Context.UniqueContainerName, sync.PropName, (data) => 
				{
					if(!claimedByLocalTransient())
						sync.SetValue((T)(object)data);
				});
                disposables.Add(bndDisp);
            }
			else if (typeof(T) == typeof(string)) 
			{
                spaceConn.DefineStringPropertyDefaultValue(sync.Context.UniqueContainerName, sync.PropName, (string)(object)sync.GetValue());

                var bndDisp = spaceConn.BindStringPropertyValue(sync.Context.UniqueContainerName, sync.PropName, (data) =>
				{
					if (!claimedByLocalTransient())
						sync.SetValue((T)(object)data);
				});
                disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(bool)) 
			{
                spaceConn.DefineBoolPropertyDefaultValue(sync.Context.UniqueContainerName, sync.PropName, (bool)(object)sync.GetValue());

                var bndDisp = spaceConn.BindBoolPropertyValue(sync.Context.UniqueContainerName, sync.PropName, (data) =>
				{
					if (!claimedByLocalTransient())
						sync.SetValue((T)(object)data);
				});
                disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(Vector4)) 
			{
                spaceConn.DefineVectorPropertyDefaultValue(sync.Context.UniqueContainerName, sync.PropName, (Vector4)(object)sync.GetValue());

                var bndDisp = spaceConn.BindVectorPropertyValue(sync.Context.UniqueContainerName, sync.PropName, (data) =>
				{
					sync.SetValue((T)(object)data);
				});
                disposables.Add(bndDisp);
            }
			else if (typeof(T) == typeof(float)) 
			{
                spaceConn.DefineFloatPropertyDefaultValue(sync.Context.UniqueContainerName, sync.PropName, (float)(object)sync.GetValue());

                var bndDisp = spaceConn.BindFloatPropertyValue(sync.Context.UniqueContainerName, sync.PropName, (data) => 
				{ 
					if (!claimedByLocalTransient()) 
						sync.SetValue((T)(object)data);
				});
                disposables.Add(bndDisp);
			}
			else if (typeof(T) == typeof(CavrnusTransformData)) 
			{
                spaceConn.DefineTransformPropertyDefaultValue(sync.Context.UniqueContainerName, sync.PropName, (CavrnusTransformData)(object)sync.GetValue());

                var bndDisp = spaceConn.BindTransformPropertyValue(sync.Context.UniqueContainerName, sync.PropName,	(data) => 
				{
					if (!claimedByLocalTransient()) 
						sync.SetValue((T)(object)data);
				});
				disposables.Add(bndDisp);
			}
			else {
				throw new Exception(
					$"Property value of type {typeof(T)} is not supported by CavrnusDisplayProperty yet!");
			}
		}

		public void Dispose()
		{
			//Dispose of everything
			foreach (var disp in disposables) disp.Dispose();
			disposables.Clear();
		}
	}
}