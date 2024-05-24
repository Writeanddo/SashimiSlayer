using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeatmapEditorWindow : EditorWindow
{
    private const string PrefsPath = "Assets/Settings/Editor/User/SimpleUtilsPrefs.asset";
    private const string _beatmapConfigPref = "BeatmapEditorWindow.beatmapConfigSO";

    public static BeatmapConfigSo CurrentEditingBeatmap { get; private set; }

    private string _lastEditedScenePath = string.Empty;
    private UtilsPrefs _prefs;
    private BeatmapConfigSo _beatmapConfig;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Beatmap Editor", EditorStyles.boldLabel);

        string startupScenePath = _prefs.StartupScenePath;

        if (GUILayout.Button("Play Game"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            _lastEditedScenePath = SceneManager.GetActiveScene().path;

            if (!SceneManager.GetSceneByPath(startupScenePath).isLoaded)
            {
                EditorSceneManager.OpenScene(startupScenePath, OpenSceneMode.Single);
            }

            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Open Persistent Data Path"))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        GUILayout.Label("Current Editing Beatmap");

        DrawBeatmapEditingFields();
    }

    private void OnBecameInvisible()
    {
        EditorApplication.playModeStateChanged -= ModeChanged;
    }

    private void OnBecameVisible()
    {
        _prefs = AssetDatabase.LoadAssetAtPath<UtilsPrefs>(PrefsPath);
        if (_prefs == null)
        {
            // Create directories if needed
            string directoryPath = Path.GetDirectoryName(PrefsPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            _prefs = CreateInstance<UtilsPrefs>();
            AssetDatabase.CreateAsset(_prefs, PrefsPath);
            AssetDatabase.SaveAssets();
        }

        EditorApplication.playModeStateChanged += ModeChanged;

        // Load editing beatmap from prefs
        _beatmapConfig = AssetDatabase.LoadAssetAtPath<BeatmapConfigSo>(
            EditorPrefs.GetString(_beatmapConfigPref, string.Empty));

        CurrentEditingBeatmap = _beatmapConfig;
    }

    [MenuItem("Tools/Beatmap Editor Utils")]
    public static void ShowWindow()
    {
        GetWindow<BeatmapEditorWindow>("Beatmap Editing Utils");
    }

    private void ModeChanged(PlayModeStateChange param)
    {
        if (param == PlayModeStateChange.EnteredEditMode && _lastEditedScenePath != string.Empty)
        {
            EditorSceneManager.OpenScene(_lastEditedScenePath, OpenSceneMode.Single);
        }
        else if (param == PlayModeStateChange.EnteredPlayMode)
        {
        }
    }

    private void DrawBeatmapEditingFields()
    {
        _beatmapConfig =
            (BeatmapConfigSo)EditorGUILayout.ObjectField("Beatmap Config", _beatmapConfig, typeof(BeatmapConfigSo),
                false);

        CurrentEditingBeatmap = _beatmapConfig;
        EditorPrefs.SetString(_beatmapConfigPref,
            _beatmapConfig ? AssetDatabase.GetAssetPath(_beatmapConfig) : string.Empty);
    }
}