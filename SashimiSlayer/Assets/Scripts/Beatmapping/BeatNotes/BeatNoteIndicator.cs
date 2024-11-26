using Beatmapping.Notes;
using DG.Tweening;
using Events.Core;
using UnityEngine;

namespace Beatmapping.BeatNotes
{
    public class BeatNoteIndicator : MonoBehaviour
    {
        private enum UpdateType
        {
            Smooth,
            Beat,
            Subdiv
        }

        [Header("Events Listening")]

        [SerializeField]
        private ProtagSwordStateEvent _protagSwordStateEvent;

        [Header("Visuals")]

        [SerializeField]
        private UpdateType _updateType;

        [SerializeField]
        private SpriteRenderer[] _blockPoseSprites;

        [SerializeField]
        private float _poseBurstDuration;

        [SerializeField]
        private float _poseBurstScale;

        [SerializeField]
        private SpriteRenderer[] _blockPoseBurstSprites;

        [SerializeField]
        private GameObject[] _vulnerableRotate;

        [SerializeField]
        private AnimationClip _attackClip;

        [SerializeField]
        private AnimationClip _vulnClip;

        [SerializeField]
        private SimpleAnimator _animator;

        private BeatNote _beatNote;

        private void Awake()
        {
            _beatNote = GetComponentInParent<BeatNote>();

            _protagSwordStateEvent.AddListener(OnProtagSwordState);
            _beatNote.OnNoteEnded += HandleNoteEnded;
            _beatNote.OnTickWaitingForVulnerable += HandleTickWaitingForVuln;
            _beatNote.OnTickWaitingForAttack += HandleTickWaitingForAttack;
            _beatNote.OnTransitionToWaitingToAttack += HandleTransitionToWaitingToAttack;
        }

        private void OnDestroy()
        {
            _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
            _beatNote.OnNoteEnded -= HandleNoteEnded;
            _beatNote.OnTickWaitingForVulnerable -= HandleTickWaitingForVuln;
            _beatNote.OnTickWaitingForAttack -= HandleTickWaitingForAttack;
            _beatNote.OnTransitionToWaitingToAttack -= HandleTransitionToWaitingToAttack;
        }

        private void OnProtagSwordState(Protaganist.ProtagSwordState state)
        {
            foreach (GameObject vulnIndicator in _vulnerableRotate)
            {
                vulnIndicator.transform.rotation = Quaternion.AngleAxis(state.SwordAngle, Vector3.forward);
            }
        }

        private void HandleTickWaitingForAttack(BeatNote.NoteTiming noteTiming,
            NoteInteraction noteInteraction)
        {
            _animator.Play(_attackClip);
            _animator.SetNormalizedTime(GetTime(noteTiming));
        }

        private void HandleTickWaitingForVuln(BeatNote.NoteTiming noteTiming,
            NoteInteraction noteInteraction)
        {
            _animator.Play(_vulnClip);
            _animator.SetNormalizedTime(GetTime(noteTiming));
        }

        private float GetTime(BeatNote.NoteTiming noteTiming)
        {
            switch (_updateType)
            {
                case UpdateType.Smooth:
                    return (float)noteTiming.NormalizedInteractionWaitTime;
                case UpdateType.Beat:
                    return (float)noteTiming.BeatSteppedNormalizedInteractionWaitTime;
                case UpdateType.Subdiv:
                    return (float)noteTiming.SubdivSteppedNormalizedInteractionWaitTime;
                default:
                    return (float)noteTiming.NormalizedInteractionWaitTime;
            }
        }

        private void HandleNoteEnded(BeatNote.NoteTiming noteTiming)
        {
            _animator.Stop();
        }

        private void HandleTransitionToWaitingToAttack(BeatNote.NoteTiming noteTiming,
            NoteInteraction noteInteraction)
        {
            var blockPose = (int)noteInteraction.BlockPose;

            for (var i = 0; i < _blockPoseSprites.Length; i++)
            {
                bool includesPose = blockPose.IsIndexInFlag(i);
                _blockPoseSprites[i].gameObject.SetActive(includesPose);

                SpriteRenderer burstSprite = _blockPoseBurstSprites[i];

                if (includesPose)
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