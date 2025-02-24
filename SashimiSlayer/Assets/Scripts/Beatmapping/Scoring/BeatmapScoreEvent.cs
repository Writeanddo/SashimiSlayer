using Events;
using UnityEngine;

namespace Beatmapping.Scoring
{
    [CreateAssetMenu(fileName = "BeatmapScoreEvent", menuName = "Events/BeatmapScoreEvent")]
    public class BeatmapScoreEvent : SOEvent<ScoringService.BeatmapScore>
    {
    }
}