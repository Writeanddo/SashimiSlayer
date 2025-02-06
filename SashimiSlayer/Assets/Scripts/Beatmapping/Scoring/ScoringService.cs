using System;
using Beatmapping.Notes;
using Events;
using Events.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beatmapping.Scoring
{
    public class ScoringService : MonoBehaviour
    {
        public struct BeatmapScore : IComparable<BeatmapScore>
        {
            public string BeatmapName;
            public int Perfects;
            public int Earlies;
            public int Lates;
            public int Misses;

            public int FinalScore;

            public int MaxScore;

            /// <summary>
            ///     Unused; player death has been disabled
            /// </summary>
            public bool DidSucceed;

            public int CompareTo(BeatmapScore other)
            {
                return FinalScore.CompareTo(other.FinalScore);
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

        [Header("Depends")]

        [SerializeField]
        private ScoreConfigSO _scoreConfig;

        public static ScoringService Instance { get; private set; }

        public BeatmapScore CurrentScore => _currentScore;

        [ShowInInspector]
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

        private void OnBeatInteractionResult(NoteInteraction.FinalResult interactionFinalResult)
        {
            _currentScore.MaxScore += _scoreConfig.PointsForPerfect;
            TimingWindow.Score score = interactionFinalResult.TimingResult.Score;

            if (score == TimingWindow.Score.Miss || !interactionFinalResult.Successful)
            {
                _currentScore.Misses++;
                _currentScore.FinalScore += _scoreConfig.PointsForMiss;
            }
            else if (score == TimingWindow.Score.Perfect)
            {
                _currentScore.Perfects++;
                _currentScore.FinalScore += _scoreConfig.PointsForPerfect;
            }
            else if (score == TimingWindow.Score.Pass)
            {
                if (interactionFinalResult.TimingResult.Direction == TimingWindow.Direction.Early)
                {
                    _currentScore.Earlies++;
                    _currentScore.FinalScore += _scoreConfig.PointsForEarly;
                }
                else
                {
                    _currentScore.Lates++;
                    _currentScore.FinalScore += _scoreConfig.PointsForLate;
                }
            }
        }

        private void HandlePlayerDeath()
        {
            _currentScore.DidSucceed = false;
        }

        private void OnLoadBeatmap(BeatmapConfigSo beatmap)
        {
            // Reset scoring
            _currentScore = new BeatmapScore
            {
                BeatmapName = beatmap.BeatmapName,
                DidSucceed = true
            };
        }
    }
}