using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class BeatmapEditorWindow : EditorWindow
{
    private const string PrefsPath = "Assets/Settings/Editor/User/SimpleUtilsPrefs.asset";
    private const string _levelRosterPref = "BeatmapEditorWindow.levelRosterSO";
    private static LevelRosterSO _levelRoster;

    // Caching for mapping timeline to beatmap
    private static TimelineAsset _currentEditingTimeline;
    private static BeatmapConfigSo _currentEditingBeatmap;

    // Convenience property for easily getting the correct beatmap matching the current timeline
    public static BeatmapConfigSo CurrentEditingBeatmap => GetBeatmapFromTimeline(TimelineEditor.inspectedAsset);

    private string _lastEditedScenePath = string.Empty;
    private UtilsPrefs _prefs;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Beatmap Editor", EditorStyles.boldLabel);

        string startupScenePath = _prefs.StartupScenePath;

        if (GUILayout.Button("Play Game"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            _lastEditedScenePath = SceneManager.GetActiveScene().path;
            Debug.Log(_lastEditedScenePath);

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
        _levelRoster = AssetDatabase.LoadAssetAtPath<LevelRosterSO>(
            EditorPrefs.GetString(_levelRosterPref, string.Empty));
    }

    private static BeatmapConfigSo GetBeatmapFromTimeline(TimelineAsset timeline)
    {
        if (timeline == _currentEditingTimeline)
        {
            return _currentEditingBeatmap;
        }

        foreach (GameLevelSO level in _levelRoster.Levels)
        {
            if (level.Beatmap.BeatmapTimeline == timeline)
            {
                _currentEditingTimeline = timeline;
                _currentEditingBeatmap = level.Beatmap;
                return level.Beatmap;
            }
        }

        return null;
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
            Debug.Log("Loading last edited scene");
            EditorSceneManager.OpenScene(_lastEditedScenePath, OpenSceneMode.Single);
        }
        else if (param == PlayModeStateChange.EnteredPlayMode)
        {
        }
    }

    private void DrawBeatmapEditingFields()
    {
        _levelRoster =
            (LevelRosterSO)EditorGUILayout.ObjectField("Level Roster", _levelRoster, typeof(LevelRosterSO),
                false);

        EditorPrefs.SetString(_levelRosterPref,
            _levelRoster ? AssetDatabase.GetAssetPath(_levelRoster) : string.Empty);
    }
}