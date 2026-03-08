using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Cavrnus.Base.Core;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LiveTypes;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.Comm.Prop;
using Cavrnus.Comm.Prop.JournalInterop;
using Cavrnus.Comm.Prop.TransformProp;
using Cavrnus.EngineConnector;
using Cavrnus.LiveRoomSystem;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using StringPropertyMetadata = Cavrnus.Comm.Prop.StringProp.StringPropertyMetadata;

namespace Cavrnus.SpatialConnector.Core
{
	internal static class CavrnusPropertyHelpers
	{
		private class MiniFakeDisposable : IDisposable
		{
			public static MiniFakeDisposable Instance = new MiniFakeDisposable();
			public void Dispose(){}
		}
		
		public static async void FetchLocalUserMetadata(string key, Action<string> onMetadataValue, Action<string> onFailure)
		{
			try
			{
				var lu = await CavrnusStatics.CurrentAuthentication.RestUserComm.GetUserProfileAsync();
				if (lu.userMetadata.TryGetValue(key, out var val)) {
					onMetadataValue?.Invoke(lu.userMetadata[val]);
				}
				else {
					onFailure?.Invoke($"The metadata key: {key} is not found!");
				}
			}
			catch (ErrorInfo e)
			{
				onFailure?.Invoke($"Error fetching metadata: {e.message}");
			}
		}

		public static async void DeleteLocalUserMetadataByKey(string key, Action<string> onSuccess = null, Action<string> onFailure = null)
		{
			try {
				await CavrnusStatics.CurrentAuthentication.RestUserComm.DeleteSelfUserMetadataAsync(
					new RestUserCommunication.DeleteUserMetadataRequest {keys = new[] {key}});
				
				onSuccess?.Invoke($"Successfully deleted metadata by key: {key}");
			}
			catch (ErrorInfo e) {
				onFailure?.Invoke($"Failed to delete metadata by key: {e.message}");
			}
		}

		public static async void UpdateLocalUserMetadata(string key, string value, Action<string> onSuccess = null, Action<string> onFailure = null)
		{
			try {
				await CavrnusStatics.CurrentAuthentication.RestUserComm.SetSelfUserMetadataAsync(
					new RestUserCommunication.UpdateUserMetadataRequest {
						userMetadata =
							new Dictionary<string, string> {{key, String.IsNullOrWhiteSpace(value) ? " " : value}}
					});
				
				onSuccess?.Invoke($"User metadata successfully updated with key: {key} and value: {value}");
			}
			catch (ErrorInfo e) {
				onFailure?.Invoke($"User metadata failed to update: {e.message}");
			}
		}

		public static void ResetLiveHierarchyRootName(GameObject root, string newRootName)
		{
			if(root.GetComponent<CavrnusPropertiesContainer>() == null)
				root.AddComponent<CavrnusPropertiesContainer>();
			string initialName = root.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName;

			foreach(var propsContainer in root.GetComponentsInChildren<CavrnusPropertiesContainer>())
			{
				if(propsContainer.UniqueContainerName == initialName)
				{
					propsContainer.UniqueContainerName = newRootName;

                }
				else if (propsContainer.UniqueContainerName.StartsWith(initialName + "/"))
				{
					propsContainer.UniqueContainerName = newRootName + "/" + propsContainer.UniqueContainerName.Substring((initialName + "/").Length);
				}
			}
		}

		private static void ResolveContainerPath(ref string propertyId, ref string containerId)
		{
			//Ignore the path completely and resolve from root
			if (propertyId.StartsWith("/")) {
				var split = propertyId.Split('/', StringSplitOptions.RemoveEmptyEntries);
				propertyId = split.Last();
				containerId = String.Join("/", split.Take(split.Length - 1));
			}
			//Add my path to the overall path
			else if (propertyId.Contains("/")) {
				var split = propertyId.Split('/', StringSplitOptions.RemoveEmptyEntries);
				propertyId = split.Last();
				containerId = String.Join("/", split.Take(split.Length - 1)) + "/" + containerId;
            }
		}

		private static void CheckCommonErrors(CavrnusSpaceConnection spaceConn, string containerId, string propertyId)
		{
			if (spaceConn == null)
				throw new ArgumentException("RoomConnection is null!  Has the space finished loading yet?");
		}

		private static PropertySetManager MyContainer(CavrnusSpaceConnection spaceConn, string containerId)
		{
			var myContainerId = new PropertyId(containerId);
			return spaceConn.CurrentSpaceConnection.Value.RoomSystem.PropertiesRoot.SearchForContainer(myContainerId);
		}

		internal static CavrnusLivePropertyUpdate<T> BeginContinuousPropertyUpdate<T>(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, T val, PropertyPostOptions options = null)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			options ??= new PropertyPostOptions();

			return new CavrnusLivePropertyUpdate<T>(spaceConn, containerId, propertyId, val, options.smoothed);
		}

		private static string PostLocalTransient(RoomSystem space, Operation op)
		{
			string uniqueTransientId = Guid.NewGuid().ToString();
			var entry = new TransientEntry()
			{
				ConnectionId = space.Comm.LocalCommUser.Value.ConnectionId ?? "",
				Time = Timestamp.FromDateTime(space.LiveJournal.EstimateCurrServerTimestamp()),
				Ev = new TransientEvent()
				{
					TransientJournalUpdate = new EvTransientJournalUpdate()
					{
						V1 = new EvTransientJournalUpdate.Types.V1()
						{
							UniqueId = uniqueTransientId,
							UpdatePropertyValue = op.UpdatePropertyValue,
						}
					}
				},
			};
			space.LiveJournal.RecvTransient(entry);

			return uniqueTransientId;
		}

		#region Color Props

		internal static bool ColorPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                      string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForColorProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineColorPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, Color defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			if (myContainer.GetColorProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
			{
				Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
				return MiniFakeDisposable.Instance;
			}

			return myContainer.DefineColorProperty(propertyId, defaultVal.ToColor4F(),
			                                       new Cavrnus.Comm.Prop.ColorProp.ColorPropertyMetadata() {
				                                       Name = propertyId
												   });
		}

		internal static Color GetColorPropertyValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetColorProperty(propertyId).Current.Value.Value.ToColor();
		}

		internal static IDisposable BindToColorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, Action<Color> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				internalBind = GetColorPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}
		
		internal static IDisposable GetColorPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<Color> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetColorProperty(propertyId).Bind(c => onValueChanged(c.ToColor()));
		}

		internal static CavrnusLivePropertyUpdate<Color> LiveUpdateColorProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, Color val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<Color>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateColorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                           string propertyId, Color value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForColorProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildColorPropertyOp(myProp.AbsoluteId,
			                                                       new ColorPropertyValue() {
				                                                       Constant = value.ToColor4().ToPb()
			                                                       });

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}

		#endregion

		#region String Props

		internal static bool StringPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                       string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForStringProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineStringPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, string defaultVal)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			if (myContainer.GetStringProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
			{
				Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
				return MiniFakeDisposable.Instance;
			}

			return myContainer.DefineStringProperty(propertyId, defaultVal,
			                                        new Cavrnus.Comm.Prop.StringProp.StringPropertyMetadata() {
				                                        Name = propertyId
													});
		}
		
		internal static void DefineStringPropertyDefinition(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, StringPropertyMetadata stringPropertyMetadata)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			if (myContainer.GetStringProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
			{
				Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
			}
			
			myContainer.DefineStringProperty(propertyId, propertyId, stringPropertyMetadata);
		}

		internal static string GetStringPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                            string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetStringProperty(propertyId).Current.Value.Value;
		}

		internal static IDisposable BindToStringProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Action<string> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
			
				internalBind = GetStringPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}
		
		internal static IDisposable GetStringPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<string> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetStringProperty(propertyId).Bind(c => { onValueChanged(c); });
		}

		internal static CavrnusLivePropertyUpdate<string> LiveUpdateStringProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, string val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<string>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateStringProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                            string propertyId, string value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForStringProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildStringPropertyOp(myProp.AbsoluteId,
			                                                        new StringPropertyValue() {Constant = value});

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}

		#endregion

		#region Boolean Props

		internal static bool BooleanPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                     string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForBooleanProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineBooleanPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, bool defaultVal)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				CheckCommonErrors(spaceConn, containerId, propertyId);
				ResolveContainerPath(ref propertyId, ref containerId);

				var myContainer = MyContainer(spaceConn, containerId);
				if (myContainer.GetBooleanProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
				{
					Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
					internalBind = MiniFakeDisposable.Instance;
				}
				else
				{
					internalBind = myContainer.DefineBooleanProperty(propertyId, defaultVal,
						new Cavrnus.Comm.Prop.BoolProp.BooleanPropertyMetadata {
							Name = propertyId
						});
				}
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}

		internal static bool GetBooleanPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                        string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetBooleanProperty(propertyId).Current.Value.Value;
		}
		
		internal static IDisposable BindToBooleanProperty(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<bool> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;

				internalBind = GetBoolPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}
		
		internal static IDisposable GetBoolPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<bool> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetBooleanProperty(propertyId).Bind(onValueChanged);
		}

		internal static CavrnusLivePropertyUpdate<bool> LiveUpdateBooleanProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, bool val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<bool>(spaceConn, containerId, propertyId, val);
		}
		
		internal static void UpdateBooleanProperty(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, bool value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForBooleanProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildBoolPropertyOp(myProp.AbsoluteId,
			                                                      new BooleanPropertyValue() {Constant = value});

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}

		#endregion

		#region Float Props

		internal static bool FloatPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                      string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForScalarProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineFloatPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, float defaultVal)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				CheckCommonErrors(spaceConn, containerId, propertyId);
				ResolveContainerPath(ref propertyId, ref containerId);

				var myContainer = MyContainer(spaceConn, containerId);
				if (myContainer.GetScalarProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
				{
					Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
					internalBind = MiniFakeDisposable.Instance;
				}
				else
				{
					internalBind = myContainer.DefineScalarProperty(propertyId, defaultVal,
					                                                new Cavrnus.Comm.Prop.ScalarProp.ScalarPropertyMetadata() {
						                                                Name = propertyId
					                                                });
				}
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}

		internal static float GetFloatPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                          string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return (float) myContainer.GetScalarProperty(propertyId).Current.Value.Value;
		}

		internal static IDisposable BindToFloatProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                              string propertyId, Action<float> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				internalBind = GetFloatPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}
		
		internal static IDisposable GetFloatPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<float> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetScalarProperty(propertyId).Bind(c => onValueChanged((float) c));
		}

		internal static CavrnusLivePropertyUpdate<float> LiveUpdateFloatProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, float val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<float>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateFloatProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                           string propertyId, float value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForScalarProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildScalarPropertyOp(myProp.AbsoluteId,
			                                                        new ScalarPropertyValue() {Constant = value});

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}

		#endregion

		#region Vector Props

		internal static bool VectorPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                       string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForVectorProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineVectorPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Vector4 defaultVal)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				CheckCommonErrors(spaceConn, containerId, propertyId);
				ResolveContainerPath(ref propertyId, ref containerId);

				var myContainer = MyContainer(spaceConn, containerId);
				if (myContainer.GetVectorProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
				{
					Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
					internalBind = MiniFakeDisposable.Instance;
				}
				else
				{
					internalBind = myContainer.DefineVectorProperty(propertyId, defaultVal.ToFloat4(),
					                                                new Cavrnus.Comm.Prop.VectorProp.VectorPropertyMetadata() {
						                                                Name = propertyId
					                                                });
				}
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}

		internal static Vector4 GetVectorPropertyValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                             string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.GetVectorProperty(propertyId).Current.Value.Value.ToVec4();
		}

		internal static IDisposable BindToVectorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Action<Vector4> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
		
				internalBind = GetVectorPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}
		
		internal static IDisposable GetVectorPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<Vector4> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetVectorProperty(propertyId).Bind(c => onValueChanged(c.ToVec4()));
		}

		internal static CavrnusLivePropertyUpdate<Vector4> LiveUpdateVectorProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, Vector4 val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<Vector4>(spaceConn, containerId, propertyId, val);
		}

		internal static void UpdateVectorProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                            string propertyId, Vector4 value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForVectorProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildVectorPropertyOp(myProp.AbsoluteId,
			                                                        new VectorPropertyValue() {
				                                                        Constant = value.ToFloat4().ToPb()
			                                                        });

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}

		#endregion

		#region Transform Props

		internal static bool TransformPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForTransformProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineTransformPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId,
		                                                  string propertyId, Vector3 defaultPos,
		                                                  Vector3 defaultRot, Vector3 defaultScl)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				CheckCommonErrors(spaceConn, containerId, propertyId);
				ResolveContainerPath(ref propertyId, ref containerId);

				var myContainer = MyContainer(spaceConn, containerId);
				if (myContainer.GetTransformProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
				{
					Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
					internalBind = MiniFakeDisposable.Instance;
				}
				else
				{
					var defaultTrans = new TransformComplete() {
						startData = new TransformDataSRT() {
							translation = defaultPos.ToFloat3(), euler = defaultRot.ToFloat3(), scale = defaultScl.ToFloat3(),
						}
					};

					internalBind = myContainer.DefineTransformProperty(propertyId, defaultTrans,
					                                           new Cavrnus.Comm.Prop.TransformProp.TransformPropertyMetadata() {
						                                           Name = propertyId
					                                           });
				}
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}

		internal static CavrnusTransformData GetTransformPropertyValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			var val = myContainer.GetTransformProperty(propertyId).Current.Value.Value;

			var res = new CavrnusTransformData(val.ResolveTranslation().ToVec3(), val.ResolveEuler().ToVec3(), val.ResolveScaleVector().ToVec3());
			return res;
		}

		internal static IDisposable BindToTransformProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                                  string propertyId,
		                                                  Action<CavrnusTransformData> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;

				if (sc == null) return;

				internalBind = GetTransformPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}

		internal static IDisposable GetTransformPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<CavrnusTransformData> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetTransformProperty(propertyId).Bind(t => {
				onValueChanged(new CavrnusTransformData(t.ResolveTranslation().ToVec3(),
					t.ResolveEuler().ToVec3(),
					t.ResolveScaleVector().ToVec3()));
			});
		}

		internal static CavrnusLivePropertyUpdate<CavrnusTransformData> LiveUpdateTransformProperty(
			CavrnusSpaceConnection spaceConn, string containerId,
			string propertyId, Vector3 localPos, Vector3 localRot, Vector3 localScl)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<CavrnusTransformData>(spaceConn, containerId, propertyId,
			                                                          new CavrnusTransformData(localPos, localRot, localScl));
		}

		internal static void UpdateTransformProperty(CavrnusSpaceConnection spaceConn, string containerId,
		                                               string propertyId, Vector3 localPos, Vector3 localRot,
		                                               Vector3 localScl, PropertyPostOptions options = null)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);
			options = options ?? new PropertyPostOptions();

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForTransformProperty(new PropertyId(propertyId));

			TransformSet trns;
			if (options.smoothed)
			{
				trns = new TransformSet()
				{
					Approach = new TransformSetApproach()
					{
						To = new TransformSet()
						{
							Srt = new TransformSetSRT()
							{
								TransformPos = new VectorPropertyValue() { Constant = localPos.ToFloat4().ToPb() },
								RotationEuler = new VectorPropertyValue() { Constant = localRot.ToFloat4().ToPb() },
								Scale = new VectorPropertyValue() { Constant = localScl.ToFloat4().ToPb() },
							}
						},
						TimeToHalf = new ScalarPropertyValue() { Constant = .1f },
						T = new ScalarPropertyValue() { Ref = new PropertyIdentifier() { Id = "t" } },
					},
				};
			}
			else
			{
				trns = new TransformSet()
				{
					Srt = new TransformSetSRT()
					{
						TransformPos = new VectorPropertyValue() { Constant = localPos.ToFloat4().ToPb() },
						RotationEuler = new VectorPropertyValue() { Constant = localRot.ToFloat4().ToPb() },
						Scale = new VectorPropertyValue() { Constant = localScl.ToFloat4().ToPb() },
					}
				};
			}

			

			var op = PropertyOperationHelpers.BuildTransformPropertyOp(myProp.AbsoluteId, trns);

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}

		#endregion

		#region JSON Props

		internal static bool JsonPropertyHasDefaultValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			return myContainer.SearchForJsonProperty(new PropertyId(propertyId)).IsDefined();
		}

		internal static IDisposable DefineJsonPropertyDefaultValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, JObject defaultVal)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				CheckCommonErrors(spaceConn, containerId, propertyId);
				ResolveContainerPath(ref propertyId, ref containerId);

				var myContainer = MyContainer(spaceConn, containerId);
				if (myContainer.GetJsonProperty(propertyId)?.Meta?.Value?.StaticDefinition ?? false)
				{
					Debug.Log($"Cannot redefine a default for {propertyId}, since it is statically defined");
					internalBind = MiniFakeDisposable.Instance;
				}
				else
				{
					var stjnode = JsonObject.Parse(defaultVal.ToString(Formatting.None));
					var stjobject = stjnode.GetValueKind() == System.Text.Json.JsonValueKind.Object ? stjnode.AsObject() : null;
					internalBind = myContainer.DefineJsonProperty(propertyId, stjobject,
						new Cavrnus.Comm.Prop.JsonProp.JsonPropertyMetadata() {
							Name = propertyId
						});
				}
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}

		internal static JObject GetJsonPropertyValue(CavrnusSpaceConnection spaceConn, string containerId, string propertyId)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);

			// Unfortunately we have to now convert from System.Text.Json to Newtonsoft.Json to preserve the interface.. Oh well. Json parse it is.
			return JObject.Parse(myContainer.GetJsonProperty(propertyId).Current.Value.Value?.ToJsonString() ?? "{}");
		}
		
		internal static IDisposable BindToJsonProperty(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<JObject> onValueChanged)
		{
			IDisposable internalBind = null;

			var spaceBind = spaceConn.CurrentSpaceConnection.Bind(sc => {
				internalBind?.Dispose();
				internalBind = null;
				
				if (sc == null) 
					return;
				
				internalBind = GetJsonPropertyBinding(spaceConn, containerId, propertyId, onValueChanged);
			});
			
			return new DelegatedDisposalHelper(() => {
				spaceBind?.Dispose();
				internalBind?.Dispose();
			});
		}
		
		internal static IDisposable GetJsonPropertyBinding(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, Action<JObject> onValueChanged)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			return myContainer.GetJsonProperty(propertyId).Bind((stjjson) => onValueChanged(JObject.Parse(stjjson.ToJsonString())));
		}

		internal static CavrnusLivePropertyUpdate<JObject> LiveUpdateJsonProperty(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, JObject val)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			return new CavrnusLivePropertyUpdate<JObject>(spaceConn, containerId, propertyId, val);
		}
		
		internal static void UpdateJsonProperty(CavrnusSpaceConnection spaceConn, string containerId, string propertyId, JObject value)
		{
			CheckCommonErrors(spaceConn, containerId, propertyId);
			ResolveContainerPath(ref propertyId, ref containerId);

			var myContainer = MyContainer(spaceConn, containerId);
			var myProp = myContainer.SearchForJsonProperty(new PropertyId(propertyId));

			var op = PropertyOperationHelpers.BuildJsonPropertyOp(myProp.AbsoluteId, new JsonPropertyValue() {ConstantJson = value.ToString()});

			var transId = PostLocalTransient(spaceConn.CurrentSpaceConnection.Value.RoomSystem, op.ToOp());
			spaceConn.CurrentSpaceConnection.Value.RoomSystem.Comm.SendJournalEntry(op.ToOp(), transId);
		}
		
		#endregion
	}
}