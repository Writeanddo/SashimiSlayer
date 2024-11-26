using UnityEngine;

namespace Beatmapping.Notes
{
    [CreateAssetMenu(fileName = "BeatNoteType", menuName = "BeatMapping/BeatNoteType")]
    public class BeatNoteTypeSO : ScriptableObject
    {
        [field: SerializeField]
        public int DamageDealtToPlayer { get; private set; }

        [field: SerializeField]
        public float HitboxRadius { get; private set; }

        [field: SerializeField]
        public BeatNote Prefab { get; private set; }
    }
}