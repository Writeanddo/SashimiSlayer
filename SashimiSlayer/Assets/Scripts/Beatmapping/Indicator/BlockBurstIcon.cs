using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Simple monobehaviour for block icon burst animation
    /// </summary>
    public class BlockBurstIcon : IndicatorVisual
    {
        [SerializeField]
        private SharedTypes.BlockPoseStates _pose;

        [SerializeField]
        private List<SpriteRenderer> _blockIcons;

        [SerializeField]
        private float _poseBurstDuration;

        [SerializeField]
        private float _poseBurstScale;

        public override void OnBlockPose(SharedTypes.BlockPoseStates pose)
        {
            bool isMatch = pose == _pose;
            foreach (SpriteRenderer burstSprite in _blockIcons)
            {
                if (isMatch)
                {
                    burstSprite.enabled = true;
                    burstSprite.transform.localScale = Vector3.one;
                    burstSprite.color = new Color(1, 1, 1, 1);
                    burstSprite.DOFade(0, _poseBurstDuration);
                    burstSprite.transform.DOScale(_poseBurstScale, _poseBurstDuration);
                }
                else
                {
                    burstSprite.enabled = false;
                }
            }
        }
    }
}