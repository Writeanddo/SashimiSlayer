using System;

public static class SharedTypes
{
    public enum BeatInteractionResultType
    {
        Successful,
        Failure
    }

    [Flags]
    public enum BlockPoseStates
    {
        TopPose = 1 << 0,
        MidPose = 1 << 1,
        BotPose = 1 << 2
    }

    public enum SheathState
    {
        Sheathed,
        Unsheathed
    }

    public struct BeatInteractionResult
    {
        public BnHActionCore.InteractionType InteractionType;
        public BeatInteractionResultType Result;
        public BnHActionSo Action;
        public double TimingOffset;
    }
}