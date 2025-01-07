using System.Collections.Generic;
using Beatmapping.Notes;

namespace Beatmapping.Tooling
{
    public interface IInteractionUser
    {
        public IEnumerable<InteractionUsage> GetInteractionUsages();

        /// <summary>
        ///     What interactions and positions does this listener use?
        ///     This lets specific interaction "outputs" declare what interactions, types, and positions it needs to function
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
}