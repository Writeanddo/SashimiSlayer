using UnityEngine;

public class ProtagBody : MonoBehaviour
{
    [Header("Visuals")]

    [SerializeField]
    private Transform _targetTransform;

    public Vector3 TargetPosition => _targetTransform.position;

    private void Awake()
    {
        Protaganist.Instance.SpritePosition = TargetPosition;
    }
}