using System;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Timeline.BeatNoteTrack.BeatNote.Editor
{
    [CustomTimelineEditor(typeof(BeatNoteClip))]
    public class BeatNoteClipEditor : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip)
        {
            BeatmapConfigSo beatmapConfig = BeatmapEditorWindow.CurrentEditingBeatmap;

            if (beatmapConfig == null)
            {
                return;
            }

            // Clip shouldn't start before the beatmap
            clip.start = Math.Max(clip.start, beatmapConfig.StartTime);

            SnapClipToBPM(clip, beatmapConfig);

            var clipAsset = clip.asset as BeatNoteClip;
            if (clipAsset == null)
            {
                return;
            }

            // Update the internal note data to match the clip
            clipAsset.Template.NoteData.NoteStartTime = clip.start - beatmapConfig.StartTime;
            clipAsset.Template.NoteData.NoteEndTime = clip.end - beatmapConfig.StartTime;
            clipAsset.Template.NoteData.NoteBeatCount = (uint)Math.Round(clip.duration * beatmapConfig.Bpm / 60);

            // Ensure at least one position in the array
            if (clipAsset.Template.NoteData.Positions == null || clipAsset.Template.NoteData.Positions.Length == 0)
            {
                clipAsset.Template.NoteData.Positions = new Vector2[1];
            }
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
            var actionClip = clip.asset as BeatNoteClip;

            if (actionClip == null)
            {
                return;
            }

            BeatmapConfigSo beatmapConfig = BeatmapEditorWindow.CurrentEditingBeatmap;

            if (beatmapConfig == null)
            {
                return;
            }

            // We assume clip.Start time is properly snapped to the interval

            DrawBeatGuides(beatmapConfig, clip, region);

            DrawInteractions(beatmapConfig, actionClip, region);

            base.DrawBackground(clip, region);
        }

        private void DrawInteractions(BeatmapConfigSo beatmap, BeatNoteClip noteClip, ClipBackgroundRegion region)
        {
            foreach (SequencedNoteInteraction sequencedInteraction in noteClip.Template.NoteData.Interactions)
            {
                double interactionStartTime = sequencedInteraction.BeatsFromNoteStart * 60 / beatmap.Bpm;

                float normalizedPos = Mathf.InverseLerp(
                    (float)region.startTime,
                    (float)region.endTime,
                    (float)interactionStartTime);

                NoteInteractionData interactionData = sequencedInteraction.InteractionData;

                // Add 0.01f for precision errors(?)
                if (interactionStartTime >= region.startTime && interactionStartTime <= region.endTime + 0.01f)
                {
                    if (interactionData.InteractionType == NoteInteraction.InteractionType.IncomingAttack)
                    {
                        DrawAttackInteraction(interactionData, region, normalizedPos);
                    }
                    else if (interactionData.InteractionType == NoteInteraction.InteractionType.TargetToHit)
                    {
                        DrawVulnerableInteraction(region, normalizedPos);
                    }
                }
            }
        }

        private void DrawAttackInteraction(
            NoteInteractionData interactionData,
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

            var pose = (int)interactionData.BlockPose;

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

        private void DrawBeatGuides(BeatmapConfigSo beatmap, TimelineClip clip, ClipBackgroundRegion region)
        {
            // region.startTime is the time of the visible area RELATIVE to the start of the ENTIRE clip
            // Calculate times in beatmap timespace
            double clipStartTime = clip.start - beatmap.StartTime;
            double drawnAreaStartTime = clipStartTime + region.startTime;
            double drawnAreaEndTime = clipStartTime + region.endTime;

            // Draw lines on every measure
            var subdivisionInterval = (float)(60 /
                                              beatmap.Bpm /
                                              beatmap.Subdivisions);

            int subdivsPerMeasure = beatmap.BeatsPerMeasure *
                                    beatmap.Subdivisions;

            // Find the enclosing bounds, exclusive
            var startSubdiv = (int)Math.Ceiling(drawnAreaStartTime / subdivisionInterval);
            var endSubdiv = (int)Math.Floor(drawnAreaEndTime / subdivisionInterval);

            for (int i = startSubdiv; i <= endSubdiv; i++)
            {
                // time to place the marking, in beatmap timespace
                float markingTime = i * subdivisionInterval;
                // time to place the marking, drawn region timespace
                float markingRegionTime = markingTime - (float)drawnAreaStartTime;

                float normalizedBeatTime =
                    Mathf.InverseLerp((float)region.startTime, (float)region.endTime, markingRegionTime);

                Rect linePos = region.position;
                linePos.x += normalizedBeatTime * linePos.width;

                linePos.width = 1;
                if (i % subdivsPerMeasure == 0)
                {
                    // Start of a measure
                    EditorGUI.DrawRect(linePos, Color.white);
                }
                else if (i % beatmap.Subdivisions == 0)
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
}