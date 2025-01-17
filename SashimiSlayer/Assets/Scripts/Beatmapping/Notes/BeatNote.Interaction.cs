using UnityEngine;

namespace Beatmapping.Notes
{
    public partial class BeatNote : MonoBehaviour
    {
        /// <summary>
        ///     Handle protag's attempt to block
        /// </summary>
        public void AttemptPlayerBlock(Protaganist.ProtagSwordState protagSwordState)
        {
            double currentBeatmapTime = _noteTickInfo.BeatmapTime;
            NoteInteraction interaction = _noteTickInfo.InsideInteractionWindow;

            if (interaction == null)
            {
                return;
            }

            NoteInteraction.AttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.IncomingAttack,
                protagSwordState.BlockPose);

            // Hitting in the fail window means instant failure
            if (interactionAttemptResult.TimingResult.Score == TimingWindow.Score.Fail)
            {
                var earlyFailResult = new NoteInteraction.FinalResult(
                    interactionAttemptResult.TimingResult,
                    NoteInteraction.InteractionType.IncomingAttack,
                    false
                )
                {
                    Pose = protagSwordState.BlockPose
                };

                _noteInteractionFinalResultEvent.Raise(earlyFailResult);
                OnInteractionFinalResult?.Invoke(_noteTickInfo, earlyFailResult);
                OnProtagFailBlock?.Invoke(_noteTickInfo, earlyFailResult);

                return;
            }

            // No pass means do nothing
            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            // Success!
            Protaganist.Instance.SuccessfulBlock(protagSwordState.BlockPose);

            var finalResult = new NoteInteraction.FinalResult(
                interactionAttemptResult.TimingResult,
                NoteInteraction.InteractionType.IncomingAttack,
                true
            )
            {
                Pose = protagSwordState.BlockPose
            };

            _noteInteractionFinalResultEvent.Raise(finalResult);
            OnInteractionFinalResult?.Invoke(_noteTickInfo, finalResult);

            OnBlockedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);
        }

        /// <summary>
        ///     Handle protag's attempt to attack
        /// </summary>
        /// <param name="protagSwordState"></param>
        public void AttemptPlayerSlice(Protaganist.ProtagSwordState protagSwordState)
        {
            // Check if slice is in hitbox
            Vector3 pos = _hitboxTransform.transform.position;
            float dist = protagSwordState.DistanceToSwordPlane(pos);
            bool isAttackOnTarget = dist < _hitboxRadius;

            if (!isAttackOnTarget)
            {
                return;
            }

            // Call interaction logic
            double currentBeatmapTime = _noteTickInfo.BeatmapTime;
            NoteInteraction interaction = _noteTickInfo.InsideInteractionWindow;

            if (interaction == null)
            {
                return;
            }

            NoteInteraction.AttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.TargetToHit,
                protagSwordState.BlockPose);

            // Hitting in the early lockout window fails immediately
            if (interactionAttemptResult.TimingResult.Score == TimingWindow.Score.Fail)
            {
                var earlyFailResult = new NoteInteraction.FinalResult(interactionAttemptResult.TimingResult,
                    NoteInteraction.InteractionType.TargetToHit,
                    false);

                _noteInteractionFinalResultEvent.Raise(earlyFailResult);
                OnInteractionFinalResult?.Invoke(_noteTickInfo, earlyFailResult);
                OnProtagMissedHit?.Invoke(_noteTickInfo, earlyFailResult);
                return;
            }

            // Other fails simply do nothing
            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            // Success!
            OnSlicedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);

            Protaganist.Instance.SuccessfulSlice();

            var finalResult = new NoteInteraction.FinalResult(interactionAttemptResult.TimingResult,
                NoteInteraction.InteractionType.TargetToHit,
                true);

            _noteInteractionFinalResultEvent.Raise(finalResult);
            OnInteractionFinalResult?.Invoke(_noteTickInfo, finalResult);
        }
    }
}