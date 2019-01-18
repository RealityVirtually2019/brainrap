/*
* Copyright 2017 Neurable Inc.
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Types = Neurable.API.Types;

namespace Neurable.Core
{
    /*
    * Class that handles communication between the Neurable API and the user
    * wearing the headset in game. Pulls data from the headset to be processed.
    */
    public class NeurableUser : MonoBehaviour
    {
        private static NeurableUser _instance;

        public static NeurableUser Instance
        {
            get
            {
                if (_instance == null)
                {
                    //Debug.LogError("Attempting to access Neurable User Instance before it is set up");
                    throw new
                        UnassignedReferenceException("Attempting to access Neurable User Instance before it is set up");
                }

                return _instance;
            }
        }

        private static bool _isAwake;

        public static bool Instantiated
        {
            get { return _isAwake && (_instance != null) && (_instance.currentUser != null); }
        }

        [Tooltip("Simulate EEG Connection without a Headset. Overridden in Standalone Players.")]
        public bool SimulateConnectionInEditor = false;
        private bool _simulateConnection = false;
        [Tooltip(
            "Port to use to connect to EEG HEadset. On Windows, an Empty String \"\" will automatically find to the proper USB port.")]
        public string DevicePort = ""; // Device Port
        [Tooltip("Path to Existing Neurable Model File")]
        public string ImportModelPath = "trained_model"; // Path to output the Trained Model to
        [Tooltip("Path to write new Neurable Models after Calibration")]
        public string ExportModelPath = "trained_model"; // Path to import the Trained Model from

        [SerializeField, Tooltip("True: Allows the user to make selections with their mind")]
        protected bool enableMentalSelect = true;
        [SerializeField,
         Tooltip(
             "True: Integrates Eye data in Neurable's Hybrid Selection system. False: Neurable makes selections using ONLY EEG.")]
        protected bool useHybridEyeSystem = true;
        [SerializeField, Tooltip("Sensitivity Settings for Hybrid System.")]
        protected API.Types.Sensitivity _hybridSensitivity = API.Types.Sensitivity.VERY_HIGH;
        [SerializeField, Tooltip("Sensitivity Settings for EEG Only System.")]
        protected API.Types.Sensitivity _EEGOnlySensitivity = API.Types.Sensitivity.HIGH;

        [SerializeField,
         Tooltip(
             "File path of Json to load settings from. File is in the following form:\n{\n\"COMPort\": \"\",\n\"ImportPath\": \"trained_model\",\n\"ExportPath\": \"trained_model\",\n\"HybridSensitivity\": 4,\n\"EEGOnlySensitivity\": 3\n}")]
        protected string jsonFilePath = "NeurableConfig.json";
        [SerializeField, Tooltip("Load settings from JSON on awake.")]
        protected bool loadJsonOnAwake = true;
        [SerializeField, Tooltip("Load settings from JSON in editor mode.")]
        protected bool loadJsonInEditorMode = false;

        [System.Serializable]
        private class NeurableUserJsonSettings
        {
            public string COMPort = "";
            public string ImportPath = "";
            public string ExportPath = "";
            public int HybridSensitivity = -1;
            public int EEGOnlySensitivity = -1;
        }

        [SerializeField, Tooltip("Metadata Header for Resulting Data File")]
        public string[] MetaDataHeader = new string[] {"Trial", ""}; // Metadata Fields for output file

        [SerializeField, Tooltip("READONLY: Is the EEG Headset Connected?")]
        protected bool EEGReady = false; // Tells inspector if EEG is active
        [SerializeField, Tooltip("READONLY: Is the Eyetracker Connected?")]
        protected bool EyetrackerReady = false; // Tells inspector if Eyetracker is active
        [Space]
        [SerializeField, Tooltip("READONLY: Timestamp Read from Headset")]
        protected double NeurableEEGTime;
        [SerializeField, Tooltip("READONLY: Data Titles")]
        protected String[] NeurableEEGChannels;
        [SerializeField, Tooltip("READONLY: Data Values")]
        protected double[] NeurableEEGData;

        [SerializeField, Tooltip("READONLY: Timestamp Read from Headset")]
        protected double NeurableEyeTime;
        [SerializeField, Tooltip("READONLY: Data Titles")]
        protected String[] NeurableEyeChannels;
        [SerializeField, Tooltip("READONLY: Data Values")]
        protected double[] NeurableEyeData;

        public API.Types.DataCallback onData; // Callback on new data sample
        protected Dictionary<String, double> HeadsetData;
        public NeurableMetadata Metadata;

        protected bool dying = false;

        #region User Creation

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = this;

            Debug.Log("Neurable Version:: " + API.Library.GetVersion());
            HeadsetData = new Dictionary<string, double>();
            EEGReady = false;
            EyetrackerReady = false;

            if (loadJsonOnAwake)
            {
                if (!Application.isEditor || (Application.isEditor && loadJsonInEditorMode)) ImportSettingsFromJson();
            }

            if (SimulateConnectionInEditor)
            {
                if (Application.isEditor)
                {
                    _simulateConnection = true;
                    Debug.LogWarning("Neurable is Simulating an EEG Connection");
                }
                else
                {
                    Debug.LogError("Simulated Data not available in Standalone Player.");
                }
            }

            InitializeUser();
            _isAwake = true;

            Debug.Log("NeurableUser has completed initilization");
        }

#if UNITY_EDITOR
        private bool _prevUseEye = false;
        private API.Types.Sensitivity _prevHybridSensitivity = Types.Sensitivity.MEDIUM;
        private API.Types.Sensitivity _prevEEGSensitivity = Types.Sensitivity.MEDIUM;
        private void Update()
        {
            if (useHybridEyeSystem != _prevUseEye)
            {
                UseHybridEyeSystem = useHybridEyeSystem;
                _prevUseEye = UseHybridEyeSystem;
            }
            if (_hybridSensitivity != _prevHybridSensitivity)
            {
                HybridSensitivity = _hybridSensitivity;
                _prevHybridSensitivity = HybridSensitivity;
            }
            if (_EEGOnlySensitivity != _prevEEGSensitivity)
            {
                EEGOnlySensitivity = _EEGOnlySensitivity;
                _prevEEGSensitivity = EEGOnlySensitivity;
            }
        }
#endif

        protected virtual void ImportSettingsFromJson()
        {
            if (!File.Exists(jsonFilePath)) return;
            string json = File.ReadAllText(jsonFilePath);
            var jsonClass = JsonUtility.FromJson<NeurableUserJsonSettings>(json);
            Instance.DevicePort = jsonClass.COMPort;
            if (CheckBeforeWrite(jsonClass.ImportPath)) ImportModelPath = jsonClass.ImportPath;
            if (CheckBeforeWrite(jsonClass.ExportPath)) ExportModelPath = jsonClass.ExportPath;
            if (CheckBeforeWrite(jsonClass.HybridSensitivity)) SetHybridSensitivity((byte) jsonClass.HybridSensitivity);
            if (CheckBeforeWrite(jsonClass.EEGOnlySensitivity)) SetEEGSensitivity((byte) jsonClass.EEGOnlySensitivity);
            Debug.Log("JSON Imported");
        }

        protected static bool CheckBeforeWrite(string reference)
        {
            return reference != "";
        }

        protected static bool CheckBeforeWrite(int reference)
        {
            return reference != -1;
        }

        protected virtual void InitializeUser()
        {
            currentUser =
                new API.User(DevicePort, !_simulateConnection); // Do not load an eyetracker in simulation mode

            Metadata = new NeurableMetadata(this, MetaDataHeader);

            onData = DebugData;
            currentUser.SetDataCallback(onData, API.Types.CallbackType.FILTERED_DATA);

            EnableMentalSelect = enableMentalSelect;
            UseHybridEyeSystem = useHybridEyeSystem;
            HybridSensitivity = _hybridSensitivity;
            EEGOnlySensitivity = _EEGOnlySensitivity;
        }

        // Return Neurable User.
        protected API.User currentUser = null;

        public API.User User
        {
            get { return currentUser; }
        }

        // Controller for Camera State
        private NeurableCamera _neurableCam = null;

        public NeurableCamera NeurableCam
        {
            get
            {
                if (_neurableCam == null)
                {
                    _neurableCam = GetComponent<NeurableCamera>();
                    if (_neurableCam == null)
                    {
                        _neurableCam = FindObjectOfType<NeurableCamera>();
                        if (_neurableCam == null)
                        {
                            Debug.LogError("Cannot Find NeurableCamera Object");
                        }
                    }
                }

                return _neurableCam;
            }
        }

        public void OnEnable()
        {
            if (User != null) User.StartDataCollection(_simulateConnection);
        }

        public void OnDisable()
        {
            if (User != null) User.StopDataCollection();
        }

        #endregion

        #region User Functions

        public bool ExportModel(string path = "")
        {
            if (path == "") path = ExportModelPath;
            if (User == null || path == "") return false;
            return User.ExportModel(path);
        }

        public bool ImportModel(string path = "")
        {
            if (path == "") path = ImportModelPath;
            if (User == null || path == "") return false;
            bool result = User.ImportModel(path);
            if (result)
            {
                Debug.Log("Import Succeeded");
            }
            else
            {
                Debug.LogError("Import Failed");
            }

            return result;
        }

        // Is the user connected and ready to collect data?
        public bool Ready
        {
            get
            {
                if (dying || User == null) return false;
                if (!EEGReady) EEGReady = User.IsConnectedEeg();
                if (!EyetrackerReady) EyetrackerReady = User.IsConnectedEye();
                return User.IsConnected();
            }
        }

        #endregion

        #region User Parameters

        // Use Neurable to make Selections
        public virtual bool EnableMentalSelect
        {
            get { return enableMentalSelect; }
            set
            {
                User.SetInteractable(value);
                enableMentalSelect = value;
            }
        }

        // Set the Sensitivity for Hybrid System use. The hybrid EEG/Eye system is faster at higher sensitivities.
        public virtual API.Types.Sensitivity HybridSensitivity
        {
            get { return _hybridSensitivity; }
            set
            {
                _hybridSensitivity = value;
                if (User == null) return;
                if (UseHybridEyeSystem)
                    User.SetSensitivity(_hybridSensitivity);
                else
                    User.SetSensitivity(_EEGOnlySensitivity);
            }
        }

        // Set the Sensitivity for EEG only Operation. The EEG Only is slower but more acurrate at lower sensitivities.
        public virtual API.Types.Sensitivity EEGOnlySensitivity
        {
            get { return _EEGOnlySensitivity; }
            set
            {
                _EEGOnlySensitivity = value;
                if (User == null) return;
                if (UseHybridEyeSystem)
                    User.SetSensitivity(_hybridSensitivity);
                else
                    User.SetSensitivity(_EEGOnlySensitivity);
            }
        }

        // Set Sensitivity with 1-5 scaling
        public void SetHybridSensitivity(byte val1to5)
        {
            if (val1to5 > 5) val1to5 = 5;
            if (val1to5 < 1) val1to5 = 1;
            HybridSensitivity = (API.Types.Sensitivity) (val1to5 * 5);
        }

        // Set EEGOnlySensitivity with 1-5 scaling
        public void SetEEGSensitivity(byte val1to5)
        {
            if (val1to5 > 5) val1to5 = 5;
            if (val1to5 < 1) val1to5 = 1;
            EEGOnlySensitivity = (API.Types.Sensitivity) (val1to5 * 5);
        }

        // Select Hybrid system options. Setting to True will allow Eyetracking to assist EEG in making selections. Setting to false activates EEG Only operation.
        public virtual bool UseHybridEyeSystem
        {
            get { return useHybridEyeSystem; }
            set
            {
                if (User != null)
                {
                    User.UseEyeTrackingThresholds(value);
                    useHybridEyeSystem = value;
                    if (!useHybridEyeSystem)
                        User.SetSensitivity(EEGOnlySensitivity);
                    else
                        User.SetSensitivity(HybridSensitivity);
                }
            }
        }

        #endregion

        #region User Data

        // Callback only used for Headset Data Collection.
        public virtual void DebugData(double timestamp, String[] titles, double[] values)
        {
            if (titles[0] == "Time")
            {
                NeurableEEGTime = timestamp;
                NeurableEEGChannels = titles;
                NeurableEEGData = values;
            }
            else if (titles[0] == "Eye_Time")
            {
                NeurableEyeTime = timestamp;
                NeurableEyeChannels = titles;
                NeurableEyeData = values;
            }

            for (int i = 0; i < titles.Length; i++)
            {
                HeadsetData[titles[i]] = values[i];
            }
        }

        // Get most recent Eye Data
        public bool GetEyeData(out double timestamp, out Vector2 eyePosition)
        {
            HeadsetData.TryGetValue("Eye_Time", out timestamp);
            eyePosition = new Vector2();

            double outX, outY;
            if (HeadsetData.TryGetValue("Eye_X", out outX) && HeadsetData.TryGetValue("Eye_Y", out outY))
            {
                eyePosition.x = (float) outX;
                eyePosition.y = (float) outY;
            }
            else
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Destruction

        protected void cleanMemory()
        {
            dying = true;
            if (currentUser != null) API.Library.DeleteUser(currentUser.GetPointer());
            currentUser = null;
        }

        void OnDestroy()
        {
            cleanMemory();
        }

        #endregion
    }
}
