using Beatmapping.Interactions;
using UnityEngine;

namespace Events.Core
{
    [CreateAssetMenu(menuName = "Events/Core/NoteInteractionAttemptEvent")]
    public class NoteInteractionAttemptEvent : SOEvent<NoteInteraction.AttemptResult>
    {
    }
}