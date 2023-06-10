using System;

namespace UnityEngine {

	public sealed class BuildInfo : ScriptableObject {

		private static BuildInfo _instance;
		internal static BuildInfo Instance {
			get {
				if (_instance == null) {
					// load
					BuildInfo[] assets = Resources.LoadAll<BuildInfo>(string.Empty);
					if (assets == null || assets.Length < 1) {
#if UNITY_EDITOR
						string resDirPath = System.IO.Path.Combine(Application.dataPath, "Resources");
						if (!System.IO.Directory.Exists(resDirPath)) {
							System.IO.Directory.CreateDirectory(resDirPath);
						}
						_instance = CreateInstance<BuildInfo>();
						UnityEditor.AssetDatabase.CreateAsset(_instance, System.IO.Path.Combine("Assets", "Resources", "BuildInfo.asset"));
						UnityEditor.AssetDatabase.SaveAssets();
						UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
#else
						Debug.LogError("Failed to load a build info resource");
						_instance = new();
#endif
					} else {
						if (assets.Length > 1) {
							Debug.LogWarning("Found several build info resources. Using the first one...");
						}
						_instance = assets[0];
					}
				}
				return _instance;
			}
		}

		#region Data
		[SerializeField]
		internal long
			winClientBuildNumber = 1,
			linuxClientBuildNumber = 1,
			androidClientBuildNumber = 1;

		[SerializeField]
		internal long lastBuildTimestamp;
		#endregion

		internal void UpdateLastBuildTimestamp() {
			lastBuildTimestamp = DateTime.UtcNow.Ticks;
		}

#if UNITY_EDITOR
		internal void Save() {
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
		}
#endif

		#region Public
		public static long BuildNumber => Application.platform switch {
			RuntimePlatform.WindowsPlayer => Instance.winClientBuildNumber,
			RuntimePlatform.LinuxPlayer => Instance.linuxClientBuildNumber,
			RuntimePlatform.Android => Instance.androidClientBuildNumber,
			_ => -1,
		};

		/// <summary>UTC</summary>
		public static DateTime Timestamp => new(Instance.lastBuildTimestamp);
		#endregion
	}
}
