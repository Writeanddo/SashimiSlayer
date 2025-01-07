using System.Collections.Generic;
using Beatmapping.Indicator;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using DG.Tweening;
using Events.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.BeatNotes
{
    public class BeatNoteIndicator : BeatNoteListener
    {
        [Header("Events Listening")]

        [SerializeField]
        private ProtagSwordStateEvent _protagSwordStateEvent;

        [SerializeField]
        private List<IndicatorVisual> _indicatorVisuals;

        [Header("Visuals")]

        [SerializeField]
        private UpdateType _updateType;

        [SerializeField]
        private SpriteRenderer _blockRing;

        [SerializeField]
        private GameObject[] _vulnerableRotate;

        [SerializeField]
        private AnimationClip _attackClip;

        [SerializeField]
        private AnimationClip _vulnClip;

        [SerializeField]
        private SimpleAnimator _animator;

        [SerializeField]
        private float _animationNormalizedTimeOffset;

        public UnityEvent<SharedTypes.BlockPoseStates> OnBlockPose;

        private BeatNote _beatNote;

        private NoteInteraction.InteractionType _interactionType = (NoteInteraction.InteractionType)(-1);

        private void Start()
        {
            _protagSwordStateEvent.AddListener(OnProtagSwordState);
        }

        private void OnDestroy()
        {
            _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
        }

        [Button("Detect Visuals")]
        private void DetectVisuals()
        {
            _indicatorVisuals = new List<IndicatorVisual>(GetComponentsInChildren<IndicatorVisual>(true));
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.TimeSegmentType segmentType = tickinfo.NoteSegment.Type;

            if (segmentType != BeatNote.TimeSegmentType.Interaction)
            {
                _animator.Stop();
                _interactionType = (NoteInteraction.InteractionType)(-1);
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
            if (_interactionType != NoteInteraction.InteractionType.IncomingAttack)
            {
                _animator.Stop();
                _animator.Play(_attackClip);
                _interactionType = NoteInteraction.InteractionType.IncomingAttack;
            }

            _animator.SetNormalizedTime((float)noteTickInfo.NormalizedSegmentTime + _animationNormalizedTimeOffset);
            _animator.UpdateAnim(0);
        }

        private void TargetToHitIndicator(BeatNote.NoteTickInfo noteTickInfo)
        {
            if (_interactionType != NoteInteraction.InteractionType.TargetToHit)
            {
                _animator.Stop();
                _animator.Play(_vulnClip);
                _interactionType = NoteInteraction.InteractionType.TargetToHit;
            }

            _animator.SetNormalizedTime((float)noteTickInfo.NormalizedSegmentTime + _animationNormalizedTimeOffset);
            _animator.UpdateAnim(0);
        }

        private void UpdateIncomingAttackIndicator(SharedTypes.BlockPoseStates blockPose)
        {
            OnBlockPose.Invoke(blockPose);

            foreach (IndicatorVisual indicatorVisual in _indicatorVisuals)
            {
                indicatorVisual.OnBlockPose(blockPose);
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