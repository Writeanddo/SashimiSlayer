using Core.Protag;
using Events.Core;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Script that aligns a visual container to the sword angle of the protaganist. Intended for pip indicators
    /// </summary>
    public class AlignToSwordAngle : MonoBehaviour
    {
        [SerializeField]
        private Transform _visualContainer;

        [Header("Event (In)")]

        [SerializeField]
        private ProtagSwordStateEvent _protagSwordStateEvent;

        private void Awake()
        {
            _protagSwordStateEvent.AddListener(OnProtagSwordState);
        }

        private void OnDestroy()
        {
            _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
        }

        private void OnProtagSwordState(Protaganist.ProtagSwordState state)
        {
            _visualContainer.rotation = Quaternion.AngleAxis(state.SwordAngle, Vector3.forward);
        }
    }
}