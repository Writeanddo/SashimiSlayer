using System.Collections.Generic;
using Beatmapping.Notes;

public interface IInteractionUser
{
    public IEnumerable<InteractionUsage> GetInteractionUsages();

    /// <summary>
    ///     What interactions and positions does this listener use?
    ///     This is used to automatically fill out beat note data in the beatmapping tool, based on the prefab setup
    /// </summary>
    public struct InteractionUsage
    {
        public int InteractionIndex;
        public NoteInteraction.InteractionType InteractionType;
        public int PositionCount;

        public InteractionUsage(NoteInteraction.InteractionType interactionType, int interactionIndex,
            int positionCount)
        {
            InteractionIndex = interactionIndex;
            PositionCount = positionCount;
            InteractionType = interactionType;
        }
    }
}