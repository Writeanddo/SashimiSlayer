using System.Collections.Generic;
using Beatmapping.Notes;
using DG.Tweening;
using Events.Core;
using UnityEngine;

namespace Beatmapping.BeatNotes
{
    public class BeatNoteIndicator : BeatNoteListener
    {
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

        private void Start()
        {
            _protagSwordStateEvent.AddListener(OnProtagSwordState);
        }

        private void OnDestroy()
        {
            _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.TimeSegmentType segmentType = tickinfo.NoteSegment.Type;

            if (segmentType != BeatNote.TimeSegmentType.Interaction)
            {
                _animator.Stop();
                return;
            }

            NoteInteraction interaction = tickinfo.NoteSegment.Interaction;

            switch (interaction.Type)
            {
                case NoteInteraction.InteractionType.IncomingAttack:
                    UpdateIncomingAttackIndicator(interaction.BlockPose);
                    IncomingAttackIndicator(tickinfo);
                    break;
                case NoteInteraction.InteractionType.TargetToHit:
                    TargetToHitIndicator(tickinfo);
                    break;
                default:
                    Debug.LogError("Invalid interaction type");
                    break;
            }
        }

        private void OnProtagSwordState(Protaganist.ProtagSwordState state)
        {
            foreach (GameObject vulnIndicator in _vulnerableRotate)
            {
                vulnIndicator.transform.rotation = Quaternion.AngleAxis(state.SwordAngle, Vector3.forward);
            }
        }

        private void IncomingAttackIndicator(BeatNote.NoteTickInfo noteTickInfo)
        {
            _animator.Play(_attackClip);
            _animator.SetNormalizedTime((float)noteTickInfo.NormalizedSegmentTime);
            _animator.UpdateAnim(0);
        }

        private void TargetToHitIndicator(BeatNote.NoteTickInfo noteTickInfo)
        {
            _animator.Play(_vulnClip);
            _animator.SetNormalizedTime((float)noteTickInfo.NormalizedSegmentTime);
            _animator.UpdateAnim(0);
        }

        private void UpdateIncomingAttackIndicator(SharedTypes.BlockPoseStates blockPose)
        {
            for (var i = 0; i < _blockPoseSprites.Length; i++)
            {
                bool includesPose = (int)blockPose == i;
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

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _beatNote = GetComponentInParent<BeatNote>();
            _animator.SetManualUpdate(true);
            _beatNote.OnTick += BeatNote_OnTick;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
        }
    }
}