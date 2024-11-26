using Beatmapping.Notes;
using UnityEngine;

namespace Events.Core
{
    [CreateAssetMenu(menuName = "Events/Core/NoteInteractionAttemptEvent")]
    public class NoteInteractionAttemptEvent : SOEvent<NoteInteraction.InteractionAttemptResult>
    {
    }
}