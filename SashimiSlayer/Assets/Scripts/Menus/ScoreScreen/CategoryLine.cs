using TMPro;
using UnityEngine;

namespace Menus.ScoreScreen
{
    /// <summary>
    ///     UI controller for a single score category line (i.e "Early: 10 x 50pts = 500")
    /// </summary>
    public class CategoryLine : MonoBehaviour
    {
        [Header("Display")]

        [SerializeField]
        private TMP_Text _categoryText;

        [SerializeField]
        private TMP_Text _countText;

        [SerializeField]
        private TMP_Text _scorePerCountText;

        [SerializeField]
        private TMP_Text _totalScoreText;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        public void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1 : 0;
        }

        public void SetCategory(string category, int count, int scorePerCount)
        {
            _categoryText.text = category;
            _countText.text = count.ToString();
            _scorePerCountText.text = scorePerCount.ToString();
            _totalScoreText.text = (count * scorePerCount).ToString();
        }
    }
}