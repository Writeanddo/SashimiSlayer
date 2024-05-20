using System;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(BnHActionClip))]
public class BnHActionClipEditor : ClipEditor
{
    public override void OnClipChanged(TimelineClip clip)
    {
        BeatmapConfigSO beatmapConfig = BeatmapEditorWindow.CurrentEditingBeatmap;

        if (beatmapConfig != null)
        {
            SnapClipToBPM(clip, beatmapConfig);
        }

        var actionClip = clip.asset as BnHActionClip;

        if (actionClip == null)
        {
            return;
        }

        actionClip.template.actionData._beatsUntilVulnerable =
            (int)Math.Round((clip.end - clip.start) / 60 * beatmapConfig.BPM);
    }

    private void SnapClipToBPM(TimelineClip clip, BeatmapConfigSO beatmapConfig)
    {
        double start = clip.start;
        double duration = clip.duration;

        clip.start = QuantizeTime(start, beatmapConfig);
        clip.duration = QuantizeTime(start + duration, beatmapConfig) - clip.start;
    }

    private double QuantizeTime(double time, BeatmapConfigSO beatmapConfig)
    {
        double startTime = beatmapConfig.StartTime;
        double bpm = beatmapConfig.BPM;

        int subdivisions = beatmapConfig.Subdivisions;
        subdivisions = subdivisions == 0 ? 1 : subdivisions;

        double beatDuration = 60 / bpm / subdivisions;

        double beatOffset = beatmapConfig.BeatOffset;

        double beatTime = (time - startTime) / beatDuration;

        double quantizedBeatTime = Math.Round(beatTime) + beatOffset;

        time = startTime + quantizedBeatTime * beatDuration;

        return time;
    }

    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
    {
        var actionClip = clip.asset as BnHActionClip;

        if (actionClip == null)
        {
            return;
        }

        double attackTime = actionClip.template.actionData._beatsUntilAttack * 60 /
                            BeatmapEditorWindow.CurrentEditingBeatmap.BPM;

        float normalizedPos = Mathf.InverseLerp((float)region.startTime, (float)region.endTime, (float)attackTime);

        // draw a line at the attack time
        if (normalizedPos > 0 && normalizedPos < 1)
        {
            Rect linePos = region.position;
            linePos.x += normalizedPos * linePos.width;
            linePos.width = 3;

            EditorGUI.DrawRect(linePos, Color.yellow);
        }

        // Draw lines on every measure
        var beatDuration = (float)(60 / BeatmapEditorWindow.CurrentEditingBeatmap.BPM);
        int beatsPerMeasure = BeatmapEditorWindow.CurrentEditingBeatmap.BeatsPerMeasure;

        var startBeat = (int)Math.Round(clip.start / beatDuration);
        startBeat = -startBeat % beatsPerMeasure;
        var endBeat = (int)Math.Round(clip.end / beatDuration);

        for (int i = startBeat; i <= endBeat; i += beatsPerMeasure)
        {
            float beatTime = i * beatDuration;
            float normalizedBeatTime = Mathf.InverseLerp((float)region.startTime, (float)region.endTime, beatTime);

            Rect linePos = region.position;
            linePos.x += normalizedBeatTime * linePos.width;
            linePos.width = 1;

            EditorGUI.DrawRect(linePos, Color.white);
        }

        base.DrawBackground(clip, region);
    }
}