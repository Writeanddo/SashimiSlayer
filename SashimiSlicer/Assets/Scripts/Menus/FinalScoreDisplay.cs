using DG.Tweening;
using Events;
using TMPro;
using UnityEngine;

public class FinalScoreDisplay : MonoBehaviour
{
    [Header("Display")]

    [SerializeField]
    private TMP_Text _percentageText;

    [SerializeField]
    private TMP_Text _victoryText;

    [SerializeField]
    private TMP_Text _failureText;

    [SerializeField]
    private TMP_Text _highscoreText;

    [Header("Events")]

    [SerializeField]
    private VoidEvent _onProtagVictory;

    [SerializeField]
    private VoidEvent _onProtagLoss;

    private ScoringService.BeatmapScore _finalScore;

    private float _percentageAnimated;

    private void Awake()
    {
        _finalScore = ScoringService.Instance.CurrentScore;

        _percentageAnimated = 0;

        float targetPercentage = (float)_finalScore.Successes / (_finalScore.Successes + _finalScore.Failures);

        DOTween.To(
                () => _percentageAnimated,
                x => _percentageAnimated = x,
                targetPercentage,
                5f)
            .SetEase(Ease.OutExpo)
            .OnUpdate(UpdatePercentageText);

        _victoryText.enabled = _finalScore.DidSucceed;
        _failureText.enabled = !_finalScore.DidSucceed;

        if (_finalScore.DidSucceed)
        {
            _onProtagVictory.Raise();
            _percentageText.gameObject.SetActive(true);
        }
        else
        {
            _onProtagLoss.Raise();
            _percentageText.enabled = false;
            _percentageText.gameObject.SetActive(false);
        }

        float currentHighestAccuracy = PlayerPrefs.GetFloat($"{_finalScore.BeatmapName}.highscore", 0);

        if (targetPercentage > currentHighestAccuracy && _finalScore.DidSucceed)
        {
            _highscoreText.text = "New Highscore!";
            PlayerPrefs.SetFloat($"{_finalScore.BeatmapName}.highscore", targetPercentage);
        }
        else
        {
            if (currentHighestAccuracy > 0)
            {
                _highscoreText.text = $"Highscore: {currentHighestAccuracy:P2}";
            }
            else
            {
                _highscoreText.text = "No Completions Yet!";
            }
        }
    }

    private void UpdatePercentageText()
    {
        _percentageText.text = $"{_percentageAnimated:P2}";
    }
}