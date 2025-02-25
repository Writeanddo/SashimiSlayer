using UnityEngine;

namespace Events.Core
{
    public struct SliceResultData
    {
        /// <summary>
        ///     Number of notes that were sliced.
        /// </summary>
        public int SliceCount;
    }

    /// <summary>
    ///     Event for when a slice occurs during gameplay.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Core/SliceResultEvent")]
    public class SliceResultEvent : SOEvent<SliceResultData>
    {
    }
}