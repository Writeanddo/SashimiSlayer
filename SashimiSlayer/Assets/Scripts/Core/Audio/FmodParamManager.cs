using Beatmapping.Interactions;
using Events.Core;
using FMODUnity;
using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    ///     Script that handles setting global FMOD params
    /// </summary>
    public class FmodParamManager : MonoBehaviour
    {
        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        private int _successfulStreak;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
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

            RuntimeManager.StudioSystem.setParameterByName("SuccessStreak", _successfulStreak, true);
        }
    }
}