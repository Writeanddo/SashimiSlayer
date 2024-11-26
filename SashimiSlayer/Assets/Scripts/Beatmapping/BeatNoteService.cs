using System.Collections.Generic;
using System.Linq;
using Beatmapping;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Events.Core;
using UnityEngine;

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
        var timingService = TimingService.Instance;
        if (timingService == null)
        {
            Debug.LogWarning("Timing service not found");
        }

        timingService.OnTick += TimeManager_OnTick;
    }

    private void OnDisable()
    {
        TimingService.Instance.OnTick -= TimeManager_OnTick;
    }

    private void OnDestroy()
    {
        _onBlockByProtag.RemoveListener(OnBlockByProtag);
        _onSliceByProtag.RemoveListener(OnSliceByProtag);
    }

    private void OnValidate()
    {
        // Destroy children, in case timeline failed to clean up
        if (!Application.isPlaying)
        {
            BeatNote[] notes = GetComponentsInChildren<BeatNote>();
            foreach (BeatNote note in notes)
            {
                DestroyImmediate(note.gameObject);
            }
        }
    }

    private void TimeManager_OnTick(TimingService.TickInfo tickInfo)
    {
        TickNotes(tickInfo);
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

    public BeatNote SpawnNote(BeatNoteTypeSO hitConfig, BeatNoteData data, BeatmapConfigSo beatmap)
    {
        TimingWindowSO timingWindowSo = beatmap.TimingWindowSO;

        double noteStartTime = data.NoteStartTime;
        double timeIntervalPerBeat = 60 / beatmap.Bpm;

        // Create interactions from data
        var interactions = new List<NoteInteraction>();

        NoteInteraction CreateNoteInteraction(SequencedNoteInteraction sequencedInteraction)
        {
            NoteInteractionData interactionData = sequencedInteraction.InteractionData;
            uint beatOffset = sequencedInteraction.BeatsFromNoteStart;

            TimingWindow timingWindow = timingWindowSo.CreateTimingWindow(
                noteStartTime + beatOffset * timeIntervalPerBeat);

            return new NoteInteraction(
                interactionData.InteractionType,
                interactionData.Flags,
                interactionData.BlockPose,
                timingWindow);
        }

        foreach (SequencedNoteInteraction sequencedInteraction in data.Interactions)
        {
            interactions.Add(CreateNoteInteraction(sequencedInteraction));
        }

        // Instantiate note and register
        BeatNote note = Instantiate(hitConfig.Prefab, transform);
        note.Initialize(
            interactions,
            data.Positions.ToList(),
            data.NoteStartTime,
            data.NoteEndTime,
            hitConfig.HitboxRadius,
            hitConfig.DamageDealtToPlayer
        );

        _activeBeatNotes.Add(note);

        note.OnReadyForCleanup += HandleCleanupRequested;

        return note;
    }

    private void TickNotes(TimingService.TickInfo tickInfo)
    {
        BeatNote[] hits = _activeBeatNotes.ToArray();
        foreach (BeatNote hit in hits)
        {
            hit.Tick(tickInfo);
        }
    }

    public void CleanupNote(BeatNote blockAndHit)
    {
        _activeBeatNotes.Remove(blockAndHit);
        if (Application.isPlaying)
        {
            Destroy(blockAndHit.gameObject);
        }
        else
        {
            DestroyImmediate(blockAndHit.gameObject);
        }
    }

    private void HandleCleanupRequested(BeatNote action)
    {
        action.OnReadyForCleanup -= HandleCleanupRequested;
        CleanupNote(action);
    }
}