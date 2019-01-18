using UnityEditor;
using UnityEngine;
using CognitiveVR;
using Neurable.Core;
using Neurable.Analytics;

namespace Neurable.Analytics.Portal
{
	public static class NeurableCognitiveMenu
	{
		private const string ANALYTICS_MANAGER_NAME = "Neurable Analytics Engine";

		[MenuItem("Neurable/Analytics Portal/Setup Analytics Portal", priority = -50)]
		public static void InstantiateAnalyticsManager_Init()
		{
			InitWizard.Init();
			InstantiateAnalyticsManager();
		}

		public static void InstantiateAnalyticsManager()
		{
			CognitiveCheck.AddDefineCVR();
			var cognitiveManager = Object.FindObjectOfType<CognitiveVR_Manager>();
			if (cognitiveManager == null)
			{
				CognitiveVR.EditorCore.SpawnManager(ANALYTICS_MANAGER_NAME);
			}
			else
			{
				cognitiveManager.gameObject.name = ANALYTICS_MANAGER_NAME;
			}
			NeurableMenu.InstantiateManager<NeurableCognitiveInterface>(ANALYTICS_MANAGER_NAME);
			NeurableMenu.InstantiateManager<NeurableMentalStateEngine>(ANALYTICS_MANAGER_NAME);
			NeurableMenu.InstantiateManager<NeurableEegDataEngine>(ANALYTICS_MANAGER_NAME);
			NeurableMenu.InstantiateManager<FixationEngine>(ANALYTICS_MANAGER_NAME);
			var diag = NeurableMenu.InstantiateManager<Diagnostics.HeadsetDiagnosticReporter>(ANALYTICS_MANAGER_NAME);
			diag.triggerButton = KeyCode.None;
			diag.periodicReset = false;
			diag.refreshButtonString = "";
			EditorCore.GetPreferences().Dashboard = "app.analytics.neurable.com";
			EditorCore.GetPreferences().Viewer = "viewer.analytics.neurable.com/scene/";
			EditorCore.GetPreferences().SensorSnapshotCount = 1400;
			EditorCore.GetPreferences().Documentation = "https://wiki.neurable.com/-LMnMGPGyap6Om7i9Oje/";
			if(CognitiveVR_Preferences.Instance) EditorUtility.SetDirty(CognitiveVR_Preferences.Instance);

			Selection.activeGameObject = GameObject.Find(ANALYTICS_MANAGER_NAME);
		}
	}
}