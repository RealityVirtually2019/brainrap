using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Neurable.Analytics.Portal
{
    public class CognitiveCheck
    {
        static string CognitiveDefinition = "CVR_NEURABLE";
        private static bool CVRinstalled = false;
        private static bool importsComplete = false;

		public void DrawInstallButton()
        {
            CVRinstalled = checkCognitiveManager();
            if (CVRinstalled || EditorApplication.isPlaying) return;
            EditorGUI.BeginChangeCheck();
            bool runInstaller = false;
            if (!CVRinstalled)
            {
                EditorGUILayout.BeginHorizontal();
                runInstaller = GUILayout.Button("Enable Neurable Analytics Dashboard.",
                    GUILayout.Height(50));
                EditorGUILayout.EndHorizontal();
            }

            if (runInstaller) InstallCVR();

            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.Refresh();
            }
		}

		private static bool checkImports()
		{
			var cogDefine = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
							from type in assembly.GetTypes()
							where type.Name == "CognitiveVR_Manager"
							 select type).FirstOrDefault();
			var cogNeurableDefine = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
							from type in assembly.GetTypes()
							where type.Name == "NeurableCognitiveInterface"
							select type).FirstOrDefault();
			importsComplete = cogDefine != null && cogNeurableDefine != null;
			return importsComplete;
		}

        public void InstallCVR()
        {
			AddDefineCVR();
			AssetDatabase.Refresh();
			if (!checkCognitiveManager())
			{
#if CVR_NEURABLE
				NeurableCognitiveMenu.InstantiateAnalyticsManager_Init();
#endif
			}
        }

        public static void AddDefineCVR()
        {
			if (!checkImports())
			{
				Debug.LogError("Please Import the Neurable.Cognitive3D package to Support the Analytics Dashboard.");
				return;
			}
			Core.NeurableEditorUtilities.AddCompilerDefinition(CognitiveDefinition);
        }

        private static bool checkCognitiveManager()
		{
			UnityEngine.Object _obj = null;
#if CVR_NEURABLE
			_obj = UnityEngine.Object.FindObjectOfType<CognitiveVR.CognitiveVR_Manager>();
#endif
			return _obj != null;
		}

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            CVRinstalled = checkCognitiveManager();
        }
    }
}