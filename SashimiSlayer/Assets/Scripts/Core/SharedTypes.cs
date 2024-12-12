using Beatmapping;
using Beatmapping.Notes;

public static class SharedTypes
{
    public enum BlockPoseStates
    {
        TopPose,
        MidPose,
        BotPose
    }

    public enum SheathState
    {
        Sheathed,
        Unsheathed
    }

    public const int NumPoses = 3;

    /// <summary>
    ///     The final result of a note interaction; either a success when it occurs, or a failure after the timing window ends
    /// </summary>
    public struct InteractionFinalResult
    {
        public TimingWindow.TimingResult TimingResult;
        public NoteInteraction.InteractionType InteractionType;
        public BlockPoseStates Pose;
        public bool Successful;
    }
}