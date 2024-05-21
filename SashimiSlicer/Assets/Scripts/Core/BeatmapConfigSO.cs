using System;
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

    /// <summary>
    ///     Take a time and snap it to the nearest subdivision
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public double QuantizeTime(double time)
    {
        double startTime = StartTime;
        double bpm = BPM;

        int subdivisions = Subdivisions;
        subdivisions = subdivisions == 0 ? 1 : subdivisions;

        double beatDuration = 60 / bpm / subdivisions;

        double beatOffset = BeatOffset;

        double beatTime = (time - startTime) / beatDuration;

        double quantizedBeatTime = Math.Round(beatTime) + beatOffset;

        time = startTime + quantizedBeatTime * beatDuration;

        return time;
    }
}