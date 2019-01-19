using System.Globalization;
using UnityEngine;
using UnityEditor;

namespace Neurable.Core
{
    [CustomEditor(typeof(NeurableUser))]
	public class NeurableUserEditor : Editor
	{
		private const string SIMULATION_PROPERTY = "SimulateConnectionInEditor";
		private const string DEVICE_PORT_PROPERTY = "DevicePort";
		private const string IMPORT_MODEL_PATH_PROPERTY = "ImportModelPath";
		private const string EXPORT_MODEL_PATH_PROPERTY = "ExportModelPath";
		private const string ENABLE_MENTAL_SELECT_PROPERTY = "enableMentalSelect";
		private const string USE_HYBRID_EYE_SYSTEM_PROPERTY = "useHybridEyeSystem";
		private const string HYBRID_SENSITIVITY_PROPERTY = "_hybridSensitivity";
		private const string EEG_ONLY_SENSITIVITY_PROPERTY = "_EEGOnlySensitivity";
		private const string JSON_FILE_PATH_PROPERTY = "jsonFilePath";
		private const string LOAD_JSON_ON_AWAKE_PROPERTY = "loadJsonOnAwake";
		private const string LOAD_JSON_IN_EDITOR_MODE_PROPERTY = "loadJsonInEditorMode";
		private const string META_DATA_HEADER_PROPERTY = "MetaDataHeader";
		private const string EEG_READY_PROPERTY = "EEGReady";
		private const string EYE_TRACKER_READY_PROPERTY = "EyetrackerReady";
		private const string NEURABLE_DEVICE_TIME_PROPERTY = "NeurableEEGTime";
		private const string NEURABLE_DEVICE_CHANNELS_PROPERTY = "NeurableEEGChannels";
		private const string NEURABLE_DEVICE_DATA_PROPERTY = "NeurableEEGData";
		//private const string NEURABLE_DEVICE2_TIME_PROPERTY = "NeurableEyeTime";
		private const string NEURABLE_DEVICE2_CHANNELS_PROPERTY = "NeurableEyeChannels";
		private const string NEURABLE_DEVICE2_DATA_PROPERTY = "NeurableEyeData";

		private VerifySteamVR _VerifySteamVR = new VerifySteamVR();
		private Analytics.Portal.CognitiveCheck _CognitiveCheck = new Analytics.Portal.CognitiveCheck();

		private SerializedProperty _simulationProperty;
		private SerializedProperty _devicePortProperty;
		private SerializedProperty _importModelPathProperty;
		private SerializedProperty _exportModelPathProperty;
		private SerializedProperty _enableMentalSelectProperty;
		private SerializedProperty _useHybridEyeSystemProperty;
		private SerializedProperty _hybridSensitivityProperty;
		private SerializedProperty _eegOnlySensitivityProperty;
		private SerializedProperty _jsonFilePathProperty;
		private SerializedProperty _loadJsonOnAwakeProperty;
		private SerializedProperty _loadJsonInEditorModeProperty;
		private SerializedProperty _metaDataHeaderProperty;
		private SerializedProperty _eegReadyProperty;
		private SerializedProperty _eyetrackerReadyProperty;
		//private SerializedProperty _neurableEEGTimeProperty;
		private SerializedProperty _neurableEEGChannelsProperty;
		private SerializedProperty _neurableEEGDataProperty;
		//private SerializedProperty _neurableEyeTimeProperty;
		private SerializedProperty _neurableEyeChannelsProperty;
		private SerializedProperty _neurableEyeDataProperty;

		private bool _fontsSetup;
		private GUIStyle _okayLabelStyle;
		private GUIStyle _badLabelStyle;
		private GUIStyle _headerLabelStyle;
		private Color _2Gray = new Color(.2f, .2f, .2f);
		private Color _1Gray = new Color(.15f, .15f, .15f);


		public virtual void OnEnable()
		{
			// Fetch the objects from the GameObject script to display in the inspector
			_simulationProperty = serializedObject.FindProperty(SIMULATION_PROPERTY);
			_devicePortProperty = serializedObject.FindProperty(DEVICE_PORT_PROPERTY);
			_importModelPathProperty = serializedObject.FindProperty(IMPORT_MODEL_PATH_PROPERTY);
			_exportModelPathProperty = serializedObject.FindProperty(EXPORT_MODEL_PATH_PROPERTY);
			_enableMentalSelectProperty = serializedObject.FindProperty(ENABLE_MENTAL_SELECT_PROPERTY);
			_useHybridEyeSystemProperty = serializedObject.FindProperty(USE_HYBRID_EYE_SYSTEM_PROPERTY);
			_hybridSensitivityProperty = serializedObject.FindProperty(HYBRID_SENSITIVITY_PROPERTY);
			_eegOnlySensitivityProperty = serializedObject.FindProperty(EEG_ONLY_SENSITIVITY_PROPERTY);
			_jsonFilePathProperty = serializedObject.FindProperty(JSON_FILE_PATH_PROPERTY);
			_loadJsonOnAwakeProperty = serializedObject.FindProperty(LOAD_JSON_ON_AWAKE_PROPERTY);
			_loadJsonInEditorModeProperty = serializedObject.FindProperty(LOAD_JSON_IN_EDITOR_MODE_PROPERTY);
			_metaDataHeaderProperty = serializedObject.FindProperty(META_DATA_HEADER_PROPERTY);
			_eegReadyProperty = serializedObject.FindProperty(EEG_READY_PROPERTY);
			_eyetrackerReadyProperty = serializedObject.FindProperty(EYE_TRACKER_READY_PROPERTY);
			//_neurableEEGTimeProperty = serializedObject.FindProperty(NEURABLE_DEVICE_TIME_PROPERTY);
			_neurableEEGChannelsProperty = serializedObject.FindProperty(NEURABLE_DEVICE_CHANNELS_PROPERTY);
			_neurableEEGDataProperty = serializedObject.FindProperty(NEURABLE_DEVICE_DATA_PROPERTY);
			//_neurableEyeTimeProperty = serializedObject.FindProperty(NEURABLE_DEVICE2_TIME_PROPERTY);
			_neurableEyeChannelsProperty = serializedObject.FindProperty(NEURABLE_DEVICE2_CHANNELS_PROPERTY);
			_neurableEyeDataProperty = serializedObject.FindProperty(NEURABLE_DEVICE2_DATA_PROPERTY);
		}

		public override void OnInspectorGUI()
		{
			SetupFonts();
			DrawDeviceSettings();
			DrawSelectionSettings();
			DrawJsonLoadingSettings();
			DrawAdvancedSettings();
			DrawHeadsetDataReadOnly();
			DrawInstallSVR();
			DrawInstallCVR();
			serializedObject.ApplyModifiedProperties();
		}

		protected void SetupFonts()
		{
			if (_fontsSetup)
			{
				return;
			}

			_okayLabelStyle = new GUIStyle(EditorStyles.label);
			_okayLabelStyle.normal.textColor = Color.green;

			_badLabelStyle = new GUIStyle(EditorStyles.label);
			_badLabelStyle.normal.textColor = Color.red;

			_headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
			_headerLabelStyle.normal.textColor = new Color(.9f, .9f, .9f);
			_fontsSetup = true;
		}

		protected void DrawInstallSVR()
		{
			_VerifySteamVR.DrawInstallButton();
		}

		protected void DrawInstallCVR()
		{
			_CognitiveCheck.DrawInstallButton();
		}

		protected void DrawHeader(string title)
		{
			var rect = EditorGUILayout.BeginVertical();
			EditorGUI.DrawRect(rect, _2Gray);
			EditorGUILayout.LabelField(title, _headerLabelStyle);
			EditorGUILayout.EndVertical();
		}

		protected void DrawDeviceSettings()
		{
			DrawHeader("Device Setttings");
			EditorGUILayout.PropertyField(_simulationProperty);
			EditorGUILayout.PropertyField(_devicePortProperty);
			EditorGUILayout.PropertyField(_importModelPathProperty);
			EditorGUILayout.PropertyField(_exportModelPathProperty);
		}

		protected void DrawSelectionSettings()
		{
#if NEURABLE_INTERACTIONS
			bool selectionAvailable = VerifySteamVR.SteamVRFound;
			if (!selectionAvailable)
			{
				var style = new GUIStyle(_headerLabelStyle);
				style.stretchHeight = true;
				style.alignment = TextAnchor.MiddleLeft;
				style.fixedHeight = 50f;
				var rect = EditorGUILayout.BeginHorizontal(style);
				EditorGUI.DrawRect(rect, _2Gray);
				EditorGUILayout.LabelField("Selection Settings", style);
				EditorGUILayout.EndHorizontal();
			} else {
				DrawHeader("Selection Settings");
			}
			EditorGUILayout.PropertyField(_enableMentalSelectProperty);
			if (_enableMentalSelectProperty.boolValue)
			{
				EditorGUILayout.PropertyField(_useHybridEyeSystemProperty);
				using (new EditorGUI.DisabledScope(!_useHybridEyeSystemProperty.boolValue))
				{
					EditorGUILayout.PropertyField(_hybridSensitivityProperty);
				}

				using (new EditorGUI.DisabledScope(_useHybridEyeSystemProperty.boolValue))
				{
					EditorGUILayout.PropertyField(_eegOnlySensitivityProperty);
				}
			}
#endif
		}

		protected void DrawJsonLoadingSettings()
		{
			DrawHeader("JSON Import Settings");

			EditorGUILayout.PropertyField(_jsonFilePathProperty);
			EditorGUILayout.PropertyField(_loadJsonOnAwakeProperty);
			EditorGUILayout.PropertyField(_loadJsonInEditorModeProperty);
		}

		protected void DrawAdvancedSettings()
		{
			DrawHeader("Advanced Settings");

			EditorGUILayout.PropertyField(_metaDataHeaderProperty, true);
		}

		protected void DrawHeadsetDataReadOnly()
		{
			if (Application.isPlaying)
			{
				var rect = EditorGUILayout.BeginVertical();
				EditorGUI.DrawRect(rect, _1Gray);
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Headset Data (Read Only)", _headerLabelStyle);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("EEG State:");
				if (_eegReadyProperty.boolValue)
				{
					EditorGUILayout.LabelField("Ready", _okayLabelStyle);
				}
				else
				{
					EditorGUILayout.LabelField("Not Connected", _badLabelStyle);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Eyetracker State:");
				if (_eyetrackerReadyProperty.boolValue)
				{
					EditorGUILayout.LabelField("Ready", _okayLabelStyle);
				}
				else
				{
					EditorGUILayout.LabelField("Not Connected", _badLabelStyle);
				}
				EditorGUILayout.EndHorizontal();

				// EditorGUILayout.PropertyField(_neurableDeviceTimeProperty);
				DrawChannelAndDataInfoReadOnly();
				EditorGUILayout.EndVertical();
			}
		}

		private void DrawChannelAndDataInfoReadOnly()
		{
			_neurableEEGChannelsProperty.isExpanded = EditorGUILayout.Foldout(_neurableEEGChannelsProperty.isExpanded, "Neurable EEG Data");
			if (_neurableEEGChannelsProperty.isExpanded)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Channels");
				EditorGUILayout.LabelField("Data");
				EditorGUILayout.EndHorizontal();

				var count = Mathf.Min(_neurableEEGChannelsProperty.arraySize, _neurableEEGDataProperty.arraySize);
				for (int i = 0; i < count; i++)
				{
					var rect = EditorGUILayout.BeginHorizontal();
					if (i % 2 == 0)
					{
						EditorGUI.DrawRect(rect, _2Gray);
					}
					EditorGUILayout.LabelField(_neurableEEGChannelsProperty.GetArrayElementAtIndex(i).stringValue);
					EditorGUILayout.LabelField(_neurableEEGDataProperty.GetArrayElementAtIndex(i).floatValue.ToString("e", CultureInfo.InvariantCulture));
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;
			}
			if (_neurableEyeChannelsProperty.arraySize > 0)
			{
				_neurableEyeChannelsProperty.isExpanded = EditorGUILayout.Foldout(_neurableEyeChannelsProperty.isExpanded, "Neurable Eye Data");
				if (_neurableEyeChannelsProperty.isExpanded)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Channels");
					EditorGUILayout.LabelField("Data");
					EditorGUILayout.EndHorizontal();

					var count = Mathf.Min(_neurableEyeChannelsProperty.arraySize, _neurableEyeDataProperty.arraySize);
					for (int i = 0; i < count; i++)
					{
						var rect = EditorGUILayout.BeginHorizontal();
						if (i % 2 == 0)
						{
							EditorGUI.DrawRect(rect, _2Gray);
						}
						EditorGUILayout.LabelField(_neurableEyeChannelsProperty.GetArrayElementAtIndex(i).stringValue);
						EditorGUILayout.LabelField(_neurableEyeDataProperty.GetArrayElementAtIndex(i).floatValue.ToString("e", CultureInfo.InvariantCulture));
						EditorGUILayout.EndHorizontal();
					}
					EditorGUI.indentLevel--;
				}

			}
		}
	}
}
