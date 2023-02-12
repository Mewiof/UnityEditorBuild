using System.IO;

namespace UnityEngine {

	[System.Serializable]
	public sealed class BuildInfo {

		public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, string.Concat(nameof(BuildInfo), ".json"));

		private static BuildInfo _instance;
		internal static BuildInfo Instance {
			get {
				if (_instance == null) {
					// load
					string path = Path;
					if (!File.Exists(path)) {
						_instance = new();
					} else {
						_instance = JsonUtility.FromJson<BuildInfo>(File.ReadAllText(path));
					}
				}
				return _instance;
			}
		}

		#region Data
		[SerializeField]
		internal int
			winClientBuildNumber = 1,
			linuxClientBuildNumber = 1,
			androidClientBuildNumber = 1;

		[SerializeField]
		internal long timestampTicks;
		#endregion

		public static int BuildNumber => Application.platform switch {
			RuntimePlatform.WindowsPlayer => Instance.winClientBuildNumber,
			RuntimePlatform.LinuxPlayer => Instance.linuxClientBuildNumber,
			RuntimePlatform.Android => Instance.androidClientBuildNumber,
			_ => -1,
		};

		public static System.DateTime Timestamp => new(Instance.timestampTicks);

		public void Save() {
			if (!Directory.Exists(Application.streamingAssetsPath)) {
				Directory.CreateDirectory(Application.streamingAssetsPath);
			}
			File.WriteAllText(Path, JsonUtility.ToJson(this));
		}
	}
}
