using Core.Protag;
using Events;
using UnityEngine;

public class ProtagBody : MonoBehaviour
{
    [Header("Visuals")]

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
        Protaganist.Instance.SpritePosition = TargetPosition;
    }

    private void Start()
    {
        _swordPivotPositionChangeEvent.Raise(_swordPivot.position);
    }
}