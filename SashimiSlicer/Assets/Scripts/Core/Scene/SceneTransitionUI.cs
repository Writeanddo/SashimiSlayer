using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SceneTransitionUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _titleText;

    [SerializeField]
    private CanvasGroup _fadeOutCanvasGroup;

    [SerializeField]
    private CanvasGroup _fadeInCanvasGroup;

    [SerializeField]
    private float _fadeOutTime;

    [SerializeField]
    private float _fadeInTime;

    [SerializeField]
    private float _fadeInDelay;

    public void SetTitleText(string text)
    {
        _titleText.text = text;
    }

    public async UniTask FadeOut()
    {
        _fadeOutCanvasGroup.DOFade(1, _fadeOutTime);
        await UniTask.Delay((int)(_fadeOutTime * 1000));
        _fadeOutCanvasGroup.alpha = 1;
    }

    public async UniTask FadeIn()
    {
        _fadeInCanvasGroup.alpha = 1;
        _fadeOutCanvasGroup.alpha = 0;
        await UniTask.Delay((int)(_fadeInDelay * 1000));

        _fadeInCanvasGroup.DOFade(0, _fadeInTime);
        await UniTask.Delay((int)(_fadeInTime * 1000));
        _fadeInCanvasGroup.alpha = 0;
    }
}