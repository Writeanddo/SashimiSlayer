using UnityEditor;
using UnityEngine;

public class BeatmapEditorWindow : EditorWindow
{
    private const string _beatmapConfigPref = "BeatmapEditorWindow.beatmapConfigSO";
    public static BeatmapConfigSo CurrentEditingBeatmap { get; private set; }

    private BeatmapConfigSo _beatmapConfig;

    private void OnGUI()
    {
        GUILayout.Label("Current Editing Beatmap");

        DrawFields();
    }

    private void OnBecameVisible()
    {
        // Load from editor prefs
        _beatmapConfig = AssetDatabase.LoadAssetAtPath<BeatmapConfigSo>(
            EditorPrefs.GetString(_beatmapConfigPref, string.Empty));

        CurrentEditingBeatmap = _beatmapConfig;
    }

    [MenuItem("Tools/Beatmap Helper")]
    public static void ShowWindow()
    {
        GetWindow<BeatmapEditorWindow>("Beatmap Editor Helper");
    }

    private void DrawFields()
    {
        _beatmapConfig =
            (BeatmapConfigSo)EditorGUILayout.ObjectField("Beatmap Config", _beatmapConfig, typeof(BeatmapConfigSo),
                false);

        CurrentEditingBeatmap = _beatmapConfig;
        EditorPrefs.SetString(_beatmapConfigPref,
            _beatmapConfig ? AssetDatabase.GetAssetPath(_beatmapConfig) : string.Empty);

        if (GUILayout.Button("Play") && Application.isPlaying)
        {
            BeatmapService.Instance.StartBeatmap();
        }
    }
}