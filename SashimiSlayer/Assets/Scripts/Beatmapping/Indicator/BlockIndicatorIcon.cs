using System.Collections.Generic;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Simple monobehaviour to show/hide a block icon depending on the note
    /// </summary>
    public class BlockIndicatorIcon : IndicatorVisual
    {
        [SerializeField]
        private SharedTypes.BlockPoseStates _pose;

        [SerializeField]
        private List<GameObject> _blockIcons;

        public override void OnBlockPose(SharedTypes.BlockPoseStates pose)
        {
            bool isMatch = pose == _pose;
            foreach (GameObject icon in _blockIcons)
            {
                icon.SetActive(isMatch);
            }
        }
    }
}