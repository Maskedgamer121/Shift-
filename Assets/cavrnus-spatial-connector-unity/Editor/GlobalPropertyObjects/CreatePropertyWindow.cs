#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using Cavrnus.SpatialConnector.API;

namespace Cavrnus.SpatialConnector.Properties.Editor
{
	public class CreatePropertyWindow : EditorWindow
	{
		private static CreatePropertyWindow window;

		[MenuItem("Assets/Create/Cavrnus/Property")]
		private static void Init()
		{
			window = (CreatePropertyWindow)GetWindow(typeof(CreatePropertyWindow));
			window.titleContent = new GUIContent("Create Property");
			window.minSize = new Vector2(600, 435);
			window.maxSize = new Vector2(600, 435);
			window.Show();
		}

		public enum GlobalPropertyType
		{
			Bool,
			Float,
			String,
			Vector,
			Color
		}

		private string propertyName;
		private GlobalPropertyType propType;
		

		private CavrnusPropertyBinder.PropertyContainerType containerType = CavrnusPropertyBinder.PropertyContainerType.StaticContainer;
		private string containerName;

		private bool siteWideUserMetadata = false;

		private bool defaultBoolVal;
		private float defaultFloatVal;
		private string defaultStringVal;
		private Vector3 defaultVectorVal;
		private Color defaultColorVal = new Color(1, 1, 1, 1);
		private CavrnusTransformData defaultTransformData;

		private SelectFolderLocationEditor folderLocationEditor;

		private void OnDestroy()
		{
			folderLocationEditor = null;
		}

		private void OnEnable()
		{
			folderLocationEditor ??= CreateInstance<SelectFolderLocationEditor>();
			folderLocationEditor.Init("SavePropFolderLocation");
		}

		void OnGUI()
		{
			StartPadding();
			
			StartGroupContainer();
			if (EditorGUILayout.LinkButton("[Documentation]"))
				Application.OpenURL("https://cavrnus.atlassian.net/wiki/spaces/CSM/pages/1361608713/Property+Binders");
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			folderLocationEditor?
				.HeaderTitle("Save Asset To Location")
				.ShowSaveValue(true)
				.FieldLength(new Vector2(350, 350))
				.SetFolderPathLabel("Folder*");
			folderLocationEditor?.UpdateGUI();
			EndGroupContainer();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			StartGroupContainer();
			EditorGUILayout.LabelField("Property", EditorStyles.boldLabel);
			DrawPropertyFields();
			EndGroupContainer();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			StartGroupContainer();
			EditorGUILayout.LabelField("Container", EditorStyles.boldLabel);
			ShowContainerDescription();
			ShowContainerName();
			EndGroupContainer();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			StartGroupContainer();
			EditorGUILayout.LabelField("Default Value", EditorStyles.boldLabel);
			ShowDefaultValue();
			EndGroupContainer();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			GUILayout.FlexibleSpace(); // Push everything up
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); // Push the button to the right

			EditorGUILayout.LabelField("*Required Fields", EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight + 10));
			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(!HasValidSettings());

			if (GUILayout.Button("Create", GUILayout.Width(120), GUILayout.Height(30)))
			{
				if(string.IsNullOrEmpty(propertyName))
				{
					Debug.LogError("Requires Property Name.");
					EndPadding();
					return;
				}

				foreach (var fileNameChar in Path.GetInvalidFileNameChars())
				{
					if(propertyName.Contains(fileNameChar))
					{
						Debug.LogError($"Invalid character \"{fileNameChar}\" in Property Name.");
						EndPadding();
						return;
					}
				}
				if(name.Contains('.'))
				{
					Debug.LogError("Invalid character \".\" in Property Name.");
					EndPadding();
					return;
				}

				CreateGlobal();
			}

			EditorGUI.EndDisabledGroup();
			EndPadding();
		}

		private bool HasValidSettings()
		{
			return !String.IsNullOrEmpty(folderLocationEditor.FolderPath) && !String.IsNullOrEmpty(propertyName);
		}

		private void StartPadding()
		{
			GUILayout.Space(20); // Top padding
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20); // Left padding
			
			EditorGUILayout.BeginVertical();
		}

		private void EndPadding()
		{
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20); // Right padding
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20); // Bottom padding
		}

		private void DrawPropertyFields()
		{
			propType = (GlobalPropertyType)EditorGUILayout.EnumPopup("Type", propType, GUILayout.MinWidth(100), GUILayout.MaxWidth(250));
			propertyName = EditorGUILayout.TextField("Name*", propertyName, GUILayout.MinWidth(350), GUILayout.MaxWidth(350));
		}

		private void ShowContainerDescription()
		{
			EditorGUILayout.BeginHorizontal();
			containerType = (CavrnusPropertyBinder.PropertyContainerType)EditorGUILayout.EnumPopup("Type", containerType, GUILayout.MinWidth(350), GUILayout.MaxWidth(350));
	
			var Description = "";
			switch (containerType)
			{
				case CavrnusPropertyBinder.PropertyContainerType.StaticContainer:
					Description = "Provide the Container name below";
					break;
				
				case CavrnusPropertyBinder.PropertyContainerType.LocalUserContainer:
					Description = "Local User Container - automatically place this property in the Local User's Container";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			EditorGUILayout.HelpBox(Description, MessageType.None, true);
			EditorGUILayout.EndHorizontal();
		}

		private void ShowContainerName()
		{
			EditorGUILayout.Space();

			switch (containerType)
			{
				case CavrnusPropertyBinder.PropertyContainerType.StaticContainer:
					containerName = EditorGUILayout.TextField("Name", containerName, GUILayout.MinWidth(350), GUILayout.MaxWidth(350));
					break;
				
				case CavrnusPropertyBinder.PropertyContainerType.LocalUserContainer:
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextField("Name", "Local User Container", GUILayout.MinWidth(350), GUILayout.MaxWidth(350));
					EditorGUI.EndDisabledGroup();
					
					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal();
					var helpMsg = "This makes the data available across all spaces, including outside them. Ideal for usernames, avatars, etc.";
					siteWideUserMetadata = EditorGUILayout.Toggle("Save To User Account ", siteWideUserMetadata);
					
					EditorGUILayout.HelpBox(helpMsg, MessageType.None, true);
					EditorGUILayout.EndHorizontal();

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ShowDefaultValue()
		{
			switch (propType)
			{
				case GlobalPropertyType.Bool:
					defaultBoolVal = EditorGUILayout.Toggle("Bool Value", defaultBoolVal);
					break;
				case GlobalPropertyType.Float:
					defaultFloatVal = EditorGUILayout.FloatField("Float Value", defaultFloatVal, GUILayout.MinWidth(100), GUILayout.MaxWidth(320));
					break;
				case GlobalPropertyType.String:
					defaultStringVal = EditorGUILayout.TextField("Text Value", defaultStringVal, GUILayout.MinWidth(100), GUILayout.MaxWidth(320));
					break;
				case GlobalPropertyType.Vector:
					defaultVectorVal = EditorGUILayout.Vector3Field("Vector3 Value", defaultVectorVal, GUILayout.MinWidth(100), GUILayout.MaxWidth(320));
					break;
				case GlobalPropertyType.Color:
					defaultColorVal = EditorGUILayout.ColorField("Color Value", defaultColorVal, GUILayout.MinWidth(100), GUILayout.MaxWidth(320));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void StartGroupContainer()
		{
			GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.padding = new RectOffset(10, 10, 5, 5);

			EditorGUILayout.BeginVertical(boxStyle, GUILayout.ExpandWidth(true));
		}

		private void EndGroupContainer()
		{
			EditorGUILayout.EndVertical();
		}

		private void CreateGlobal()
		{
			CavrnusPropertyBinder prop = null;
			
			if (propType == GlobalPropertyType.Bool)
			{
				prop = CreateInstance<CavrnusPropertyBinderBool>();
				(prop as CavrnusPropertyBinderBool).DefaultValue = defaultBoolVal;
				
			}
			if (propType == GlobalPropertyType.String)
			{
				prop = CreateInstance<CavrnusPropertyBinderString>();
				(prop as CavrnusPropertyBinderString).DefaultValue = defaultStringVal;

			}
			if (propType == GlobalPropertyType.Float)
			{
				prop = CreateInstance<CavrnusPropertyBinderFloat>();
				(prop as CavrnusPropertyBinderFloat).DefaultValue = defaultFloatVal;

			}
			if (propType == GlobalPropertyType.Vector)
			{
				prop = CreateInstance<CavrnusPropertyBinderVector>();
				(prop as CavrnusPropertyBinderVector).DefaultValue = defaultVectorVal;

			}
			if (propType == GlobalPropertyType.Color)
			{
				prop = CreateInstance<CavrnusPropertyBinderColor>();
				(prop as CavrnusPropertyBinderColor).DefaultValue = defaultColorVal;
			}

			prop.PropertyName = propertyName;
			prop.ContainerName = containerName;
			prop.ContainerType = containerType;
			prop.IsUserMetadata = siteWideUserMetadata;

			if (!AssetDatabase.IsValidFolder(folderLocationEditor.FolderPath))
				Directory.CreateDirectory(folderLocationEditor.FolderPath);

			if (File.Exists($"{folderLocationEditor.FolderPath}/{prop.PropertyName}.asset"))
			{
				Debug.LogError($" Choose a unique property name - a property asset named <b>[{propertyName}]</b> already exists at desired location!");
				return;
			}
			
			AssetDatabase.CreateAsset(prop, $"{folderLocationEditor.FolderPath}/{prop.PropertyName}.asset");
			AssetDatabase.SaveAssets();
			
			Debug.Log($"New property asset named <b>[{propertyName}]</b> created in <b>[{folderLocationEditor.FolderPath}]</b>");

			Selection.activeObject = prop;
			window.Close();
		}
	}
}
#endif