using UnityEngine;

namespace Events.Core
{
    [CreateAssetMenu(menuName = "Events/Core/BeatInteractionResultEvent")]
    public class BeatInteractionResultEvent : SOEvent<BeatActionService.BeatInteractionResult>
    {
    }
}