namespace Beatmapping.Tooling
{
    /// <summary>
    ///     Beatmap editor utilities.
    ///     This is intended to only be used in the editor, but should NOT be placed in the editor folder.
    /// </summary>
    public static class BeatmappingUtilities
    {
        /// <summary>
        ///     Should the player be invincible, for beatmapping purposes.
        /// </summary>
        public static bool ProtagInvincible { get; set; }

        /// <summary>
        ///     Should the play mode level start from the editing timeline playhead?
        /// </summary>
        public static bool StartFromTimelinePlayhead { get; set; }

        /// <summary>
        ///     The time of the timeline playhead, for starting the level from the playhead to speed up testing.
        /// </summary>
        public static double TimelinePlayheadTime { get; set; }

        /// <summary>
        ///     Load directly into the edited beatmap on play
        /// </summary>
        public static bool PlayFromEditedBeatmap { get; set; }

        public static BeatmapConfigSo CurrentEditingBeatmapConfig { get; private set; }

        public static void SetBeatmapConfig(BeatmapConfigSo beatmapConfig)
        {
            CurrentEditingBeatmapConfig = beatmapConfig;
        }
    }
}