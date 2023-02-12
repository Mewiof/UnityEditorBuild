#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine {

	public static class BuildStripper {

		// Sowwry~

		public static List<string> dirsToExclude = new();

		private static void CheckBuild() {
			if (!BuildPipeline.isBuildingPlayer) {
				RevertStrip();
			}
		}

		public static void Strip() {
			EditorApplication.update += CheckBuild;
			for (int i = 0; i < dirsToExclude.Count; i++) {
				string dirPath = dirsToExclude[i];
				Debug.Log(string.Concat("[Build] Stripping \"", dirPath, "\"..."));
				string errorText = AssetDatabase.MoveAsset("Assets/" + dirPath, "Assets/" + dirPath + '~');
				if (!string.IsNullOrEmpty(errorText)) {
					Debug.LogError(string.Concat("[Build] Error: \"", dirPath, "\"\n", errorText));
				}
			}
		}

		public static void RevertStrip() {
			EditorApplication.update -= CheckBuild;
			for (int i = 0; i < dirsToExclude.Count; i++) {
				string dirPath = dirsToExclude[i];
				// already contains '~'? Delete
				if (dirPath[^1..] == "~") {
					dirPath = dirPath[0..^1];
				}
				string sDirPath = dirPath + '~';
				Debug.Log(string.Concat("[Build] Reverting \"", sDirPath, "\"..."));
				string errorText = AssetDatabase.MoveAsset("Assets/" + sDirPath, "Assets/" + dirPath);
				if (!string.IsNullOrEmpty(errorText)) {
					Debug.LogError(string.Concat("[Build] Error: \"", sDirPath, "\"\n", errorText));
				}
			}
		}
	}
}
#endif
