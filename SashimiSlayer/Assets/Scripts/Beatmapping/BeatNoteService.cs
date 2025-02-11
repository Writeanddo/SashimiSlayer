using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Events.Core;
using UnityEditor;
using UnityEngine;

namespace Beatmapping
{
    public class BeatNoteService : MonoBehaviour
    {
        [Header("Events")]

        [SerializeField]
        private ProtagSwordStateEvent _onBlockByProtag;

        [SerializeField]
        private ProtagSwordStateEvent _onSliceByProtag;

        private readonly List<BeatNote> _activeBeatNotes = new();

        private void Awake()
        {
            _onBlockByProtag.AddListener(OnBlockByProtag);
            _onSliceByProtag.AddListener(OnSliceByProtag);
        }

        private void OnEnable()
        {
            var timingService = BeatmapTimeManager.Instance;
            if (timingService == null)
            {
                Debug.LogWarning("Timing service not found");
            }

            timingService.OnTick += TimeManager_OnTick;
        }

        private void OnDisable()
        {
            BeatmapTimeManager.Instance.OnTick -= TimeManager_OnTick;
        }

        private void OnDestroy()
        {
            _onBlockByProtag.RemoveListener(OnBlockByProtag);
            _onSliceByProtag.RemoveListener(OnSliceByProtag);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Destroy children, in case timeline failed to clean up
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    BeatNote[] notes = GetComponentsInChildren<BeatNote>();
                    foreach (BeatNote note in notes)
                    {
                        DestroyImmediate(note.gameObject);
                    }
                };
            }
#endif
        }

        private void TimeManager_OnTick(BeatmapTimeManager.TickInfo tickInfo)
        {
            // Time manager ticks are during play mode, so include all flags
            TickNotes(tickInfo, BeatNote.TickFlags.All);
        }

        private void OnBlockByProtag(Protaganist.ProtagSwordState swordState)
        {
            foreach (BeatNote note in _activeBeatNotes)
            {
                note.AttemptPlayerBlock(swordState);
            }
        }

        private void OnSliceByProtag(Protaganist.ProtagSwordState swordState)
        {
            foreach (BeatNote note in _activeBeatNotes)
            {
                note.AttemptPlayerSlice(swordState);
            }
        }

        public BeatNote SpawnNote(BeatNoteTypeSO hitConfig,
            BeatNoteData data,
            BeatmapConfigSo beatmap,
            double initalizeTime)
        {
            TimingWindowSO timingWindowSo = beatmap.TimingWindowSO;

            double noteStartTime = data.NoteStartTime;
            double noteBeatLength = data.NoteBeatCount;
            double timeIntervalPerBeat = 60 / beatmap.Bpm;

            // Create interactions from data
            var interactions = new List<NoteInteraction>();

            NoteInteraction CreateNoteInteraction(SequencedNoteInteraction sequencedInteraction)
            {
                NoteInteractionData interactionData = sequencedInteraction.InteractionData;
                double beatsFromStart = sequencedInteraction.GetBeatsFromNoteStart(noteBeatLength);

                TimingWindow timingWindow = timingWindowSo.CreateTimingWindow(
                    noteStartTime + beatsFromStart * timeIntervalPerBeat);

                return interactionData.ToNoteInteraction(
                    timingWindow);
            }

            foreach (SequencedNoteInteraction sequencedInteraction in data.Interactions)
            {
                interactions.Add(CreateNoteInteraction(sequencedInteraction));
            }

            // Sort, in case they were configured out of chronological order
            interactions.Sort((a, b) => a.TargetTime.CompareTo(b.TargetTime));

            // Instantiate note and register
            BeatNote note = Instantiate(hitConfig.Prefab, transform);
            note.Initialize(
                interactions,
                data.StartPosition,
                data.EndPosition,
                data.NoteStartTime,
                data.NoteEndTime,
                initalizeTime,
                hitConfig.HitboxRadius,
                hitConfig.DamageDealtToPlayer
            );

            _activeBeatNotes.Add(note);

            note.OnReadyForCleanup += HandleCleanupRequested;

            return note;
        }

        private void TickNotes(BeatmapTimeManager.TickInfo tickInfo, BeatNote.TickFlags tickFlags)
        {
            BeatNote[] hits = _activeBeatNotes.ToArray();
            foreach (BeatNote hit in hits)
            {
                hit.Tick(tickInfo, tickFlags);
            }
        }

        public void CleanupNote(BeatNote note)
        {
            _activeBeatNotes.Remove(note);
            if (Application.isPlaying)
            {
                Destroy(note.gameObject);
            }
            else
            {
                DestroyImmediate(note.gameObject);
            }
        }

        private void HandleCleanupRequested(BeatNote note)
        {
            note.OnReadyForCleanup -= HandleCleanupRequested;
            CleanupNote(note);
        }
    }
}