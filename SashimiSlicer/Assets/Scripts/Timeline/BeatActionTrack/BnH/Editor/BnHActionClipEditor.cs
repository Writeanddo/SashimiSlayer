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

        clip.start = beatmapConfig.QuantizeTime(start);
        clip.duration = beatmapConfig.QuantizeTime(start + duration) - clip.start;
    }

    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
    {
        var actionClip = clip.asset as BnHActionClip;

        if (actionClip == null)
        {
            return;
        }

        // We assume clip.Start time is properly snapped to the interval

        DrawBeatGuides(clip, region);

        DrawAttackPoints(actionClip, region);

        base.DrawBackground(clip, region);
    }

    private void DrawAttackPoints(BnHActionClip actionClip, ClipBackgroundRegion region)
    {
        foreach (BaseBnHAction.AttackInstance attack in actionClip.template.actionData._attacks)
        {
            double attackTime = attack._beatsUntilAttack * 60 /
                                BeatmapEditorWindow.CurrentEditingBeatmap.BPM;

            float normalizedPos = Mathf.InverseLerp((float)region.startTime, (float)region.endTime, (float)attackTime);

            // draw a line at the attack time
            if (normalizedPos > 0 && normalizedPos < 1)
            {
                Rect linePos = region.position;
                linePos.x += normalizedPos * linePos.width;
                linePos.width = 3;
                linePos.height /= 2;

                linePos.x -= linePos.width / 2 - 1;

                EditorGUI.DrawRect(linePos, Color.red);
            }
        }
    }

    private void DrawBeatGuides(TimelineClip clip, ClipBackgroundRegion region)
    {
        // region.startTime is the time of the visible area RELATIVE to the start of the ENTIRE clip
        double drawnAreaStartTime = clip.start + region.startTime;
        double drawnAreaEndTime = clip.start + region.endTime;

        BeatmapConfigSO currentBeatmap = BeatmapEditorWindow.CurrentEditingBeatmap;

        // Draw lines on every measure
        var subdivisionInterval = (float)(60 /
                                          currentBeatmap.BPM /
                                          currentBeatmap.Subdivisions);
        int subdivsPerMeasure = currentBeatmap.BeatsPerMeasure *
                                currentBeatmap.Subdivisions;

        // Find the first subdivision, and snap it to the nearest measure
        var startSubdiv = (int)Math.Ceiling(drawnAreaStartTime / subdivisionInterval);

        var endSubdiv = (int)Math.Floor(drawnAreaEndTime / subdivisionInterval);

        for (int i = startSubdiv; i <= endSubdiv; i++)
        {
            var beatTime = (float)(i * subdivisionInterval + currentBeatmap.StartTime - clip.start);
            float normalizedBeatTime = Mathf.InverseLerp((float)region.startTime, (float)region.endTime, beatTime);

            Rect linePos = region.position;
            linePos.x += normalizedBeatTime * linePos.width;

            linePos.width = 1;
            if (i % subdivsPerMeasure == 0)
            {
                // Start of a measure
                EditorGUI.DrawRect(linePos, Color.white);
            }
            else if (i % currentBeatmap.Subdivisions == 0)
            {
                // Start of a beat
                linePos.height /= 2;
                EditorGUI.DrawRect(linePos, Color.white);
            }
            else
            {
                // Start of a subdiv
                linePos.height = 2;
                EditorGUI.DrawRect(linePos, Color.gray);
            }
        }
    }
}