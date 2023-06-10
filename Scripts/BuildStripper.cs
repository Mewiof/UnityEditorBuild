#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine {

	public static class BuildStripper {

		public static List<string> clientDirs = new();
		private static readonly List<string> _serverDirs = new() { "ServerOnly" };
		private static List<string> _dirsToExclude;

		private static void TryMoveAsset(string pathA, string pathB) {
			string errorText = AssetDatabase.MoveAsset(pathA, pathB);
			if (string.IsNullOrEmpty(errorText)) {
				return;
			}
			Debug.LogError(EditorBuild.GetTaggedText(string.Concat("Error: \"", pathA, "\"->\"", pathB, "\"\n", errorText)));
		}

		public static void Strip(bool server) {
			_dirsToExclude = server ? clientDirs : _serverDirs;
			for (int i = 0; i < _dirsToExclude.Count; i++) {
				string dirPath = string.Concat("Assets/", _dirsToExclude[i]);
				if (!AssetDatabase.IsValidFolder(dirPath)) {
					continue;
				}
				Debug.Log(EditorBuild.GetTaggedText(string.Concat("Stripping \"", dirPath, "\"...")));
				TryMoveAsset(dirPath, string.Concat(dirPath, '~'));
			}
		}

		public static void RevertStrip() {
			for (int i = 0; i < _dirsToExclude.Count; i++) {
				string dirPath = string.Concat("Assets/", _dirsToExclude[i]);
				// already contains "~"?
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
