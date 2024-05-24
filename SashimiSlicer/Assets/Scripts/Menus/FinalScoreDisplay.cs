using DG.Tweening;
using TMPro;
using UnityEngine;

public class FinalScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _percentageText;

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
    }

    private void UpdatePercentageText()
    {
        _percentageText.text = $"Accuracy: {_percentageAnimated:P2}";
    }
}