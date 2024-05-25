using UnityEngine;

[CreateAssetMenu(fileName = "BnHConfig", menuName = "BeatAction/BnHAction")]
public class BnHActionSo : ScriptableObject
{
    [field: SerializeField]
    public BnHActionCore Prefab { get; private set; }

    [field: Header("Timing")]

    [field: SerializeField]
    public double BlockWindowHalfDuration { get; private set; }

    [field: SerializeField]
    public double VulnerableWindowHalfDuration { get; private set; }

    [field: SerializeField]
    [Tooltip("The time before the block window starts where htting a block will count as a miss (prevent spamming)")]
    public double BlockWindowFailDuration { get; private set; }

    [field: Header("Stats")]

    [field: SerializeField]
    public float DamageDealtToPlayer { get; private set; }

    [field: SerializeField]
    public float HitboxRadius { get; private set; }
}