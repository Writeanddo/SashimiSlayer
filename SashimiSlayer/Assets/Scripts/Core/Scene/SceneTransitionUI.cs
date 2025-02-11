using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SceneTransitionUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private RectTransform _fadeInTransform;

    [SerializeField]
    private RectTransform _ref;

    [SerializeField]
    private float _fadeOutTime;

    [SerializeField]
    private float _fadeInTime;

    [SerializeField]
    private float _fadeInDelay;

    [SerializeField]
    private float _margin;

    private void Awake()
    {
        _fadeInTransform.gameObject.SetActive(true);
    }

    public void SetTitleText(string text)
    {
        _titleText.text = text;
    }

    public async UniTask FadeOut()
    {
        if (_fadeInTransform == null)
        {
            return;
        }

        _fadeInTransform.anchoredPosition = new Vector2(-_ref.rect.width - _margin, 0);
        DOTween.To(
                () => _fadeInTransform.anchoredPosition.x,
                x => _fadeInTransform.anchoredPosition = new Vector2(x, 0),
                0,
                _fadeInTime)
            .SetEase(Ease.InOutSine);
        await UniTask.Delay((int)(_fadeOutTime * 1000));
    }

    public async UniTask FadeIn()
    {
        if (_fadeInTransform == null)
        {
            return;
        }

        _fadeInTransform.anchoredPosition = new Vector2(0, 0);
        await UniTask.Delay((int)(_fadeInDelay * 1000));
        DOTween.To(
                () => _fadeInTransform.anchoredPosition.x,
                x => _fadeInTransform.anchoredPosition = new Vector2(x, 0),
                _ref.rect.width + _margin,
                _fadeInTime)
            .SetEase(Ease.InOutSine);
        await UniTask.Delay((int)(_fadeInTime * 1000));
    }
}