using Events.Core;
using UnityEngine;

public class TimingBarManager : MonoBehaviour
{
    [SerializeField]
    private BeatInteractionResultEvent _beatInteractionResultEvent;

    [SerializeField]
    private RectTransform _bar;

    [SerializeField]
    private TimingBarTick _hitResultPrefab;

    private void Awake()
    {
        _beatInteractionResultEvent.AddListener(OnBeatInteractionResult);
    }

    private void OnDestroy()
    {
        _beatInteractionResultEvent.RemoveListener(OnBeatInteractionResult);
    }

    private void OnBeatInteractionResult(SharedTypes.BeatInteractionResult result)
    {
        if (result.Result == SharedTypes.BeatInteractionResultType.Failure)
        {
            return;
        }

        var offset = (float)result.NormalizedTimingOffset;
        TimingBarTick hitResult = Instantiate(_hitResultPrefab, _bar);
        var rectTransform = hitResult.GetComponent<RectTransform>();
        hitResult.SetVisuals(result.InteractionType);
        rectTransform.anchoredPosition = new Vector3(offset * _bar.rect.width / 2f, 0, 0);
    }
}