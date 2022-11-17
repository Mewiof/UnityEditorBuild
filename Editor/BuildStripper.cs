using System.Collections.Generic;
using UnityEditor;

public static class BuildStripper {

	public static List<string> dirsToExclude = new();

	private static void CheckBuild() {//?
		if (!BuildPipeline.isBuildingPlayer) {
			RevertStrip();
		}
	}

	public static void Strip() {
		EditorApplication.update += CheckBuild;//?
		for (int i = 0; i < dirsToExclude.Count; i++) {
			_ = AssetDatabase.MoveAsset("Assets/" + dirsToExclude[i], "Assets/" + dirsToExclude[i] + '~');
		}
	}

	public static void RevertStrip() {
		EditorApplication.update -= CheckBuild;//?
		for (int i = 0; i < dirsToExclude.Count; i++) {
			string dirPath = dirsToExclude[i];
			// already contains '~'? Delete
			if (dirPath[^1..] == "~") {
				dirPath = dirPath[0..^1];
			}
			_ = AssetDatabase.MoveAsset("Assets/" + dirPath + '~', "Assets/" + dirPath);
		}
	}
}
