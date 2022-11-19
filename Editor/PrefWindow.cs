using UnityEditor;
using UnityEngine;

public class PrefWindow : EditorWindow {

	[MenuItem("Build/Preferences", priority = 32)]
	private static void ShowWindow() {
		_ = GetWindow<PrefWindow>(false, "Build Preferences");
	}

	public static Texture2D CreateColorTexture(Color color) {
		Texture2D result = new(1, 1, TextureFormat.RGBA32, false);
		result.SetPixel(0, 0, color);
		result.Apply();
		return result;
	}

	private static GUIStyle _scrollBackground;
	private static GUIStyle _elem;
	private static GUIStyle _selectedElem;

	private readonly Vector2 _size = new(512f, 256f);

	private void RefreshValues() {
		_scrollBackground = new(EditorStyles.label);
		Texture2D background = CreateColorTexture(EditorGUIUtility.isProSkin ? new Color32(40, 40, 40, 255) : new Color32(128, 128, 128, 255));
		_scrollBackground.active.background = _scrollBackground.normal.background = background;

		_elem = new(EditorStyles.label) {
			fixedHeight = 24f
		};
		background = CreateColorTexture(EditorGUIUtility.isProSkin ? new Color32(64, 64, 64, 255) : new Color32(192, 192, 192, 255));
		_elem.active.background = _elem.normal.background = background;

		_selectedElem = new(_elem);
		background = CreateColorTexture(EditorGUIUtility.isProSkin ? new Color32(88, 88, 88, 255) : new Color32(240, 240, 240, 255));
		_selectedElem.active.background = _selectedElem.normal.background = background;
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
	}

	private void OnGUI() {
		// rebuild support
		if (_scrollBackground == null) {
			RefreshValues();
		}

		EditorGUILayout.LabelField("Editor Platform", EditorStyles.boldLabel);

		TrySet(ref EditorBuild.defaultBuildTargetGroup,
			(BuildTargetGroup)EditorGUILayout.EnumPopup("Group", EditorBuild.defaultBuildTargetGroup));
		TrySet(ref EditorBuild.defaultBuildTarget,
			(BuildTarget)EditorGUILayout.EnumPopup("Target", EditorBuild.defaultBuildTarget));

		EditorGUILayout.Space(12f);
		EditorGUILayout.LabelField("Directories To Exclude (Server Builds)", EditorStyles.boldLabel);
		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, _scrollBackground);
		EditorGUILayout.Space(2f);
		for (int i = 0; i < BuildStripper.dirsToExclude.Count; i++) {
			if (GUILayout.Button(string.Concat("'", BuildStripper.dirsToExclude[i], "'"), _selIndex == i ? _selectedElem : _elem)) {
				_selIndex = i;
			}
			EditorGUILayout.Space(2f);
		}
		EditorGUILayout.EndScrollView();
		_ = EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayoutOption buttonMinWidth = GUILayout.MinWidth(96f);
		if (GUILayout.Button("Add", buttonMinWidth)) {
			string path = EditorUtility.OpenFolderPanel("Directory", "Assets", string.Empty);
			if (path.Contains("/Assets/")) {
				path = path.Split("/Assets/")[1];
				if (!BuildStripper.dirsToExclude.Contains(path)) {
					BuildStripper.dirsToExclude.Add(path);
					EditorBuild.Save();
				}
			} else if (!string.IsNullOrEmpty(path)) {
				Debug.LogError(string.Concat('[', nameof(PrefWindow), "->Add] Invalid path"));
			}
		}
		if (GUILayout.Button("Remove", buttonMinWidth) && BuildStripper.dirsToExclude.Count > _selIndex) {
			BuildStripper.dirsToExclude.RemoveAt(_selIndex);
			EditorBuild.Save();
		}
		EditorGUILayout.EndHorizontal();
	}
}
