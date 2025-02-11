using Beatmapping.Notes;
using UnityEngine;

namespace Events.Core
{
    [CreateAssetMenu(menuName = "Events/Core/NoteInteractionFinalResultEvent")]
    public class NoteInteractionFinalResultEvent : SOEvent<NoteInteraction.FinalResult>
    {
    }
}