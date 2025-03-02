using Base;
using Beatmapping.Interactions;
using Events.Core;
using FMODUnity;
using TMPro;
using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    ///     Script that handles setting global FMOD params
    /// </summary>
    public class FmodParamManager : DescMono
    {
        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

        [SerializeField]
        private TMP_Text _streakDebugText;

        private int _successfulStreak;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
            _sliceResultEvent.AddListener(OnSliceResult);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
            _sliceResultEvent.RemoveListener(OnSliceResult);
        }

        private void OnNoteInteractionFinalResult(NoteInteraction.FinalResult result)
        {
            if (result.Successful)
            {
                _successfulStreak++;
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