using Events;
using UnityEngine;

namespace Core.Protag
{
    public class ProtagBody : MonoBehaviour
    {
        [Header("Visuals")]

        [Tooltip("The target transform that Notes move towards")]
        [SerializeField]
        private Transform _targetTransform;

        [SerializeField]
        private Transform _swordPivot;

        [Header("Event Invoking")]

        [SerializeField]
        private Vector2Event _swordPivotPositionChangeEvent;

        public Vector3 TargetPosition => _targetTransform.position;

        private void Awake()
        {
            Protaganist.Instance.NoteTargetPosition = TargetPosition;
        }

        private void Start()
        {
            _swordPivotPositionChangeEvent.Raise(_swordPivot.position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(TargetPosition, 0.25f);
        }
    }
}