using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Neurable.Build
{
	public static class PostBuildScript {

		public static void CopyFileToFolder(string file, string folder)
		{
			var fInfo = new FileInfo(file);
			var shortName = fInfo.Name;
			string fullDestination = folder + "/" + shortName;
			File.Copy(file, fullDestination, true);
		}
		public static void InsertReadme(string exportPath)
		{
			CopyFileToFolder("README.md", exportPath);
		}
		public static void InsertConfig(string exportPath)
		{
			CopyFileToFolder("Assets/NeurableConfig.json", exportPath);
		}

		[PostProcessBuildAttribute(1)]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			var fInfo = new FileInfo(pathToBuiltProject);
			var projectDir = fInfo.Directory.FullName;
			InsertReadme(projectDir);
		}
	}
	
}
