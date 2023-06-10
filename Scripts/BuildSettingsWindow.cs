#if UNITY_EDITOR
using UnityEditor;

namespace UnityEngine {

	public sealed class BuildSettingsWindow : EditorWindow {

		[MenuItem("Build/Settings", priority = 32)]
		private static void ShowWindow() {
			_ = GetWindow<BuildSettingsWindow>(false, "Build Settings");
		}

		/// <summary>Util</summary>
		public static Texture2D CreateColorTexture(Color color) {
			Texture2D result = new(1, 1, TextureFormat.RGBA32, false);
			result.SetPixel(0, 0, color);
			result.Apply();
			return result;
		}

		private static GUIStyle _scrollBackgroundStyle;
		private static GUIStyle _richTextLabel;
		private static GUIStyle _elemStyle;
		private static GUIStyle _selectedElemStyle;

		private readonly Vector2 _size = new(512f, 448f);

		private void SetupStyles() {
			_scrollBackgroundStyle = new(EditorStyles.label);
			_scrollBackgroundStyle.active.background = _scrollBackgroundStyle.normal.background =
				CreateColorTexture(EditorGUIUtility.isProSkin ? new Color32(40, 40, 40, 255) : new Color32(128, 128, 128, 255));

			_richTextLabel = new(EditorStyles.label) {
				richText = true
			};

			_elemStyle = new(EditorStyles.label) {
				fixedHeight = 24f
			};
			_elemStyle.active.background = _elemStyle.normal.background =
				CreateColorTexture(EditorGUIUtility.isProSkin ? new Color32(64, 64, 64, 255) : new Color32(192, 192, 192, 255));

			_selectedElemStyle = new(_elemStyle);
			_selectedElemStyle.active.background = _selectedElemStyle.normal.background =
				CreateColorTexture(EditorGUIUtility.isProSkin ? new Color32(88, 88, 88, 255) : new Color32(240, 240, 240, 255));
		}

		private static void TrySet<T>(ref T reference, T newValue) {
			if (!reference.Equals(newValue)) {
				reference = newValue;
				EditorBuild.Save();
			}
		}

		private static Vector2 _scrollPos;
		private static int _selIndex;

		private void OnEnable() {
			minSize = _size;
			maxSize = _size;
			_selIndex = 0;

			// hot reload support
			try {
				SetupStyles();
			} catch { }
		}

		private void OnGUI() {
			// hot reload support
			if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer) {
				Close();
				return;
			}
			if (_scrollBackgroundStyle == null) {
				SetupStyles();
			}

			EditorGUILayout.LabelField("Editor Platform", EditorStyles.boldLabel);

			TrySet(ref EditorBuild.defaultBuildTargetGroup,
				(BuildTargetGroup)EditorGUILayout.EnumPopup("Group", EditorBuild.defaultBuildTargetGroup));
			TrySet(ref EditorBuild.defaultBuildTarget,
				(BuildTarget)EditorGUILayout.EnumPopup("Target", EditorBuild.defaultBuildTarget));

			EditorGUILayout.Space(12f);
			EditorGUILayout.LabelField("Client Build Number", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(string.Concat("Windows: <b>", BuildInfo.Instance.winClientBuildNumber, "</b>"), _richTextLabel);
			EditorGUILayout.LabelField(string.Concat("Linux: <b>", BuildInfo.Instance.linuxClientBuildNumber, "</b>"), _richTextLabel);
			EditorGUILayout.LabelField(string.Concat("Android: <b>", BuildInfo.Instance.androidClientBuildNumber, "</b>"), _richTextLabel);

			EditorGUILayout.Space(12f);
			EditorGUILayout.LabelField("Server", EditorStyles.boldLabel);
			TrySet(ref EditorBuild.serverRunArguments,
				EditorGUILayout.TextField("Run Arguments", EditorBuild.serverRunArguments));

			EditorGUILayout.Space(12f);
			EditorGUILayout.LabelField("Directories To Exclude");
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, _scrollBackgroundStyle);
			{
				EditorGUILayout.Space(2f);
				for (int i = 0; i < BuildStripper.clientDirs.Count; i++) {
					if (GUILayout.Button(string.Concat('"', BuildStripper.clientDirs[i], '"'), _selIndex == i ? _selectedElemStyle : _elemStyle)) {
						_selIndex = i;
					}
					EditorGUILayout.Space(2f);
				}
			}
			EditorGUILayout.EndScrollView();

			// buttons
			_ = EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				GUILayoutOption buttonMinWidth = GUILayout.MinWidth(96f);
				if (GUILayout.Button("Add", buttonMinWidth)) {
					// add
					string path = EditorUtility.OpenFolderPanel("Directory", "Assets", string.Empty);
					if (path.Contains("/Assets/")) {
						path = path.Split("/Assets/")[1];
						if (!BuildStripper.clientDirs.Contains(path)) {
							BuildStripper.clientDirs.Add(path);
							EditorBuild.Save();
						}
					} else if (!string.IsNullOrEmpty(path)) {
						Debug.LogError(EditorBuild.GetTaggedText("Invalid path"));
					}
				}
				if (GUILayout.Button("Remove", buttonMinWidth) && BuildStripper.clientDirs.Count > _selIndex) {
					// remove
					BuildStripper.clientDirs.RemoveAt(_selIndex);
					EditorBuild.Save();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}
#endif
