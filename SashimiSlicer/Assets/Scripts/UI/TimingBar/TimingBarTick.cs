using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimingBarTick : MonoBehaviour
{
    [SerializeField]
    private float _fadeDuration;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private Image _blockImage;

    [SerializeField]
    private Image _sliceImage;

    private void Awake()
    {
        _canvasGroup.DOFade(0, _fadeDuration).SetEase(Ease.OutQuint).OnComplete(() => Destroy(gameObject));
    }

    public void SetVisuals(BnHActionCore.InteractionType resultInteractionType)
    {
        _blockImage.enabled = resultInteractionType == BnHActionCore.InteractionType.IncomingAttack;
        _sliceImage.enabled = resultInteractionType == BnHActionCore.InteractionType.Vulnerable;
    }
}