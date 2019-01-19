using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Neurable.Core
{
    public class VerifySteamVR
    {
        private const string STEAM_VR_DEFINITION_V1 = "NEURABLE_STEAMVR";
        private const string STEAM_VR_DEFINITION_V2 = "NEURABLE_STEAMVR_2";
        private const string STEAM_VR_DEFINITION_DEPRECATED = "Neurable_SteamVR_Support";
        private static bool _steamVrInstalled = false;
        private static bool _definesComplete = false;

        public static bool SteamVRFound
        {
            get
            {
                return _steamVrInstalled && _definesComplete;
            }
        }

        public void DrawInstallButton()
        {
            EditorGUI.BeginChangeCheck();
            var runInstaller = false;
            if (_steamVrInstalled)
            {
                if (!_definesComplete) AddDefineSteamVR();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                runInstaller = UnityEngine.GUILayout.Button("Please Install SteamVR Before Using The Neurable System",
                                                            UnityEngine.GUILayout.Height(50));
                EditorGUILayout.EndHorizontal();
            }

            if (runInstaller)
            {
                SetDirty();
                _steamVrInstalled = CheckSteamVR();
                if (!_steamVrInstalled) InstallSteamVR();
            }

            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.Refresh();
            }
        }

        private void SetDirty()
        {
            RemoveDefineSteamVR();
            _steamVrInstalled = false;
            _definesComplete = false;
        }

        private static void InstallSteamVR()
        {
            UnityEngine.Application.OpenURL("com.unity3d.kharma:content/32647");
        }

        private void RemoveDefineSteamVR()
        {
            NeurableEditorUtilities.RemoveCompilerDefinition(STEAM_VR_DEFINITION_V1);
            NeurableEditorUtilities.RemoveCompilerDefinition(STEAM_VR_DEFINITION_V2);
            NeurableEditorUtilities.RemoveCompilerDefinition(STEAM_VR_DEFINITION_DEPRECATED);
            _definesComplete = false;
        }

        private static void AddDefineSteamVR()
        {
            NeurableEditorUtilities.RemoveCompilerDefinition(STEAM_VR_DEFINITION_DEPRECATED);
            if (SVR_Version == 2)
            {
                NeurableEditorUtilities.RemoveCompilerDefinition(STEAM_VR_DEFINITION_V1);
                NeurableEditorUtilities.AddCompilerDefinition(STEAM_VR_DEFINITION_V2);
            }
            else
            {
                NeurableEditorUtilities.RemoveCompilerDefinition(STEAM_VR_DEFINITION_V2);
                NeurableEditorUtilities.AddCompilerDefinition(STEAM_VR_DEFINITION_V1);
            }
            _definesComplete = true;
        }

        private static bool HasSteamDef()
        {
            return NeurableEditorUtilities.HasCompilerDefinition(STEAM_VR_DEFINITION_V1) ||
                   NeurableEditorUtilities.HasCompilerDefinition(STEAM_VR_DEFINITION_V2);
        }

        private static int SVR_Version = 0;
        private static bool CheckSteamVR()
        {
            if (PlayerSettings.virtualRealitySupported == false)
            {
                UnityEngine.Debug.LogError("Please Enable VR Support in your Player Settings");
            }

            SVR_Version = 0;

            var foundSVR = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.Name == "SteamVR"
                select type).FirstOrDefault();
            if (foundSVR == null) return false;
            SVR_Version = foundSVR.Namespace == "Valve.VR" ? 2 : 1;
            return SVR_Version > 0;
        }

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            _steamVrInstalled = CheckSteamVR();
        }
    }
}