#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.com.cavrnus.spatialconnector.Editor;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.EngineConnector;
using Cavrnus.SpatialConnector.Properties.Sync;
using Cavrnus.SpatialConnector.Setup;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cavrnus.SpatialConnector.Editor
{
	public static class CavrnusSetupHelpers
	{
		public static CavrnusEditorState EditorState { get; private set; }

		static CavrnusSetupHelpers()
		{
			EditorState = new CavrnusEditorState(); // init prefs and stuff.
			_ = EditorState.Initialize();
		}

		public static bool IsSceneSetup(out CavrnusSpatialConnector csc)
		{
			var found = Object.FindFirstObjectByType<CavrnusSpatialConnector>();
			if (found == null)
			{
				csc = null;
				return false;
			}
			else
			{
				csc = found;
				return true;
			}
		}

		public static CavrnusSpatialConnector FindOrSetupSceneForCavrnus()
		{
			var found = Object.FindFirstObjectByType<CavrnusSpatialConnector>();
			if (found == null)
			{
				return SetupSceneForCavrnus();
			}

			return found;
		}

		[MenuItem("Tools/Cavrnus/Add Cavrnus Connector to Current Scene", false, 0)]
		public static CavrnusSpatialConnector SetupSceneForCavrnus()
		{
			var found = Object.FindFirstObjectByType<CavrnusSpatialConnector>();
			if (found != null)
			{
				Debug.LogWarning("A Cavrnus Spatial Connector object already exists in your scene. If you wish to replace it please delete it first.");
				return found;
			}

			var path = CavrnusEditorHelpers.AppendPath("Assets/CavrnusSetup/PF_CavrnusSpatialConnector.prefab");
			var corePrefab = AssetDatabase.LoadAssetAtPath<CavrnusSpatialConnector>(path);

			if (corePrefab == null)
			{
				Debug.LogError($"Cavrnus Spatial Connector prefab was not found at its expected location {path} Please update or reinstall the Plugin to fix!");
				return null;
			}
			
			var ob = PrefabUtility.InstantiatePrefab(corePrefab);

			((CavrnusSpatialConnector)ob).YourServerDomain = EditorState.CavrnusServer;

			Selection.SetActiveObjectWithContext(ob, ob);
			
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();

			return corePrefab;
		}

		[MenuItem("Tools/Cavrnus/Set Selected Object As Local User", false, 10)]
		public static void SetSelectedObjectAsLocalUser()
		{
			if (Selection.activeGameObject == null)
			{
				Debug.LogError("No object has been selected to set as the local user");
				return;
			}

			if (Selection.activeGameObject.GetComponent<CavrnusLocalUserFlag>() != null)
			{
				Debug.LogWarning("Selected object is already configured to be the Local User. No further action is needed.");
			}
			else if (Object.FindFirstObjectByType<CavrnusLocalUserFlag>() != null)
			{
				Debug.LogWarning($"{Object.FindFirstObjectByType<CavrnusLocalUserFlag>().name} has already been set up as the Local User. There can be only one!");
				return;
			}
			else
			{
				Selection.activeGameObject.AddComponent<CavrnusLocalUserFlag>();
			}

			if (Selection.activeGameObject.GetComponent<CavrnusPropertySyncLocalTransform>() == null)
			{
				Debug.Log("Automatically adding a Sync Transform component to the local user, so that your CoPresence is sent to other users.");

				var st = Selection.activeGameObject.AddComponent<CavrnusPropertySyncLocalTransform>();
				st.PropertyName = "Transform";
			}

			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static async Task SetupSceneToJoinAsEditorUser()
		{
			if (!EditorState.IsLoggedIn)
			{
				throw new InvalidOperationException($"Cannot setup scene to use the editor user until the editor user is authenticated.");
			}

			CavrnusSetupHelpers.SendMetric("csc-unity-editor-settojoinaseditoruser");

			var csc = FindOrSetupSceneForCavrnus();

			// create an apikey
			var ruc = new RestUserCommunication(EditorState.editorEndpoint);
			var apikey = await ruc.PostCreateApiKey(new RestUserCommunication.CreateApiKeyRequest() { name = "unity-editor-created-for-runtime" });

			csc.YourServerDomain = EditorState.editorEndpoint.CustomerSubDomain;
			csc.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsApiKey;
			csc.MemberApiAccessKey = apikey.key;
			csc.MemberApiAccessToken = apikey.secret;

			EditorUtility.SetDirty(csc);
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static void SetupSceneToJoinAsGuest()
		{
			CavrnusSetupHelpers.SendMetric("csc-unity-editor-settojoinasguest");

			var csc = FindOrSetupSceneForCavrnus();

			if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsGuest)
				return;

			csc.YourServerDomain = EditorState.editorEndpoint.CustomerSubDomain;
			csc.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsGuest;

			EditorUtility.SetDirty(csc);
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static void SetupSceneToJoinAsUser()
		{
			CavrnusSetupHelpers.SendMetric("csc-unity-editor-settojoinasuser");

			var csc = FindOrSetupSceneForCavrnus();

			if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsMember)
				return;

			csc.YourServerDomain = EditorState.editorEndpoint.CustomerSubDomain;
			csc.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsMember;

			EditorUtility.SetDirty(csc);
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static void SetupSceneToSelectUsingSpacesList()
		{
			CavrnusSetupHelpers.SendMetric("csc-unity-editor-settousespaceslist");

			var csc = FindOrSetupSceneForCavrnus();

			if (csc.SpaceJoinMethod == CavrnusSpatialConnector.SpaceJoinOption.SpacesList)
				return;

			csc.SpaceJoinMethod = CavrnusSpatialConnector.SpaceJoinOption.SpacesList;

			EditorUtility.SetDirty(csc);
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static void SetupSceneToJoinASpecificSpaceId(string id)
		{
			CavrnusSetupHelpers.SendMetric("csc-unity-editor-settousespecificid");

			var csc = FindOrSetupSceneForCavrnus();
			if (csc.SpaceJoinMethod == CavrnusSpatialConnector.SpaceJoinOption.JoinId &&
			    csc.AutomaticSpaceJoinId == id)
				return;

			csc.SpaceJoinMethod = CavrnusSpatialConnector.SpaceJoinOption.JoinId;
			csc.AutomaticSpaceJoinId = id;

			EditorUtility.SetDirty(csc);
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static void SetupSceneToJoinAsDeviceCode()
		{
			CavrnusSetupHelpers.SendMetric("csc-unity-editor-settousedevicecodelogin");

			var csc = FindOrSetupSceneForCavrnus();

			if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinUsingDeviceCodeViaBrowser)
				return;

			csc.YourServerDomain = EditorState.editorEndpoint.CustomerSubDomain;
			csc.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinUsingDeviceCodeViaBrowser;

			if (csc.BrowserLoginMenu == null)
				Debug.LogWarning($"CavrnusSpatialConnector is now set to login using a device code, but the menu prefab to do so it not set.");

			EditorUtility.SetDirty(csc);
			CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
		}

		public static void SetupSceneToNotJoin()
		{
			if (IsSceneSetup(out var csc))
			{
				GameObject.DestroyImmediate(csc.gameObject);

				CavrnusCustomEditorUtilities.MarkCurrentSceneDirty();
			}
		}

		public static void SendMetric(string metricname)
		{
			var rmc = new RestMetaCommunication(EditorState.editorEndpoint);
			rmc.PostAppMetric(new RestMetaCommunication.AppMetricRequest()
			{
				metricId = metricname, source = "unity-csc-editor", tags = new Dictionary<string, string>() { {"deviceGuid", EditorState.uniqueGuid } }
			});
		}

		#region Sync Components Setup

		public static bool IsObSyncedWith<T>(GameObject go) where T : MonoBehaviour
		{
			return go.HasComponent<T>();
		}

		public static void SetObSyncedWith<T>(GameObject go, bool synced) where T : MonoBehaviour
		{
			if (synced == IsObSyncedWith<T>(go))
				return;

			if (synced)
				go.AddComponent<T>();
			else
				Object.DestroyImmediate(go.GetComponent<T>());
		}

		#endregion
	}
}
#endif