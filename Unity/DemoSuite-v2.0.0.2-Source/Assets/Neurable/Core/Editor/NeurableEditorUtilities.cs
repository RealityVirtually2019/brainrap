using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Neurable.Core
{
	public static class NeurableEditorUtilities
	{
		public static void AddCompilerDefinition(string define)
		{
			var scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
			var scriptingDefines = scriptingDefine.Split(';');
			var listDefines = scriptingDefines.ToList();
			if (listDefines.Contains(define)) return;

			listDefines.Add(define);
			var newDefines = string.Join(";", listDefines.ToArray());
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
		}

		public static void RemoveCompilerDefinition(string define)
		{
			var scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
			var scriptingDefines = scriptingDefine.Split(';');
			var listDefines = scriptingDefines.ToList();
			if (!listDefines.Contains(define)) return;

			listDefines.Remove(define);
			var newDefines = string.Join(";", listDefines.ToArray());
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
		}

		public static bool HasCompilerDefinition(string define)
		{
			var scriptingDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
			var scriptingDefines = scriptingDefine.Split(';');
			var listDefines = scriptingDefines.ToList();
			return listDefines.Contains(define);
		}
	}
}
