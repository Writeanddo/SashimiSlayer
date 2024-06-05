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
        BeatmapConfigSo beatmapConfig = BeatmapEditorWindow.CurrentEditingBeatmap;

        if (beatmapConfig != null)
        {
            clip.start = Math.Max(clip.start, beatmapConfig.StartTime);
            SnapClipToBPM(clip, beatmapConfig);
        }

        var clipAsset = clip.asset as BnHActionClip;
        if (clipAsset == null)
        {
            return;
        }

        clipAsset.Template.ActionData.ActionStartTime = clip.start - beatmapConfig.StartTime;
        clipAsset.Template.ActionData.ActionEndTime = clip.end - beatmapConfig.StartTime;
        clipAsset.Template.ActionData.ActionBeatLength = clip.duration * beatmapConfig.Bpm / 60;
    }

    private void SnapClipToBPM(TimelineClip clip, BeatmapConfigSo beatmapConfig)
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

        DrawInteractions(actionClip, region);

        base.DrawBackground(clip, region);
    }

    private void DrawInteractions(BnHActionClip actionClip, ClipBackgroundRegion region)
    {
        foreach (BnHActionCore.InteractionInstanceConfig interaction in actionClip.Template.ActionData.Interactions)
        {
            double interactionStartTime = interaction.BeatsUntilStart * 60 /
                                          BeatmapEditorWindow.CurrentEditingBeatmap.Bpm;

            float normalizedPos = Mathf.InverseLerp((float)region.startTime, (float)region.endTime,
                (float)interactionStartTime);

            // Add 0.01f for precision errors(?)
            if (interactionStartTime >= region.startTime && interactionStartTime <= region.endTime + 0.01f)
            {
                if (interaction.InteractionType == BnHActionCore.InteractionType.IncomingAttack)
                {
                    DrawAttackInteraction(interaction, region, normalizedPos);
                }
                else if (interaction.InteractionType == BnHActionCore.InteractionType.Vulnerable)
                {
                    DrawVulnerableInteraction(region, normalizedPos);
                }
            }
        }
    }

    private void DrawAttackInteraction(
        BnHActionCore.InteractionInstanceConfig interaction,
        ClipBackgroundRegion region,
        float normalizedPos)
    {
        int posePositionCount = Enum.GetValues(typeof(SharedTypes.BlockPoseStates)).Length;

        // Draw a vertical line at the time of the attack
        Rect linePos = region.position;
        linePos.x += normalizedPos * linePos.width;
        linePos.width = 2;

        EditorGUI.DrawRect(linePos, Color.red);

        // Draw a series of lines to indicate the block poses
        linePos.height = 5;
        linePos.width = 5;
        linePos.x -= 2;

        var pose = (int)interaction.BlockPose;

        var vertMargin = 5;
        linePos.y = vertMargin;

        var poseChecker = 1;
        float heightOffsetPerTick = (region.position.height - vertMargin * 2) / (posePositionCount - 1);

        for (var i = 0; i < posePositionCount; i++)
        {
            if ((pose & poseChecker) != 0)
            {
                EditorGUI.DrawRect(linePos, Color.cyan);
            }
            else
            {
                EditorGUI.DrawRect(linePos, Color.gray);
            }

            linePos.y += heightOffsetPerTick;
            poseChecker <<= 1;
        }
    }

    private void DrawVulnerableInteraction(ClipBackgroundRegion region, float normalizedPos)
    {
        Rect linePos = region.position;
        linePos.x += normalizedPos * linePos.width;
        linePos.width = 2;
        linePos.x = Mathf.Clamp(linePos.x, region.position.x, region.position.x + region.position.width - 2);

        EditorGUI.DrawRect(linePos, Color.yellow);

        Rect crossPos = linePos;
        crossPos.width = 5;
        crossPos.x -= 2;
        crossPos.y = linePos.height / 2 - 2;
        crossPos.height = 5;
        EditorGUI.DrawRect(crossPos, Color.yellow);
    }

    private void DrawBeatGuides(TimelineClip clip, ClipBackgroundRegion region)
    {
        // region.startTime is the time of the visible area RELATIVE to the start of the ENTIRE clip
        double drawnAreaStartTime = clip.start + region.startTime;
        double drawnAreaEndTime = clip.start + region.endTime;

        BeatmapConfigSo currentBeatmap = BeatmapEditorWindow.CurrentEditingBeatmap;

        // Draw lines on every measure
        var subdivisionInterval = (float)(60 /
                                          currentBeatmap.Bpm /
                                          currentBeatmap.Subdivisions);

        int subdivsPerMeasure = currentBeatmap.BeatsPerMeasure *
                                currentBeatmap.Subdivisions;

        // Find the first subdivision, and snap it to the nearest measure
        var startSubdiv = (int)Math.Ceiling(drawnAreaStartTime / subdivisionInterval);

        var endSubdiv = (int)Math.Floor(drawnAreaEndTime / subdivisionInterval);

        double startOffset = currentBeatmap.StartTime % (subdivisionInterval * subdivsPerMeasure);

        for (int i = startSubdiv; i <= endSubdiv; i++)
        {
            var beatTime = (float)(i * subdivisionInterval + startOffset - clip.start);
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