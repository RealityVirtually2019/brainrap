using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Neurable.Core
{
    [InitializeOnLoad]
    public class NeurableDefineSymbolsManager : Editor
    {
        private const string NEURABLE_FOLDER = "Neurable";

        private static readonly string[] Folders = {"Analytics", "Analytics/Portal", "Core", "Diagnostics", "Interactions", "Internal"};
        private static readonly string[] Symbols =
        {
            "NEURABLE_ANALYTICS",
            "CVR_NEURABLE",
            "NEURABLE_CORE",
            "NEURABLE_DIAGNOSTICS",
            "NEURABLE_INTERACTIONS",
            "NEURABLE_INTERNAL"
        };

        static NeurableDefineSymbolsManager()
        {
            UpdateDefineSymbols();
            EditorApplication.projectWindowChanged += UpdateDefineSymbols;
        }

        private static void UpdateDefineSymbols()
        {            
            var neurableBasePath = GetNeurablePackagePath();

            if (string.IsNullOrEmpty(neurableBasePath))
            {
                ModifyDefines(new List<string>(), Symbols.ToList());
                return;
            }
            
            List<string> definesToAdd;
            List<string> definesToRemove;

            GetDefines(neurableBasePath, out definesToAdd, out definesToRemove);

            ModifyDefines(definesToAdd, definesToRemove);
        }

        private static string GetNeurablePackagePath()
        {
            var assets = AssetDatabase.FindAssets("NeurableDefineSymbolsManager");

            if (assets.Length < 1)
            {
                Debug.LogWarning("Failed to find NeurableDefineSymbolsManager");
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(assets[0]);

            var splitPath = path.Split('/');
            var pathBuilder = new StringBuilder();
            var foundNeurableBaseFolder = false;
            for (int i = 0; i < splitPath.Length; i++)
            {
                pathBuilder.Append(splitPath[i]);
                pathBuilder.Append("/");
                if (splitPath[i] == NEURABLE_FOLDER)
                {
                    foundNeurableBaseFolder = true;
                    break;
                }
            }

            if (!foundNeurableBaseFolder)
            {
                Debug.LogWarning("Failed to find Neurable folder, please make sure you haven't renamed the top level Neurable folder");
                return null;
            }

            return pathBuilder.ToString();
        }

        private static void GetDefines(string neurableBasePath, out List<string> toAdd, out List<string> toRemove)
        {
            toAdd = new List<string>();
            toRemove = new List<string>();

            for (int i = 0; i < Folders.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(neurableBasePath + Folders[i]))
                {
                    toAdd.Add(Symbols[i]);
                }
                else
                {
                    toRemove.Add(Symbols[i]);
                }
            }
        }

        private static void ModifyDefines(List<string> definesToAdd, List<string> definesToRemove)
        {
            string definesString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            var newDefines = definesToAdd.Except(allDefines).ToList();
            allDefines.AddRange(newDefines);
            var removedDefines = new List<string>();
            allDefines.RemoveAll(x =>
                                 {
                                     var tooRemove = definesToRemove.Contains(x);
                                     if (tooRemove)
                                     {
                                         removedDefines.Add(x);
                                     }

                                     return tooRemove;
                                 });

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                                                             string.Join(";", allDefines.ToArray()));

            if (newDefines.Count > 0)
            {
                Debug.Log("Added define symbol(s): " + string.Join(", ", newDefines.ToArray()));
            }

            if (removedDefines.Count > 0)
            {
                Debug.Log("Removed define symbol(s): " + string.Join(", ", removedDefines.ToArray()));
            }
        }
    }
}
