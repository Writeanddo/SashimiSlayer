using UnityEngine;

namespace Beatmapping.Indicator
{
    public abstract class IndicatorVisual : MonoBehaviour
    {
        public abstract void OnBlockPose(SharedTypes.BlockPoseStates pose);
    }
}