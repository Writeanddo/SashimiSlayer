using UnityEngine;

namespace Beatmapping.Timing
{
    [CreateAssetMenu(fileName = "TimingWindow", menuName = "BeatMapping/TimingWindow")]
    public class TimingWindowSO : ScriptableObject
    {
        [field: SerializeField]
        public double PerfectWindowHalfWidth { get; private set; }

        [field: SerializeField]
        public double PassWindowHalfWidth { get; private set; }

        [field: SerializeField]
        public double FailWindowHalfWidth { get; private set; }

        public TimingWindow CreateTimingWindow(double targetTime)
        {
            return new TimingWindow(targetTime,
                new[] { PerfectWindowHalfWidth, PassWindowHalfWidth, FailWindowHalfWidth });
        }
    }
}