using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neurable.Analytics.Portal
{
	public class NeurableCognitiveInterface : MonoBehaviour
	{
		public static NeurableCognitiveInterface instance;

		[SerializeField, Tooltip("Name of the Session in the Web Portal. Defaults to DateTime.")]
		private string _sessionName = "";
		public string SessionName
		{
			get { return _sessionName; }
			set
			{
				if (value == "") SessionName = GetDefaultSessionName();
				else
				{
					_sessionName = value;
					SetSessionName(_sessionName);
				}
			}
		}

		private void Awake()
		{
			if (instance != null && instance != this) Debug.LogWarning(instance.name + " has Already instanced NeurableCognitiveInterface");
			instance = this;
		}

		private void Start()
		{
			SessionName = _sessionName;
		}

		public string GetDefaultSessionName()
		{
			var dt = System.DateTime.Now;
			var sessionTime = dt.ToString("yyyy_MM_dd_HH:mm:ss");
			var sceneName = gameObject.scene.name;
			return sceneName + "_" + sessionTime;
		}

		private static void SetSessionName(string name)
		{
			CognitiveVR.CognitiveVR_Manager.SetSessionName(name);
		}

		public static void RecordPoint(string typename, float value)
		{
			if (!CognitiveVR.Core.Initialized) return;
			CognitiveVR.SensorRecorder.RecordDataPoint(typename, value);
		}

		public static void UserReadyEvent()
		{
			if (!CognitiveVR.Core.Initialized) return;
			var userReady = new CognitiveVR.CustomEvent("Neurable Headset Ready");
			userReady.SetProperties(new Dictionary<string, object>
			{
				{ "EEG Headset", Core.NeurableUser.Instance.User.IsConnectedEeg()},
				{ "Eyetracker", Core.NeurableUser.Instance.User.IsConnectedEye()}
			});
			userReady.Send();
		}

		public static void BaselineResetEvent()
		{
			if (!CognitiveVR.Core.Initialized) return;
			CognitiveVR.CustomEvent userReset = new CognitiveVR.CustomEvent("Neurable Baseline Reset");
			userReset.Send();
		}
	}
}
