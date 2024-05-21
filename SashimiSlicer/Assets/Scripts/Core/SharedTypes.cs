using System;

public static class Gameplay
{
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
}