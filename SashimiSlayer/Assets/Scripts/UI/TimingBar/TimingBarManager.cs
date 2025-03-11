using System.Collections.Generic;
using Beatmapping.Interactions;
using EditorUtils.BoldHeader;
using Events.Core;
using NaughtyAttributes;
using UnityEngine;

namespace UI.TimingBar
{
    public class TimingBarManager : MonoBehaviour
    {
        [BoldHeader("(UNUSED) Timing Bar Manager")]
        [InfoBox("Manages the interaction timing offset bar. Currently Hidden")]
        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [Header("Dependencies")]

        [SerializeField]
        private RectTransform _bar;

        [SerializeField]
        private List<TimingBarTick> _hitResultPrefab;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnBeatInteractionResult);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnBeatInteractionResult);
        }

        private void OnBeatInteractionResult(NoteInteraction.FinalResult result)
        {
            if (!result.Successful)
            {
                return;
            }

            float offset = result.TimingResult.NormalizedTimeDelta;

            TimingBarTick hitResult = Instantiate(_hitResultPrefab[(int)result.Pose], _bar);
            var rectTransform = hitResult.GetComponent<RectTransform>();
            hitResult.SetVisuals(result.InteractionType);
            rectTransform.anchoredPosition = new Vector3(offset * _bar.rect.width / 2f, 0, 0);
        }
    }
}