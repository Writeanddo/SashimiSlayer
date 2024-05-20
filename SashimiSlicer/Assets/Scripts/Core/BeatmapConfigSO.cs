using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "BeatmapConfig", menuName = "BeatmapConfigSO")]
public class BeatmapConfigSO : ScriptableObject
{
    [field: Header("Song Timing")]

    [field: SerializeField]
    [field: TextArea]
    public string BeatmapName { get; private set; }

    [field: SerializeField]
    public double BPM { get; private set; }

    [field: SerializeField]
    public int Subdivisions { get; private set; }

    [field: SerializeField]
    public double StartTime { get; private set; }

    [field: SerializeField]
    public double BeatOffset { get; private set; }

    [field: SerializeField]
    public int BeatsPerMeasure { get; private set; }

    [field: Header("Beatmap Data")]

    [field: SerializeField]
    public TimelineAsset BeatmapTimeline { get; private set; }

    [field: Header("Gameplay Data")]

    [field: SerializeField]
    public float BossHealth { get; private set; }
}