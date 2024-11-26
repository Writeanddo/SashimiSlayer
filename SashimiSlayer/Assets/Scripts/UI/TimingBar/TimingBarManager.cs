using Events.Core;
using UnityEngine;

public class TimingBarManager : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

    [SerializeField]
    private RectTransform _bar;

    [SerializeField]
    private TimingBarTick _hitResultPrefab;

    private void Awake()
    {
        _noteInteractionFinalResultEvent.AddListener(OnBeatInteractionResult);
    }

    private void OnDestroy()
    {
        _noteInteractionFinalResultEvent.RemoveListener(OnBeatInteractionResult);
    }

    private void OnBeatInteractionResult(SharedTypes.InteractionFinalResult result)
    {
        if (!result.Successful)
        {
            return;
        }

        float offset = result.TimingResult.NormalizedTimeDelta;
        TimingBarTick hitResult = Instantiate(_hitResultPrefab, _bar);
        var rectTransform = hitResult.GetComponent<RectTransform>();
        hitResult.SetVisuals(result.InteractionType);
        rectTransform.anchoredPosition = new Vector3(offset * _bar.rect.width / 2f, 0, 0);
    }
}