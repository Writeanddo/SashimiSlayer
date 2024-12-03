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

            NoteInteraction.InteractionAttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.IncomingAttack,
                protagSwordState.BlockPose);

            // Fails do nothing; failures get handled inside tick logic when the interaction window ends 
            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            Protaganist.Instance.SuccessfulBlock(protagSwordState.BlockPose);

            var finalResult = new SharedTypes.InteractionFinalResult
            {
                Successful = true,
                InteractionType = NoteInteraction.InteractionType.IncomingAttack,
                TimingResult = interactionAttemptResult.TimingResult
            };

            _noteInteractionFinalResultEvent.Raise(finalResult);

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

            NoteInteraction.InteractionAttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.TargetToHit,
                protagSwordState.BlockPose);

            // Fails do nothing; failures get handled inside tick logic when the interaction window ends
            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            // Success!
            OnSlicedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);

            Protaganist.Instance.SuccessfulSlice();

            var finalResult = new SharedTypes.InteractionFinalResult
            {
                Successful = true,
                InteractionType = NoteInteraction.InteractionType.TargetToHit,
                TimingResult = interactionAttemptResult.TimingResult
            };

            _noteInteractionFinalResultEvent.Raise(finalResult);
        }
    }
}