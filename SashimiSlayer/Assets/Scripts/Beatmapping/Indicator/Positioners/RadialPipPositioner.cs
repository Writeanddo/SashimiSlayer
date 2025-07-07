using System.Collections.Generic;
using UnityEngine;

namespace Beatmapping.Indicator.Positioners
{
    [CreateAssetMenu(fileName = "RadialPipPositioner")]
    public class RadialPipPositioner : PipPositioner
    {
        [Header("Layout")]

        [SerializeField]
        private float _centerAngle;

        [SerializeField]
        private float _pipIntervalAngle;

        [SerializeField]
        private float _pipRadius;

        [SerializeField]
        private int _pipDirection;

        [SerializeField]
        private Vector2 _centerOffset;

        public override List<Vector2> CalculatePipLocalPositions(int totalPips)
        {
            float startingAngle = _centerAngle - (totalPips - 1) * _pipIntervalAngle / 2f * _pipDirection;

            var positions = new List<Vector2>(totalPips);
            for (var i = 0; i < totalPips; i++)
            {
                Vector2 dir = Quaternion.Euler(
                                  0,
                                  0,
                                  startingAngle + i * _pipIntervalAngle * _pipDirection) *
                              Vector2.up;

                positions.Add(dir * _pipRadius + _centerOffset);
            }

            return positions;
        }
    }
}