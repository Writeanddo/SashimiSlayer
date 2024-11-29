using System;

namespace Beatmapping.Notes
{
    /// <summary>
    ///     An instantiated note interaction
    /// </summary>
    public class NoteInteraction
    {
        public enum InteractionType
        {
            /// <summary>
            ///     Interaction where the note will try to hit the protag, and the protag must block
            /// </summary>
            IncomingAttack,

            /// <summary>
            ///     Interaction where the note is vulnerable, and the protag must hit it
            /// </summary>
            TargetToHit
        }

        [Flags]
        public enum InteractionFlags
        {
            /// <summary>
            ///     End the note immediately if this interaction hits the protag
            /// </summary>
            EndNoteOnHittingProtag = 1 << 0,

            /// <summary>
            ///     End the note immediately of this interaction is hit by the protag
            /// </summary>
            EndNoteOnHitByProtag = 1 << 1,

            /// <summary>
            ///     End the note immediately if this interaction is blocked
            /// </summary>
            EndNoteOnBlocked = 1 << 2
        }

        public InteractionType Type { get; }

        public double TargetTime => _timingWindow.TargetTime;

        public bool DidSucceed => _interactionState == NoteInteractionState.Success;

        public InteractionFlags Flags { get; }

        public SharedTypes.BlockPoseStates BlockPose { get; }

        private readonly TimingWindow _timingWindow;

        private NoteInteractionState _interactionState;

        public NoteInteraction(InteractionType typeType,
            InteractionFlags interactionFlags,
            SharedTypes.BlockPoseStates blockPose,
            TimingWindow window)
        {
            Type = typeType;
            Flags = interactionFlags;
            BlockPose = blockPose;
            _timingWindow = window;
            _interactionState = NoteInteractionState.Default;
        }

        /// <summary>
        ///     Attempt an interaction
        /// </summary>
        /// <param name="attemptTime"></param>
        /// <param name="attemptedInteraction">the interaction attempted by the protag</param>
        /// <param name="blockPose">the block pose protag is using, if relevant</param>
        /// <returns></returns>
        public InteractionAttemptResult TryInteraction(
            double attemptTime,
            InteractionType attemptedInteraction,
            SharedTypes.BlockPoseStates blockPose = default)
        {
            // Interaction already succeeded
            if (_interactionState == NoteInteractionState.Success)
            {
                return new InteractionAttemptResult
                {
                    ValidInteraction = false
                };
            }

            // Interaction type doesn't match
            if (attemptedInteraction != Type)
            {
                return new InteractionAttemptResult
                {
                    ValidInteraction = false
                };
            }

            // Block pose doesn't match
            if (Type == InteractionType.IncomingAttack &&
                blockPose != BlockPose)
            {
                return new InteractionAttemptResult
                {
                    ValidInteraction = false
                };
            }

            // Locked out by a previous interaction that landed in the lockout window
            if (_interactionState == NoteInteractionState.LockedOut)
            {
                return new InteractionAttemptResult
                {
                    ValidInteraction = false
                };
            }

            TimingWindow.TimingResult timingResult = _timingWindow.CalculateTimingResult(attemptTime);
            InteractionFlags flags = Flags;

            // Lockout windows are to prevent spamming
            if (timingResult.Score == TimingWindow.Score.FailLockout)
            {
                _interactionState = NoteInteractionState.LockedOut;
            }

            var result = new InteractionAttemptResult
            {
                ValidInteraction = true,
                TimingResult = timingResult,
                Flags = flags
            };

            if (result.Passed)
            {
                _interactionState = NoteInteractionState.Success;
            }

            return result;
        }

        public bool IsTimeInWindow(double time)
        {
            return _timingWindow.CalculateTimingResult(time).Score is TimingWindow.Score.Pass
                or TimingWindow.Score.Perfect;
        }

        public struct InteractionAttemptResult
        {
            /// <summary>
            ///     Was the interaction attempt even valid? (matching type, correct block pose, not locked out)
            /// </summary>
            public bool ValidInteraction;

            /// <summary>
            ///     The result of the timing window check for valid interactions
            /// </summary>
            public TimingWindow.TimingResult TimingResult;

            public InteractionFlags Flags;

            public bool Passed => ValidInteraction &&
                                  TimingResult.Score is TimingWindow.Score.Pass or TimingWindow.Score.Perfect;
        }

        private enum NoteInteractionState
        {
            Default,
            LockedOut,
            Success
        }
    }
}