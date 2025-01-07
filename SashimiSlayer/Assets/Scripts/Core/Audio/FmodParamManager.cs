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

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
        }

        private void OnNoteInteractionFinalResult(SharedTypes.InteractionFinalResult result)
        {
            float val = result.Successful ? 1 : 0;
            RuntimeManager.StudioSystem.setParameterByName("PreviousNoteStatus", val, true);
        }
    }
}