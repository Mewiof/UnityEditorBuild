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

	private void OnEnable() {
		try {
			Vector2 size = new(512f, 256f);
			minSize = size;
			maxSize = size;

			_scrollBackground = new(EditorStyles.label);
			Texture2D background = CreateColorTexture(new Color32(44, 44, 44, 255));
			_scrollBackground.active.background = _scrollBackground.normal.background = background;

			_elem = new(EditorStyles.label) {
				fixedHeight = 24f
			};
			background = CreateColorTexture(new Color32(64, 64, 64, 255));
			_elem.active.background = _elem.normal.background = background;

			_selectedElem = new(_elem);
			background = CreateColorTexture(new Color32(88, 88, 88, 255));
			_selectedElem.active.background = _selectedElem.normal.background = background;
		} catch { // rebuild
			Close();
		}
	}

	private static void TrySet<T>(ref T reference, T newValue) {
		if (!reference.Equals(newValue)) {
			reference = newValue;
			EditorBuild.Save();
		}
	}

	private static Vector2 _scrollPos;
	private static int _selIndex;

	private void OnGUI() {
		EditorGUILayout.LabelField("Editor Platform", EditorStyles.boldLabel);

		TrySet(ref EditorBuild.defaultBuildTargetGroup,
			(BuildTargetGroup)EditorGUILayout.EnumPopup("Group", EditorBuild.defaultBuildTargetGroup));
		TrySet(ref EditorBuild.defaultBuildTarget,
			(BuildTarget)EditorGUILayout.EnumPopup("Target", EditorBuild.defaultBuildTarget));

		EditorGUILayout.Space(12f);
		EditorGUILayout.LabelField("Directories To Exclude (Server Builds)", EditorStyles.boldLabel);
		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, _scrollBackground);
		for (int i = 0; i < BuildStripper.dirsToExclude.Count; i++) {
			EditorGUILayout.Space(2f);
			if (GUILayout.Button(string.Concat("'", BuildStripper.dirsToExclude[i], "'"), _selIndex == i ? _selectedElem : _elem)) {
				_selIndex = i;
			}
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
