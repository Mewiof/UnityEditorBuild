#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine {

	public static class BuildStripper {

		public static List<string> dirsToExclude = new();

		private static void CheckBuild() {
			if (!BuildPipeline.isBuildingPlayer) {
				RevertStrip();
			}
		}

		private static void TryMoveAsset(string pathA, string pathB) {
			string errorText = AssetDatabase.MoveAsset(string.Concat("Assets/", pathA), string.Concat("Assets/", pathB));
			if (string.IsNullOrEmpty(errorText)) {
				return;
			}
			Debug.LogError(EditorBuild.GetTaggedText(string.Concat("Error: \"", pathA, "\"->\"", pathB, "\"\n", errorText)));
		}

		public static void Strip() {
			EditorApplication.update += CheckBuild;
			for (int i = 0; i < dirsToExclude.Count; i++) {
				string dirPath = dirsToExclude[i];
				Debug.Log(EditorBuild.GetTaggedText(string.Concat("Stripping \"", dirPath, "\"...")));
				TryMoveAsset(dirPath, string.Concat(dirPath, '~'));
			}
		}

		public static void RevertStrip() {
			EditorApplication.update -= CheckBuild;
			for (int i = 0; i < dirsToExclude.Count; i++) {
				string dirPath = dirsToExclude[i];
				// already contains '~'?
				if (dirPath[^1..] == "~") {
					dirPath = dirPath[0..^1];
				}
				string sDirPath = dirPath + '~';
				Debug.Log(EditorBuild.GetTaggedText(string.Concat("Reverting \"", sDirPath, "\"...")));
				TryMoveAsset(sDirPath, dirPath);
			}
		}
	}
}
#endif
