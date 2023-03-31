#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// No apples? :(

namespace UnityEngine {

	public static class EditorBuild {

		public static BuildTargetGroup defaultBuildTargetGroup;
		public static BuildTarget defaultBuildTarget;

		public static string serverRunArguments;

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

		#region Save & Load
		public static void Save() {
			PlayerPrefs.SetInt(nameof(defaultBuildTargetGroup), (int)defaultBuildTargetGroup);
			PlayerPrefs.SetInt(nameof(defaultBuildTarget), (int)defaultBuildTarget);

			PlayerPrefs.SetString(nameof(serverRunArguments), serverRunArguments);

			if (BuildStripper.clientDirs.Count > 0) {
				PlayerPrefs.SetString(nameof(BuildStripper.clientDirs), string.Join(UNIQUE_SEPARATOR, BuildStripper.clientDirs));
			} else {
				PlayerPrefs.DeleteKey(nameof(BuildStripper.clientDirs));
			}
		}

		[InitializeOnLoadMethod]
		private static void Load() {
			defaultBuildTargetGroup = (BuildTargetGroup)PlayerPrefs.GetInt(nameof(defaultBuildTargetGroup), (int)BuildTargetGroup.Standalone);
			defaultBuildTarget = (BuildTarget)PlayerPrefs.GetInt(nameof(defaultBuildTarget), (int)BuildTarget.StandaloneWindows64);

			serverRunArguments = PlayerPrefs.GetString(nameof(serverRunArguments), string.Empty);

			if (PlayerPrefs.HasKey(nameof(BuildStripper.clientDirs))) {
				BuildStripper.clientDirs =
					new(PlayerPrefs.GetString(nameof(BuildStripper.clientDirs)).Split(UNIQUE_SEPARATOR, System.StringSplitOptions.None));
			}
		}
		#endregion

		public static string GetTaggedText(string text) {
			return string.Concat("[Build] ", text);
		}

		private static void Build(BuildTargetGroup targetGroup, BuildTarget target, BuildOptions options, bool server, string fileFormat, bool revealInFinder, bool run) {
			// dir & file names
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

			// sub & strip
			if (server) {
				_ = EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, target);
				EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
				playerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
				BuildStripper.Strip();
			} else {
				_ = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
				EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
				playerOptions.subtarget = (int)StandaloneBuildSubtarget.Player;
			}

			// additional options
			playerOptions.targetGroup = targetGroup;
			playerOptions.target = target;
			playerOptions.scenes = Scenes;

			// set UTC build timestamp
			BuildInfo.Instance.lastBuildAttemptTimestampTicks = System.DateTime.UtcNow.Ticks;
			BuildInfo.Instance.Save();

			BuildReport buildReport = BuildPipeline.BuildPlayer(playerOptions);

			BuildStripper.RevertStrip();

			if (buildReport.summary.result == BuildResult.Succeeded) {
				if (revealInFinder) {
					EditorUtility.RevealInFinder(playerOptions.locationPathName);
				}

				if (run) {
					_ = Process.Start(new ProcessStartInfo(playerOptions.locationPathName, server ? serverRunArguments : string.Empty));
				}

				// increment & log
				string reportLogTags = GetTaggedText(string.Concat('[', target.ToString(), "] "));

				if (!server) {
					string buildNumberStr = null;

					switch (target) {
						case BuildTarget.StandaloneWindows:
						case BuildTarget.StandaloneWindows64:
							buildNumberStr = BuildInfo.Instance.winClientBuildNumber++.ToString();
							break;
						case BuildTarget.StandaloneLinux64:
							buildNumberStr = BuildInfo.Instance.linuxClientBuildNumber++.ToString();
							break;
						case BuildTarget.Android:
							buildNumberStr = BuildInfo.Instance.androidClientBuildNumber++.ToString();
							break;
					}

					BuildInfo.Instance.Save();

					if (buildNumberStr != null) {
						reportLogTags = string.Concat(reportLogTags, '[', buildNumberStr, "] ");
					}
				}

				Debug.Log(string.Concat(reportLogTags, "Success. Ignore '.meta' warnings (if any)"));
			}

			// switch to default target
			_ = EditorUserBuildSettings.SwitchActiveBuildTarget(defaultBuildTargetGroup, defaultBuildTarget);
			EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
		}

		public const string
			WINDOWS_EXTENSION = ".exe",
			LINUX_EXTENSION = ".x86_64",
			ANDROID_BUNDLE_EXTENSION = ".aab",
			ANDROID_EXTENSION = ".apk";

		#region Windows
		private static void BuildWindows(bool server, bool run) {
			Build(BuildTargetGroup.Standalone,
				BuildTarget.StandaloneWindows64,
				BuildOptions.None,
				server,
				WINDOWS_EXTENSION,
				!run,
				run);
		}

		[MenuItem("Build/Windows (x64)/Client")]
		public static void BuildWindowsClient() {
			BuildWindows(false, false);
		}

		[MenuItem("Build/Windows (x64)/Server")]
		public static void BuildWindowsServer() {
			BuildWindows(true, false);
		}

		[MenuItem("Build/Windows (x64)/Server (Run)")]
		public static void BuildWindowsServerRun() {
			BuildWindows(true, true);
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
				true,
				false);
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
				true,
				false);
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
}
#endif
