using UnityEngine;

namespace Beatmapping.Scoring
{
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "BeatMapping/ScoreConfig")]
    public class ScoreConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public int PointsForEarly;

        [field: SerializeField]
        public int PointsForLate;

        [field: SerializeField]
        public int PointsForPerfect;

        [field: SerializeField]
        public int PointsForMiss;
    }
}