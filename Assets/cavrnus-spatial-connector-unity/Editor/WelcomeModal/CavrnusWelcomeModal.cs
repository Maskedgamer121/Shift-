#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading.Tasks;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.EngineConnector;
using Cavrnus.SpatialConnector.Properties.Sync;
using Cavrnus.SpatialConnector.Setup;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Editor
{
	public class CavrnusWelcomeModal : EditorWindow
    {
        private static readonly Vector2 WindowSize = new Vector2(600, 720);
		private readonly Vector2 textInputSize = new Vector2(270, 20);
		private readonly Vector2 mainButtonSize = new Vector2(270, 40);
        private readonly Vector2 smallerButtonSize = new Vector2(140, 27);
        
        private const float Space = 10;
        private const int Padding = 10;

        private bool first = true;


		private static CavrnusWelcomeModal window;

        [MenuItem("Tools/Cavrnus/Welcome")]
        public static void Init()
        {
            ShowWindow();
        }

        private static void ShowWindow()
        {
			window = GetWindow<CavrnusWelcomeModal>();
            window.CreateCenteredWindow("Cavrnus Spatial Connector", WindowSize);
            window.CenterOnMainWin();

            window.ShowPopup();
        }

        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            if (EditorPrefs.GetBool("WelcomeModal_HasOpenedDuringSession", false))
                return;

            EditorPrefs.SetBool("WelcomeModal_HasOpenedDuringSession", true);

            EditorApplication.update += HandleUpdate;
        }
        
        [InitializeOnLoadMethod]
        public static void OnShutdown()
        {
            EditorApplication.quitting += () =>
            {
                EditorPrefs.DeleteKey("WelcomeModal_HasOpenedDuringSession");
            };
        }
        
        private static void HandleUpdate()
        {
            if (CavrnusSetupHelpers.EditorState.shouldAutoOpen && !Application.isPlaying) 
                ShowWindow();

            EditorApplication.update -= HandleUpdate;
        }

        private GameObject lastSel = null;
        private void Update()
        {
	        if (Selection.activeGameObject != lastSel)
	        {
		        lastSel = Selection.activeGameObject;
                Repaint();
	        }
        }

        private void OnGUI()
        {
	        if (first)
	        {
		        CavrnusSetupHelpers.SendMetric("csc-openwelcome");
		        first = false;
	        }

            ShowHeader();

            CavrnusCustomEditorUtilities.CreateLabel("Instantly add multi-user collaboration and synchronization to your project.");
            
            CavrnusCustomEditorUtilities.AddSpace(Space);

            GUILayout.BeginHorizontal();
            GUILayout.Space(Padding); // Left padding

            GUILayout.BeginVertical();
            
            CavrnusCustomEditorUtilities.AddSpace(Space);

            GUILayout.EndVertical();

            CavrnusCustomEditorUtilities.AddSpace(Space);
            
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();

            var showConnection = CavrnusSetupHelpers.EditorState.isAuthenticateOpen = EditorGUILayout.Foldout(CavrnusSetupHelpers.EditorState.isAuthenticateOpen, "Cavrnus Authentication");

            if (showConnection)
            {
	            CavrnusCustomEditorUtilities.CreateLabel("Sign in to Cavrnus inside Unity Editor for quicker testing and development");

	            if (CavrnusSetupHelpers.EditorState.IsLoggedIn)
	            {
                    CavrnusCustomEditorUtilities.AddSpace(5);
		            CavrnusCustomEditorUtilities.CreateLabel($"You are authenticated, as {CavrnusSetupHelpers.EditorState.loggedInUserName}, member of {CavrnusSetupHelpers.EditorState.loggedInOrgName}", 12, false, TextAnchor.MiddleCenter);
		            CavrnusCustomEditorUtilities.AddSpace(5);
		            CreateMediumButtonCentered("Log out", new Vector2(120, 30), () =>
					{
						CavrnusSetupHelpers.EditorState.Logout();

						if (CavrnusSetupHelpers.IsSceneSetup(out var csc))
						{
                            // wipe out apikeys..
							csc.AuthenticationMethod = CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsGuest;
							csc.MemberApiAccessKey = "";
							csc.MemberApiAccessToken = "";
							csc.AutomaticSpaceJoinId = "";
						}
					});
	            }
	            else
	            {
		            CreateLargeButton("Sign In or Sign Up", textInputSize, async () =>
		            {
			            await CavrnusSetupHelpers.EditorState.DoConnectToCavrnus();

			            if (!CavrnusSetupHelpers.IsSceneSetup(out var csc))
			            {
                            Debug.Log($"Automatically setting up current Scene to use Cavrnus; You can change this selection in the Cavrnus Welcome menu or by removing the prefabs that will be added.");

				            var cscx = CavrnusSetupHelpers.SetupSceneForCavrnus();
                            cscx.YourServerDomain = CavrnusSetupHelpers.EditorState.CavrnusServer;

							await CavrnusSetupHelpers.SetupSceneToJoinAsEditorUser();
							EditorUtility.SetDirty(cscx);

                            CavrnusSetupHelpers.SetupSceneToJoinASpecificSpaceId(CavrnusSetupHelpers.EditorState.loggedInRoomsTestingSpaceId);
			            }
			            else
			            {
				            csc.YourServerDomain = CavrnusSetupHelpers.EditorState.CavrnusServer;

                            if (csc.AuthenticationMethod != CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsApiKey)
	                            await CavrnusSetupHelpers.SetupSceneToJoinAsEditorUser();

                            if (CavrnusSetupHelpers.EditorState.loggedInRooms.Length == 0) // 0 before we created one. Assign it as the join target.
								CavrnusSetupHelpers.SetupSceneToJoinASpecificSpaceId(CavrnusSetupHelpers.EditorState.loggedInRoomsTestingSpaceId);

							EditorUtility.SetDirty(csc);
			            }
		            });

		            CavrnusCustomEditorUtilities.AddSpace(5);

					if (!String.IsNullOrWhiteSpace(CavrnusSetupHelpers.EditorState.deviceLoginError))
		            {
			            GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
			            GUILayout.Label($"Failed to login: {CavrnusSetupHelpers.EditorState.deviceLoginError}. Please try again.");
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
		            }
		            else if (CavrnusSetupHelpers.EditorState.awaitingDeviceLogin)
		            {
			            GUILayout.BeginHorizontal();
			            GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical();
                        GUILayout.FlexibleSpace();
			            if (EditorGUILayout.LinkButton("Open link here if your browser does not open automatically"))
			            {
				            Application.OpenURL(CavrnusSetupHelpers.EditorState.deviceLoginUrl);
			            }
						GUILayout.Label($"Use code {CavrnusSetupHelpers.EditorState.deviceLoginCode} in the Cavrnus console to log in your Unity Editor");
			            GUILayout.FlexibleSpace();
                        GUILayout.EndVertical();
			            GUILayout.FlexibleSpace();
			            GUILayout.EndHorizontal();
		            }

		            CavrnusCustomEditorUtilities.AddSpace(5);
				}

				CreateTextInput("Your Server Domain", textInputSize, () => CavrnusSetupHelpers.EditorState.CavrnusServer, (server) =>
	            {
		            if (server != CavrnusSetupHelpers.EditorState.CavrnusServer && !server.StartsWith(".") && !server.EndsWith("."))
		            {
			            CavrnusSetupHelpers.EditorState.ChangeServer(server);
			            if (CavrnusSetupHelpers.IsSceneSetup(out var csc))
			            {
				            csc.YourServerDomain = CavrnusSetupHelpers.EditorState.CavrnusServer;
                            EditorUtility.SetDirty(csc);
			            }
		            }
	            }, "Optional, unless connecting to a nonstandard server. This will be filled out for you automatically when logging in via your browser by hitting Connect to Cavrnus.");
            }

            CavrnusCustomEditorUtilities.AddSpace(5);
            CavrnusCustomEditorUtilities.AddDivider();
            CavrnusCustomEditorUtilities.AddSpace(5);

			var showJoinOptions = CavrnusSetupHelpers.EditorState.isSetupOpen = EditorGUILayout.Foldout(CavrnusSetupHelpers.EditorState.isSetupOpen, "Join Options");

            if (showJoinOptions)
            {
	            CavrnusCustomEditorUtilities.CreateLabel("Choose how you want to join at runtime.");
	            CavrnusCustomEditorUtilities.AddSpace(15);


				bool isSetup = CavrnusSetupHelpers.IsSceneSetup(out var csc);
	            GUIContent currentVal = new GUIContent();
	            if (!isSetup)
		            currentVal.text = "Not Configured to use Cavrnus";
                else if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsApiKey)
                {
	                currentVal.text = $"Join as you ({CavrnusSetupHelpers.EditorState.loggedInUserName})";
	                currentVal.tooltip = "More accurately, join using an apikey, which was probably set up by using the 'Join as You' option in the editor.";
                }
                else if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsGuest)
	            {
		            if (csc.GuestLoginMethod == CavrnusSpatialConnector.GuestLoginOptionEnum.EnterNameBelow)
			            currentVal.text = $"Join as a Guest with Name '{csc.GuestName}'";
		            else
			            currentVal.text = $"Join as a Guest with a name prompt";
	            }
                else if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinAsMember)
                {
	                if (csc.MemberLoginMethod == CavrnusSpatialConnector.MemberLoginOptionEnum.EnterMemberLoginCredentials)
		                currentVal.text = $"Join as a Member '{csc.MemberEmail}'";
	                else
		                currentVal.text = $"Join using a login prompt";
                }
                else if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.Custom)
	                currentVal.text = $"Use Custom Authentication";
                else if (csc.AuthenticationMethod == CavrnusSpatialConnector.AuthenticationOptionEnum.JoinUsingDeviceCodeViaBrowser)
	                currentVal.text = "Join using a browser-based login prompt";

	            GUILayout.BeginHorizontal();
	            GUILayout.Space(40);
	            var selected = EditorGUILayout.Popup(new GUIContent("Join Method", "This setting can be modified by inspecting the CavrnusSpatialConnector component. This selector is provided just to make it easy."),
		            0, new GUIContent[]
		            {
                        currentVal,
                        new GUIContent(""),
			            new GUIContent($"Join a you ({CavrnusSetupHelpers.EditorState.loggedInUserName})"),
			            new GUIContent($"Join as Member"),
			            new GUIContent($"Join as Guest"),
                        new GUIContent($"Join using a browser-based login prompt")
			            //new GUIContent($"Remove Cavrnus from Scene")
		            }
	            );
	            GUILayout.Space(40);
	            GUILayout.EndHorizontal();

				if (selected > 1)
	            {
		            if (selected == 2)
		            {
			            _ = CavrnusSetupHelpers.SetupSceneToJoinAsEditorUser();
		            }
                    else if (selected == 3)
                    {
                        CavrnusSetupHelpers.SetupSceneToJoinAsUser();
                    }
                    else if (selected == 4)
                    {
	                    CavrnusSetupHelpers.SetupSceneToJoinAsGuest();
                    }
                    else if (selected == 5)
                    {
	                    CavrnusSetupHelpers.SetupSceneToJoinAsDeviceCode();
                    }
	            }

	            if (isSetup)
	            {
		            CavrnusCustomEditorUtilities.AddSpace(5);

		            GUIContent joinVal = new GUIContent("Custom Space Selection");
					// Space select mode dropdown
					if (csc.SpaceJoinMethod == CavrnusSpatialConnector.SpaceJoinOption.SpacesList)
					{
						joinVal.text = "Connect using the SpacesList GUI";
						joinVal.tooltip = "At runtime, this will open a dialog with all of the user's available spaces to select to be joined.";
					}
					else if (csc.SpaceJoinMethod == CavrnusSpatialConnector.SpaceJoinOption.JoinId)
					{
                        if (csc.AutomaticSpaceJoinId == CavrnusSetupHelpers.EditorState.loggedInRoomsTestingSpaceId)
	                        joinVal.text = $"Connect to the Testing Space '{csc.AutomaticSpaceJoinId}'";
						else
							joinVal.text = $"Connect to Id '{csc.AutomaticSpaceJoinId}'";
					}

					GUILayout.BeginHorizontal();
					GUILayout.Space(40);
					var selectedSpace = EditorGUILayout.Popup(new GUIContent("Space Selection", "This controls how the application will choose which session/space to connect to. This setting can be modified by inspecting the CavrnusSpatialConnector component. This selector is provided just to make it easy. By default the CSC will create a testing space for you to join, but you may want to let the user pick their space, or write your own code to do so."),
						0, new GUIContent[]
						{
							joinVal,
							new GUIContent(""),
							new GUIContent($"Connect using the SpacesList GUI"),
							new GUIContent($"Connect to the Testing Space ({CavrnusSetupHelpers.EditorState.loggedInRoomsTestingSpaceId})"),
							new GUIContent($"Connect to a specific Join Id"),
							//new GUIContent($"Set to Join using a login menu"),
							//new GUIContent($"Remove Cavrnus from Scene")
						}
					);
					GUILayout.Space(40);
					GUILayout.EndHorizontal();

					if (selectedSpace > 1)
					{
						if (selectedSpace == 2)
						{
							CavrnusSetupHelpers.SetupSceneToSelectUsingSpacesList();
						}
						else if (selectedSpace == 3)
						{
							CavrnusSetupHelpers.SetupSceneToJoinASpecificSpaceId(CavrnusSetupHelpers.EditorState.loggedInRoomsTestingSpaceId);
						}
						else if (selectedSpace == 4)
						{
							CavrnusSetupHelpers.SetupSceneToJoinASpecificSpaceId("");
                            Debug.LogWarning($"The Join Id needs to be assigned in the {nameof(CavrnusSpatialConnector)} component.");
						}
					}
				}
				
	            CavrnusCustomEditorUtilities.AddSpace(15);
			}

            CavrnusCustomEditorUtilities.AddSpace(5);
            CavrnusCustomEditorUtilities.AddDivider();
            CavrnusCustomEditorUtilities.AddSpace(5);

            var showSyncOptions = CavrnusSetupHelpers.EditorState.isSyncOpen = EditorGUILayout.Foldout(CavrnusSetupHelpers.EditorState.isSyncOpen, "GameObject Sync Setup");

            if (showSyncOptions)
            {
	            CavrnusCustomEditorUtilities.CreateLabel("Quickly add built-in Cavrnus Sync Components to Game Objects.", 15, false, TextAnchor.MiddleLeft, "You can write your own sync components! You are not limited to these examples, but they are a useful place to start.");

	            if (Selection.activeGameObject == null)
	            {
                    CavrnusCustomEditorUtilities.CreateLabel($"Select a GameObject to show example sync options.", 14, false, TextAnchor.MiddleLeft, "Alternatively, you can easily add sync components directly to the GameObjects, or write your own. Use the examples, such as CavrnusPropertySyncVisibility as a guide.");
	            }
	            else
	            {
		            var go = Selection.activeGameObject;

		            GUILayout.BeginHorizontal();
		            GUILayout.Space(40);
                    
                    GUILayout.BeginVertical();

                    var issyncedworld = CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncWorldTransform>(go);
                    var issyncedlocal = CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncLocalTransform>(go);

                    EditorGUI.BeginDisabledGroup(issyncedworld);
                    {
	                    bool synclocal = EditorGUILayout.Toggle(new GUIContent("Sync Local Transform", "Synchronizes this object's local transform"),
		                    CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncLocalTransform>(go));
	                    CavrnusSetupHelpers.SetObSyncedWith<CavrnusPropertySyncLocalTransform>(go, synclocal); // this is safe if the value doesn't change.
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(issyncedlocal);
                    {
	                    bool syncworld = EditorGUILayout.Toggle(new GUIContent("Sync World Transform", "Synchronizes this object's world transform"),
		                    CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncWorldTransform>(go));
	                    CavrnusSetupHelpers.SetObSyncedWith<CavrnusPropertySyncWorldTransform>(go, syncworld); // this is safe if the value doesn't change.
                    }
                    EditorGUI.EndDisabledGroup();

                    bool syncvis = EditorGUILayout.Toggle(new GUIContent("Sync Visibility", "Synchronizes this object's active state"),
	                    CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncVisibility>(go));
                    CavrnusSetupHelpers.SetObSyncedWith<CavrnusPropertySyncVisibility>(go, syncvis); // this is safe if the value doesn't change.

                    GUILayout.EndVertical();

                    var haslight = go.TryGetComponent<Light>(out var _);
                    if (haslight)
                    {
                        GUILayout.BeginVertical();

                        bool synclightc = EditorGUILayout.Toggle(new GUIContent("Sync Light Color", "Synchronizes this object's light's color"),
	                        CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncLightColor>(go));
                        CavrnusSetupHelpers.SetObSyncedWith<CavrnusPropertySyncLightColor>(go, synclightc); // this is safe if the value doesn't change.

                        bool synclighti = EditorGUILayout.Toggle(new GUIContent("Sync Light Intensity", "Synchronizes this object's light's intensity"),
	                        CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncLightIntensity>(go));
                        CavrnusSetupHelpers.SetObSyncedWith<CavrnusPropertySyncLightIntensity>(go, synclighti); // this is safe if the value doesn't change.


						GUILayout.EndVertical();
                    }
                    var hasmat = go.TryGetComponent<MeshRenderer>(out var _) || go.TryGetComponent<SkinnedMeshRenderer>(out var _);
                    if (hasmat)
                    {
	                    GUILayout.BeginVertical();

	                    bool syncmatc = EditorGUILayout.Toggle(new GUIContent("Sync Material Color", "Synchronizes this object's material's color"),
		                    CavrnusSetupHelpers.IsObSyncedWith<CavrnusPropertySyncMaterialColor>(go));
	                    CavrnusSetupHelpers.SetObSyncedWith<CavrnusPropertySyncMaterialColor>(go, syncmatc); // this is safe if the value doesn't change.
						
						GUILayout.EndVertical();
                    }
					GUILayout.Space(40);
                    GUILayout.EndHorizontal();
	            }

			}

			CavrnusCustomEditorUtilities.AddSpace(5);
            CavrnusCustomEditorUtilities.AddDivider();
            CavrnusCustomEditorUtilities.AddSpace(25);

           // CreateMediumButtonCentered("Required Project Settings", mainButtonSize, ()=> Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/845381657/Required+Project+Settings"));
           // CreateMediumButtonCentered("Add Cavrnus Connector to Current Scene", mainButtonSize, ()=>CavrnusSetupHelpers.SetupSceneForCavrnus());
            
            CavrnusCustomEditorUtilities.AddSpace(5);
			
            GUILayout.EndVertical();

            CavrnusCustomEditorUtilities.AddSpace(50); 
            CavrnusCustomEditorUtilities.AddSpace(50); 
            CavrnusCustomEditorUtilities.AddSpace(50); 
            // Footer area

            GUILayout.BeginArea(new Rect(0, position.height - 80, position.width, 80));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(18); // Left padding
            CavrnusCustomEditorUtilities.CreateButton("Getting Started Guide", new Vector2(200, 27), () => Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/827457539/Cavrnus+Spatial+Connector+for+Unity"));
            GUILayout.FlexibleSpace();
            CavrnusCustomEditorUtilities.CreateButton("Discord", new Vector2(120, 27), () => Application.OpenURL($"https://discord.gg/pjYY8ubf5t"));
            GUILayout.FlexibleSpace();
            CavrnusCustomEditorUtilities.CreateButton("Cavrnus Management Console", new Vector2(200, 27), () => Application.OpenURL($"https://{CavrnusSetupHelpers.EditorState.editorEndpoint.ToUserDomainString()}/"));
            GUILayout.Space(18); // Right padding
			GUILayout.EndHorizontal();
			

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Space(18); // Left padding
            CavrnusCustomEditorUtilities.CreateButton("Dismiss", new Vector2(220, 27), Close);
            GUILayout.FlexibleSpace();
            var shouldAutoOpen = GUILayout.Toggle(CavrnusSetupHelpers.EditorState.shouldAutoOpen, "Show when Unity starts");
            if (shouldAutoOpen != CavrnusSetupHelpers.EditorState.shouldAutoOpen)
	            CavrnusSetupHelpers.EditorState.SetShouldAutoOpen(shouldAutoOpen);
            GUILayout.Space(18); // Right padding
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace(); // Bottom padding

            GUILayout.EndVertical();

            // End horizontal layout group
            GUILayout.EndArea();

            var textColor = Color.white;
            CavrnusCustomEditorUtilities.CreateLabelAbsolutePos(CavrnusPackageInfo.Name, new Rect(15, 10, 200, 20), textColor, 11, true);
            CavrnusCustomEditorUtilities.CreateLabelAbsolutePos($"Version {CavrnusPackageInfo.Version}", new Rect(WindowSize.x - 115, 10, 100, 20), textColor, 11, true, TextAnchor.MiddleRight);
        }

        private void ShowHeader()
        {
            GUILayout.BeginVertical();
                var cacheColor = GUI.color;
                
                var boxStyle = new GUIStyle(GUI.skin.box) {
                    alignment = TextAnchor.MiddleCenter, 
                    fontStyle = FontStyle.Bold,
                    fontSize = 30,
                };
                
                var path = CavrnusEditorHelpers.AppendPath("Editor/WelcomeModal/T_Cav-logo.png");
                var assetsImg = CavrnusCustomEditorUtilities.LoadTextureFromFile(path);
                GUILayout.Box(assetsImg, boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(170));
                
                GUI.color = cacheColor;

            GUILayout.EndVertical();
        }

        private void CreateTextInput(string text, Vector2 size, Func<string> getString, Action<string> onEdit, string tooltip = "")
        {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();

			string res = CavrnusCustomEditorUtilities.CreateTextFieldWithLabel(getString(), text, 10, (int)size.x, tooltip);
			onEdit(res);

			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void CreateLargeButton(string text, Vector2 size, Action onClick)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            
            CavrnusCustomEditorUtilities.CreateLargeButton(text, size,0, onClick);
            
            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        private void CreateLargeButtonWithColor(string text, Vector2 size, Color color, Action onClick)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            
            CavrnusCustomEditorUtilities.CreateLargeButtonWithColor(text, size,0, color, onClick);
            
            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        private void CreateMediumButton(string text, Vector2 size, Action onClick)
        {
            GUILayout.BeginHorizontal();
            
            CavrnusCustomEditorUtilities.CreateLargeButton(text, size,0, onClick);
            
            GUILayout.EndHorizontal();
        }

        private void CreateMediumButtonCentered(string text, Vector2 size, Action onClick)
        {
	        GUILayout.BeginHorizontal();
	        GUILayout.FlexibleSpace();
	        GUILayout.BeginVertical();

			CavrnusCustomEditorUtilities.CreateLargeButton(text, size, 0, onClick);

			GUILayout.Space(10);
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
	        GUILayout.EndHorizontal();
        }
    }
}
#endif