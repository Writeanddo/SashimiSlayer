using UnityEngine;

namespace Timeline
{
    public static class BeatmapEditorUtil
    {
        private static BeatmapConfigSo _editingBeatmapConfig;

        public static BeatmapConfigSo CurrentEditingBeatmapConfig
        {
            get
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("EditingBeatmapConfig  should only be used in edit mode");
                }

                return _editingBeatmapConfig;
            }
        }

        public static void SetBeatmapConfig(BeatmapConfigSo beatmapConfig)
        {
            _editingBeatmapConfig = beatmapConfig;
        }
    }
}