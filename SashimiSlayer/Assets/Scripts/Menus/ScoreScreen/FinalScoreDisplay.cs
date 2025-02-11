using Beatmapping.Scoring;
using Cysharp.Threading.Tasks;
using Events;
using Menus.ScoreScreen;
using TMPro;
using UnityEngine;

public class FinalScoreDisplay : MonoBehaviour
{
    [Header("Display")]

    [SerializeField]
    private TMP_Text _scoreText;

    [SerializeField]
    private TMP_Text _highscoreText;

    [SerializeField]
    private CategoryLine _perfectLine;

    [SerializeField]
    private CategoryLine _earlyLine;

    [SerializeField]
    private CategoryLine _lateLine;

    [SerializeField]
    private CategoryLine _missLine;

    [Header("Events")]

    [SerializeField]
    private VoidEvent _onProtagVictory;

    [SerializeField]
    private VoidEvent _onProtagLoss;

    [Header("Depends")]

    [SerializeField]
    private ScoreConfigSO _scoreConfig;

    private float _percentageAnimated;

    private void Start()
    {
        ScoringService.BeatmapScore scoring = ScoringService.Instance.CurrentScore;

        if (scoring.DidSucceed)
        {
            _onProtagVictory.Raise();
        }
        else
        {
            _onProtagLoss.Raise();
        }

        DisplayUI(scoring).Forget();
    }

    private async UniTaskVoid DisplayUI(ScoringService.BeatmapScore scoring)
    {
        _scoreText.gameObject.SetActive(false);
        _perfectLine.SetVisible(false);
        _earlyLine.SetVisible(false);
        _lateLine.SetVisible(false);
        _missLine.SetVisible(false);
        _highscoreText.gameObject.SetActive(false);

        var delay = 500;

        await UniTask.Delay(1000);

        _perfectLine.SetVisible(true);
        _perfectLine.SetCategory("Perfect", scoring.Perfects, _scoreConfig.PointsForPerfect);

        await UniTask.Delay(delay);

        _earlyLine.SetVisible(true);
        _earlyLine.SetCategory("Early", scoring.Earlies, _scoreConfig.PointsForEarly);

        await UniTask.Delay(delay);

        _lateLine.SetVisible(true);
        _lateLine.SetCategory("Late", scoring.Lates, _scoreConfig.PointsForLate);

        await UniTask.Delay(delay);

        _missLine.SetVisible(true);
        _missLine.SetCategory("Miss", scoring.Misses, _scoreConfig.PointsForMiss);

        await UniTask.Delay(delay);

        _scoreText.gameObject.SetActive(true);
        _scoreText.text = scoring.FinalScore.ToString();

        await UniTask.Delay(delay);

        HighScore(scoring);
    }

    private void HighScore(ScoringService.BeatmapScore scoring)
    {
        _highscoreText.gameObject.SetActive(true);

        float currentHighestScore = PlayerPrefs.GetFloat($"{scoring.BeatmapName}.highscore", 0);

        if (scoring.FinalScore > currentHighestScore && scoring.DidSucceed)
        {
            _highscoreText.text = "New Highscore!";
            PlayerPrefs.SetFloat($"{scoring.BeatmapName}.highscore", scoring.FinalScore);
        }
        else
        {
            if (currentHighestScore > 0)
            {
                _highscoreText.text = $"Highscore: {currentHighestScore}";
            }
            else
            {
                _highscoreText.text = "No Completions Yet!";
            }
        }
    }
}