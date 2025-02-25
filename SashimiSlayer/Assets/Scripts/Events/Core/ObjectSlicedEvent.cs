using UnityEngine;

namespace Events.Core
{
    public struct ObjectSlicedData
    {
        public Vector3 Position;
    }

    /// <summary>
    ///     Event for when an arbitrary object is sliced. Not limited to just notes
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Core/ObjectSlicedEvent")]
    public class ObjectSlicedEvent : SOEvent<ObjectSlicedData>
    {
    }
}