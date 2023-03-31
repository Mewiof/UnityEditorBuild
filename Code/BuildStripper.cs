#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine {

	public static class BuildStripper {

		public static List<string> clientDirs = new();

		private static void CheckBuild() {
			if (!BuildPipeline.isBuildingPlayer) {
				RevertStrip();
			}
		}

		private static void TryMoveAsset(string pathA, string pathB) {
			string errorText = AssetDatabase.MoveAsset(pathA, pathB);
			if (string.IsNullOrEmpty(errorText)) {
				return;
			}
			Debug.LogError(EditorBuild.GetTaggedText(string.Concat("Error: \"", pathA, "\"->\"", pathB, "\"\n", errorText)));
		}

		public static void Strip() {
			EditorApplication.update += CheckBuild;
			for (int i = 0; i < clientDirs.Count; i++) {
				string dirPath = string.Concat("Assets/", clientDirs[i]);
				if (!AssetDatabase.IsValidFolder(dirPath)) {
					continue;
				}
				Debug.Log(EditorBuild.GetTaggedText(string.Concat("Stripping \"", dirPath, "\"...")));
				TryMoveAsset(dirPath, string.Concat(dirPath, '~'));
			}
		}

		public static void RevertStrip() {
			EditorApplication.update -= CheckBuild;
			for (int i = 0; i < clientDirs.Count; i++) {
				string dirPath = string.Concat("Assets/", clientDirs[i]);
				// already contains '~'?
				if (dirPath[^1..] == "~") {
					dirPath = dirPath[0..^1];
				}
				string sDirPath = dirPath + '~';
				if (!AssetDatabase.IsValidFolder(sDirPath)) {
					continue;
				}
				Debug.Log(EditorBuild.GetTaggedText(string.Concat("Reverting \"", sDirPath, "\"...")));
				TryMoveAsset(sDirPath, dirPath);
			}
		}
	}
}
#endif
