using UnityEngine;
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
		defaultBuildTargetGroup = (BuildTargetGroup)PlayerPrefs.GetInt(nameof(defaultBuildTargetGroup), (int)BuildTargetGroup.Standalone);
		defaultBuildTarget = (BuildTarget)PlayerPrefs.GetInt(nameof(defaultBuildTarget), (int)BuildTarget.StandaloneWindows64);

		if (PlayerPrefs.HasKey(nameof(BuildStripper.dirsToExclude))) {
			BuildStripper.dirsToExclude =
			new(PlayerPrefs.GetString(nameof(BuildStripper.dirsToExclude)).Split(UNIQUE_SEPARATOR, System.StringSplitOptions.None));
		}
	}

	public static void Save() {
		PlayerPrefs.SetInt(nameof(defaultBuildTargetGroup), (int)defaultBuildTargetGroup);
		PlayerPrefs.SetInt(nameof(defaultBuildTarget), (int)defaultBuildTarget);

		if (BuildStripper.dirsToExclude.Count > 0) {
			PlayerPrefs.SetString(nameof(BuildStripper.dirsToExclude), string.Join(UNIQUE_SEPARATOR, BuildStripper.dirsToExclude));
		} else {
			PlayerPrefs.DeleteKey(nameof(BuildStripper.dirsToExclude));
		}
	}

	private static void Build(BuildTargetGroup targetGroup, BuildTarget target, BuildOptions options, bool server, string fileFormat, bool revealInFinder) {
		string dirName = target.ToString();
		if (server) {
			dirName += "Server";
		}
		string fileName = server ? "Server" : Application.productName;//?
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

	public const string
		WINDOWS_EXTENSION = ".exe",
		LINUX_EXTENSION = ".x86_64",
		ANDROID_BUNDLE_EXTENSION = ".aab",
		ANDROID_EXTENSION = ".apk";

	#region Windows
	private static void BuildWindows(bool server) {
		Build(BuildTargetGroup.Standalone,
			BuildTarget.StandaloneWindows64,
			BuildOptions.None,
			server,
			WINDOWS_EXTENSION,
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
	#endregion

	#region Linux
	private static void BuildLinux(bool server) {
		Build(BuildTargetGroup.Standalone,
			BuildTarget.StandaloneLinux64,
			BuildOptions.None,
			server,
			LINUX_EXTENSION,
			true);
	}

	[MenuItem("Build/Linux (x64)/Client")]
	public static void BuildLinuxClient() {
		BuildLinux(false);
	}

	[MenuItem("Build/Linux (x64)/Server")]
	public static void BuildLinuxServer() {
		BuildLinux(true);
	}

	[MenuItem("Build/Linux (x64)/Both")]
	public static void BuildLinuxBoth() {
		BuildLinuxServer();
		BuildStripper.RevertStrip();
		BuildLinuxClient();
	}
	#endregion

	#region Android
	private static void BuildAndroid(bool bundle, BuildOptions options) {
		EditorUserBuildSettings.buildAppBundle = bundle;
		Build(BuildTargetGroup.Android,
			BuildTarget.Android,
			options,
			false,
			bundle ? ANDROID_BUNDLE_EXTENSION : ANDROID_EXTENSION,
			true);
	}

	[MenuItem("Build/Android/AAB")]
	public static void BuildAndroidBundle() {
		BuildAndroid(true, BuildOptions.UncompressedAssetBundle);
	}

	[MenuItem("Build/Android/APK")]
	public static void BuildAndroid() {
		BuildAndroid(false, BuildOptions.None);
	}

	[MenuItem("Build/Android/Development (Run)")]
	public static void BuildAndroidRun() {
		BuildAndroid(false, BuildOptions.AutoRunPlayer | BuildOptions.Development);
	}
	#endregion
}
