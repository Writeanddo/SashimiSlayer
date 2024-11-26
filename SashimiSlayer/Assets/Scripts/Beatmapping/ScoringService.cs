using System;
using Events;
using Events.Core;
using UnityEngine;

namespace Beatmapping
{
    public class ScoringService : MonoBehaviour
    {
        public struct BeatmapScore : IComparable<BeatmapScore>
        {
            public string BeatmapName;
            public int Successes;
            public int Failures;
            public bool DidSucceed;

            public int CompareTo(BeatmapScore other)
            {
                return Successes - other.Successes;
            }
        }

        [Header("Listening Events")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _interactionFinalResultEvent;

        [SerializeField]
        private BeatmapEvent _loadBeatmapEvent;

        [SerializeField]
        private VoidEvent _playerDeathEvent;

        [SerializeField]
        private VoidEvent _onDrawGuiEvent;

        public static ScoringService Instance { get; private set; }

        public BeatmapScore CurrentScore => _currentScore;

        private BeatmapScore _currentScore;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _interactionFinalResultEvent.AddListener(OnBeatInteractionResult);
            _loadBeatmapEvent.AddListener(OnLoadBeatmap);
            _playerDeathEvent.AddListener(HandlePlayerDeath);
            _onDrawGuiEvent.AddListener(DrawGUI);
        }

        private void OnDestroy()
        {
            _interactionFinalResultEvent.RemoveListener(OnBeatInteractionResult);
            _loadBeatmapEvent.RemoveListener(OnLoadBeatmap);
            _playerDeathEvent.RemoveListener(HandlePlayerDeath);
            _onDrawGuiEvent.RemoveListener(DrawGUI);
        }

        private void DrawGUI()
        {
            GUILayout.Label(JsonUtility.ToJson(_currentScore));
        }

        private void OnBeatInteractionResult(SharedTypes.InteractionFinalResult interactionFinalResult)
        {
            if (interactionFinalResult.Successful)
            {
                _currentScore.Successes++;
            }
            else
            {
                _currentScore.Failures++;
            }
        }

        private void HandlePlayerDeath()
        {
            _currentScore.DidSucceed = false;
        }

        private void OnLoadBeatmap(BeatmapConfigSo beatmap)
        {
            _currentScore = new BeatmapScore();
            _currentScore.BeatmapName = beatmap.BeatmapName;
            _currentScore.DidSucceed = true;
        }
    }
}