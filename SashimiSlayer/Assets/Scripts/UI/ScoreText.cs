using Beatmapping.Scoring;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ScoreText : MonoBehaviour
    {
        [SerializeField]
        private BeatmapScoreEvent _beatmapScoreEvent;

        [SerializeField]
        private TMP_Text _scoreText;

        [SerializeField]
        private float _shakeDuration;

        [SerializeField]
        private float _shakeStrength;

        [SerializeField]
        private int _vibratoStrength;

        private void OnEnable()
        {
            _beatmapScoreEvent.AddListener(OnBeatmapScore);
        }

        private void OnDisable()
        {
            _beatmapScoreEvent.RemoveListener(OnBeatmapScore);
        }

        private void OnBeatmapScore(ScoringService.BeatmapScore score)
        {
            _scoreText.transform.DOShakePosition(_shakeDuration, _shakeStrength, _vibratoStrength);
            _scoreText.text = score.FinalScore.ToString();
        }
    }
}