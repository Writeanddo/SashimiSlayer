using System;
using Beatmapping;
using Beatmapping.Interactions;
using EditorUtils.BoldHeader;
using Events.Core;
using FMODUnity;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Core.Audio
{
    [Serializable]
    public struct PitchShiftValues
    {
        [Range(0.5f, 2f)]
        public float SlicePitchShift;

        [Range(0.5f, 2f)]
        public float StarBlockPitchShift;

        [Range(0.5f, 2f)]
        public float ShellBlockPitchShift;
    }

    /// <summary>
    ///     Script that handles setting global FMOD params
    /// </summary>
    public class FmodParamManager : MonoBehaviour
    {
        [BoldHeader("FMOD Param Manager")]
        [InfoBox("Handles setting various global FMOD params (e.g slice timing, streak)")]
        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        [SerializeField]
        private TMP_Text _streakDebugText;

        private int _successfulStreak;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
            _sliceResultEvent.AddListener(OnSliceResult);
            _beatmapLoadedEvent.AddListener(OnBeatmapLoaded);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
            _sliceResultEvent.RemoveListener(OnSliceResult);
            _beatmapLoadedEvent.RemoveListener(OnBeatmapLoaded);
        }

        private void OnBeatmapLoaded(BeatmapConfigSo beatmap)
        {
            PitchShiftValues pitchShiftValues = beatmap.PitchShiftValues;
            FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
            studioSystem.setParameterByName("Pitching/SlicePitchShift", pitchShiftValues.SlicePitchShift, true);
            studioSystem.setParameterByName("Pitching/StarBlockPitchShift", pitchShiftValues.StarBlockPitchShift, true);
            studioSystem.setParameterByName("Pitching/ShellBlockPitchShift", pitchShiftValues.ShellBlockPitchShift,
                true);
        }

        private void OnNoteInteractionFinalResult(NoteInteraction.FinalResult result)
        {
            if (result.Successful)
            {
                _successfulStreak++;

                var timingParamVal = 0;
                switch (result.TimingResult.Score)
                {
                    case TimingWindow.Score.Pass:
                        if (result.TimingResult.Direction == TimingWindow.Direction.Early)
                        {
                            timingParamVal = 0;
                        }
                        else if (result.TimingResult.Direction == TimingWindow.Direction.Late)
                        {
                            timingParamVal = 2;
                        }

                        break;
                    case TimingWindow.Score.Perfect:
                        timingParamVal = 1;
                        break;
                }

                RuntimeManager.StudioSystem.setParameterByName("InteractionSuccessTiming", timingParamVal,
                    true);
            }
            else
            {
                _successfulStreak = 0;
            }

            _streakDebugText.text = $"{_successfulStreak}";

            RuntimeManager.StudioSystem.setParameterByName("SuccessStreak", _successfulStreak, true);
        }

        private void OnSliceResult(SliceResultData data)
        {
            if (data.SliceCount > 0)
            {
                RuntimeManager.StudioSystem.setParameterByName("SliceTargetCount", data.SliceCount, true);
            }
        }
    }
}