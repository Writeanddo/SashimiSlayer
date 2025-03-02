using Base;
using Events.Core;
using Feel;
using UnityEngine;

namespace Core.Protag
{
    /// <summary>
    ///     Handles playing VFX on top of targets that are sliced
    /// </summary>
    public class SliceTargetVFX : DescMono
    {
        [SerializeField]
        private ObjectSlicedEvent _objectSlicedEvent;

        [SerializeField]
        private SwordIndicator _swordIndicator;

        [SerializeField]
        private OneshotVFX _sliceVFX;

        private void Awake()
        {
            _objectSlicedEvent.AddListener(OnObjectSliced);
        }

        private void OnDestroy()
        {
            _objectSlicedEvent.RemoveListener(OnObjectSliced);
        }

        private void OnObjectSliced(ObjectSlicedData data)
        {
            OneshotVFX.PlayVFX(_sliceVFX, data.Position, Quaternion.Euler(0, 0, _swordIndicator.Angle));
        }
    }
}