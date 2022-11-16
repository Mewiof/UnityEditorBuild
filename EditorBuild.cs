using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class BuildStripper {

	// Changeable
	private static readonly string[] _foldersToExclude = new string[] {
		"Audio",
		"Sprites",
		"ThirdParty/TMPro/Shaders"
	};

	private static string _strValue;

	private static void CheckBuild() {
		if (!BuildPipeline.isBuildingPlayer) {
			RevertStrip();
		}
	}

	public static void Strip() {
		EditorApplication.update += CheckBuild;
		for (int i = 0; i < _foldersToExclude.Length; i++) {
			_ = AssetDatabase.MoveAsset("Assets/" + _foldersToExclude[i], "Assets/" + _foldersToExclude[i] + '~');
		}
	}

	public static void RevertStrip() {
		EditorApplication.update -= CheckBuild;
		for (int i = 0; i < _foldersToExclude.Length; i++) {
			_strValue = _foldersToExclude[i];
			if (_strValue[^1..] == "~") {
				_strValue = _strValue[0..^1];
			}
			_ = AssetDatabase.MoveAsset("Assets/" + _strValue + '~', "Assets/" + _strValue);
		}
	}
}

public static class EditorBuild {

	// Changeable
	private static readonly string[] _scenes = new string[] {
		"Assets/Scenes/MainScene.unity"
	};

	// Changeable
	private static readonly BuildTargetGroup _defaultBuildTargetGroup = BuildTargetGroup.Standalone;
	// Changeable
	private static readonly BuildTarget _defaultBuildTarget = BuildTarget.StandaloneWindows64;

	private static void Build(BuildTargetGroup targetGroup, BuildTarget target, BuildOptions options, bool server, string fileFormat, bool revealInFinder) {
		BuildPlayerOptions playerOptions = new() {
			locationPathName = "Builds/" + target + (server ? "Server" : string.Empty) + '/' + (server ? "Server" : UnityEngine.Application.productName) + fileFormat,
			options = options
		};

		playerOptions.targetGroup = targetGroup;
		playerOptions.target = target;
		playerOptions.subtarget = server ? (int)StandaloneBuildSubtarget.Server : (int)StandaloneBuildSubtarget.Player;

		if (server) {//?
			_ = EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, target);//?
			EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;//?
			BuildStripper.Strip();
		}
		else {
			_ = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);//?
			EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;//?
		}

		playerOptions.scenes = _scenes;

		BuildReport buildReport = BuildPipeline.BuildPlayer(playerOptions);

		if (revealInFinder && buildReport.summary.result == BuildResult.Succeeded) {
			EditorUtility.RevealInFinder(playerOptions.locationPathName);
		}

		_ = EditorUserBuildSettings.SwitchActiveBuildTarget(_defaultBuildTargetGroup, _defaultBuildTarget);//?
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;//?
	}

	private static void BuildWindows(bool server) {
		Build(BuildTargetGroup.Standalone,
			BuildTarget.StandaloneWindows64,
			BuildOptions.None,
			server,
			".exe",
			true);
	}

	[MenuItem("Build/Windows (x64)/Client")]
	public static void BuildWindowsClient() {
		BuildWindows(false);
	}

	[MenuItem("Build/Windows (x64)/Server")]
	public static void BuildWindowsServer() {
		BuildWindows(true);
	}

	[MenuItem("Build/Windows (x64)/Both")]
	public static void BuildWindowsBoth() {
		BuildWindowsServer();
		BuildStripper.RevertStrip();
		BuildWindowsClient();
	}

	[MenuItem("Build/Android")]
	public static void BuildAndroid() {
		EditorUserBuildSettings.buildAppBundle = true;
		Build(BuildTargetGroup.Android,
			BuildTarget.Android,
			BuildOptions.UncompressedAssetBundle,
			false,
			".aab",
			true);
	}

	[MenuItem("Build/Android (Run)")]
	public static void BuildAndroidRun() {
		EditorUserBuildSettings.buildAppBundle = false;
		Build(BuildTargetGroup.Android,
			BuildTarget.Android,
			BuildOptions.AutoRunPlayer | BuildOptions.Development,
			false,
			".apk",
			true);
	}
}
