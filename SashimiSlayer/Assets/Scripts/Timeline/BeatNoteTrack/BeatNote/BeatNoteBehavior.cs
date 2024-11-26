using System;
using Beatmapping.Notes;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.BeatNoteTrack.BeatNote
{
    [Serializable]
    public class BeatNoteBehavior : PlayableBehaviour
    {
        public BeatNoteTypeSO HitConfig;

        public BeatNoteData NoteData;

        private BeatNoteService _beatNoteService;

        private Beatmapping.Notes.BeatNote _beatNote;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_beatNoteService == null)
            {
                _beatNoteService = playerData as BeatNoteService;

                if (_beatNoteService == null)
                {
                    return;
                }
            }

            if (_beatNote == null)
            {
                BeatmapConfigSo beatmap;
                if (Application.isPlaying)
                {
                    beatmap = LevelLoader.Instance.CurrentLevel.Beatmap;
                }
                else
                {
                    beatmap = BeatmapEditorUtil.CurrentEditingBeatmapConfig;
                }

                _beatNote = _beatNoteService.SpawnNote(
                    HitConfig,
                    NoteData,
                    beatmap);
            }

            Debug.DrawLine(NoteData.Positions[0], NoteData.Positions[0] + Vector2.up * 10, Color.red);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            Cleanup();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_beatNote == null)
            {
                return;
            }

            _beatNoteService.CleanupNote(_beatNote);
            _beatNote = null;
        }
    }
}