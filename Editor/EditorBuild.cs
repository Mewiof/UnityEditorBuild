using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public static class EditorBuild {

	public static BuildTargetGroup defaultBuildTargetGroup;
	public static BuildTarget defaultBuildTarget;

	private static string[] Scenes {
		get {
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			string[] result = new string[scenes.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = scenes[i].path;
			}
			return result;
		}
	}

	public const string UNIQUE_SEPARATOR = "MeWiof~OwO~MeWiof";

	[InitializeOnLoadMethod]
	private static void Load() {
		defaultBuildTargetGroup = (BuildTargetGroup)EditorPrefs.GetInt(nameof(defaultBuildTargetGroup), (int)BuildTargetGroup.Standalone);
		defaultBuildTarget = (BuildTarget)EditorPrefs.GetInt(nameof(defaultBuildTarget), (int)BuildTarget.StandaloneWindows64);

		string dirsToExcludeDefaultValue = string.Concat("Audio", UNIQUE_SEPARATOR, "Sprites");
		BuildStripper.dirsToExclude =
			new(EditorPrefs.GetString(nameof(BuildStripper.dirsToExclude), dirsToExcludeDefaultValue).Split(UNIQUE_SEPARATOR, System.StringSplitOptions.None));
	}

	public static void Save() {
		EditorPrefs.SetInt(nameof(defaultBuildTargetGroup), (int)defaultBuildTargetGroup);
		EditorPrefs.SetInt(nameof(defaultBuildTarget), (int)defaultBuildTarget);

		EditorPrefs.SetString(nameof(BuildStripper.dirsToExclude), string.Join(UNIQUE_SEPARATOR, BuildStripper.dirsToExclude));
	}

	private static void Build(BuildTargetGroup targetGroup, BuildTarget target, BuildOptions options, bool server, string fileFormat, bool revealInFinder) {
		string dirName = target.ToString();
		if (server) {
			dirName += "Server";
		}
		string fileName = server ? "Server" : UnityEngine.Application.productName;//?
		fileName += fileFormat;

		BuildPlayerOptions playerOptions = new() {
			locationPathName = System.IO.Path.Combine("Builds", dirName, fileName),
			options = options
		};

		if (server) {
			_ = EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, target);
			EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
			BuildStripper.Strip();
		} else {
			_ = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
			EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
		}

		playerOptions.targetGroup = targetGroup;
		playerOptions.target = target;
		playerOptions.subtarget = server ? (int)StandaloneBuildSubtarget.Server : (int)StandaloneBuildSubtarget.Player;
		playerOptions.scenes = Scenes;

		BuildReport buildReport = BuildPipeline.BuildPlayer(playerOptions);

		if (revealInFinder && buildReport.summary.result == BuildResult.Succeeded) {
			EditorUtility.RevealInFinder(playerOptions.locationPathName);
		}

		_ = EditorUserBuildSettings.SwitchActiveBuildTarget(defaultBuildTargetGroup, defaultBuildTarget);
		EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
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
