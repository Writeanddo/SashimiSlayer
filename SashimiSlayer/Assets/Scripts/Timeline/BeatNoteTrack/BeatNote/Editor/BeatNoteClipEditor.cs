using System;
using System.Collections.Generic;
using System.Linq;
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

            BeatNoteBehavior template = clipAsset.Template;

            // Update the internal note data to match the clip
            template.NoteData.NoteStartTime = clip.start - beatmapConfig.StartTime;
            template.NoteData.NoteEndTime = clip.end - beatmapConfig.StartTime;
            template.NoteData.NoteBeatCount = clip.duration * beatmapConfig.Bpm / 60;

            EnforceInteractionUsage(template);
        }

        private void EnforceInteractionUsage(BeatNoteBehavior template)
        {
            List<IInteractionUser.InteractionUsage> interactionUsages =
                template.NoteConfig.Prefab.GetInteractionUsages().ToList();

            List<SequencedNoteInteraction> sequencedInteractions = template.NoteData.Interactions;

            int requiredNumInteractions =
                interactionUsages.Count > 0 ? interactionUsages.Max(a => a.InteractionIndex) + 1 : 0;

            // Add interactions if needed
            if (sequencedInteractions.Count < requiredNumInteractions)
            {
                sequencedInteractions.AddRange(Enumerable.Repeat(new SequencedNoteInteraction(),
                    requiredNumInteractions - sequencedInteractions.Count));
            }

            // Validate each interaction
            foreach (IInteractionUser.InteractionUsage usage in interactionUsages)
            {
                int interactionIndex = usage.InteractionIndex;

                SequencedNoteInteraction sequencedInteraction = sequencedInteractions[interactionIndex];
                NoteInteractionData interactionData = sequencedInteraction.InteractionData;

                interactionData.InteractionType = usage.InteractionType;

                int numPositions = usage.PositionCount;

                if (interactionData.Positions == null)
                {
                    interactionData.Positions = new List<Vector2>();
                }

                // Add or Remove positions if needed
                if (interactionData.Positions.Count < numPositions)
                {
                    interactionData.Positions.AddRange(
                        Enumerable.Repeat(Vector2.zero, numPositions - interactionData.Positions.Count));
                }
                else if (interactionData.Positions.Count > numPositions)
                {
                    interactionData.Positions.RemoveRange(numPositions, interactionData.Positions.Count - numPositions);
                }

                // Kinda awkward to copy around structs...
                sequencedInteraction.InteractionData = interactionData;
                sequencedInteractions[interactionIndex] = sequencedInteraction;
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
            double noteBeatLength = noteClip.Template.NoteData.NoteBeatCount;
            foreach (SequencedNoteInteraction sequencedInteraction in noteClip.Template.NoteData.Interactions)
            {
                double interactionStartTime =
                    sequencedInteraction.GetBeatsFromNoteStart(noteBeatLength) * 60 / beatmap.Bpm;

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

            float heightOffsetPerTick = (region.position.height - vertMargin * 2) / (posePositionCount - 1);

            for (var i = 0; i < posePositionCount; i++)
            {
                if (i == pose)
                {
                    EditorGUI.DrawRect(linePos, Color.cyan);
                }
                else
                {
                    EditorGUI.DrawRect(linePos, Color.gray);
                }

                linePos.y += heightOffsetPerTick;
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
                float markingClipTime = markingTime - (float)clipStartTime;

                float normalizedBeatTime =
                    Mathf.InverseLerp((float)region.startTime, (float)region.endTime, markingClipTime);

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