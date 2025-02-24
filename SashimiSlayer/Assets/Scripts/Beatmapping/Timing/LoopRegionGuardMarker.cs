using System;
using System.Runtime.InteropServices;
using AOT;
using Beatmapping.Notes;
using Events;
using Events.Core;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beatmapping.Timing
{
    /// <summary>
    ///     Handles FMOD destination markers that act as "guards" against loop leaking.
    ///     Loop leaking is when loops trigger on the frame after their actual time, causing notes to spawn that occur within 1
    ///     frame after the loop
    /// </summary>
    public class LoopRegionGuardMarker : MonoBehaviour
    {
        [SerializeField]
        private BeatmapTimeManager _beatmapTimeManager;

        [Header("Event (Out)")]

        [SerializeField]
        private BoolEvent _setBeatNoteSpawningEnabledEvent;

        [Header("Event (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        private EVENT_CALLBACK _callback;

        private int _currentSuccessfulStreak;

        private int _successfulStreakRequiredToUnlockLoopRegion;

        private double _guardMarkerBeatmapTime;

        private void Awake()
        {
            _beatmapTimeManager.OnBeatmapSoundtrackInstanceCreated += OnBeatmapSoundtrackInstanceCreated;
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
            _beatmapTimeManager.OnTick += OnTick;
        }

        private void OnDestroy()
        {
            _beatmapTimeManager.OnBeatmapSoundtrackInstanceCreated -= OnBeatmapSoundtrackInstanceCreated;
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
            _beatmapTimeManager.OnTick -= OnTick;
        }

        private void OnTick(BeatmapTimeManager.TickInfo tickInfo)
        {
            // If we looped back to before the guard marker, wipe the marker and unlock the note spawning
            if (tickInfo.CurrentBeatmapTime < _guardMarkerBeatmapTime)
            {
                _successfulStreakRequiredToUnlockLoopRegion = 0;
                _setBeatNoteSpawningEnabledEvent.Raise(true);
            }
        }

        private void OnNoteInteractionFinalResult(NoteInteraction.FinalResult result)
        {
            if (result.Successful)
            {
                _currentSuccessfulStreak++;
            }
            else
            {
                _currentSuccessfulStreak = 0;
            }

            UpdateSpawningEnabled();
        }

        private void UpdateSpawningEnabled()
        {
            bool spawningEnabled = _currentSuccessfulStreak >= _successfulStreakRequiredToUnlockLoopRegion;
            _setBeatNoteSpawningEnabledEvent.Raise(spawningEnabled);
        }

        private void OnBeatmapSoundtrackInstanceCreated(EventInstance instance)
        {
            _callback = OnEventCallback;
            instance.setCallback(_callback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        }

        [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private RESULT OnEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            var instance = new EventInstance(instancePtr);

            IntPtr timelineInfoPtr;
            RESULT result = instance.getUserData(out timelineInfoPtr);
            if (result != RESULT.OK)
            {
                Debug.LogError("Timeline Callback error: " + result);
                return RESULT.OK;
            }

            var parameter =
                (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));

            // The name of the marker is the number of successes needed to pass the incoming loop region check
            // This number is used to alert the note manager that a loop check is incoming
            // This is a hack used to prevent spawning notes due to loop leaks
            string markerName = parameter.name;

            Debug.Log("Loop region guard marker passed: " + markerName);

            try
            {
                _successfulStreakRequiredToUnlockLoopRegion = int.Parse(markerName.Split(' ')[0]);
                _guardMarkerBeatmapTime = _beatmapTimeManager.CurrentTickInfo.CurrentBeatmapTime;
                UpdateSpawningEnabled();
            }
            catch (Exception)
            {
                return RESULT.OK;
            }

            return RESULT.OK;
        }
    }
}