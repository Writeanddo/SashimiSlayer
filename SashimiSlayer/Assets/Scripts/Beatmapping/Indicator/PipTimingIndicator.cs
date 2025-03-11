using System.Collections.Generic;
using Base;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Script that handles timing indicators using pips that count down
    /// </summary>
    public class PipTimingIndicator : DescMono
    {
        [BoldHeader("Pip Timing Indicator")]
        [InfoBox("Represents a single series of pips that indicate timing")]
        [SerializeField]
        private Transform _visualContainer;

        [Header("Layout")]

        [SerializeField]
        private float _totalDistance;

        [SerializeField]
        private Vector2 _layoutDirection;

        [Header("Indicator")]

        [SerializeField]
        private IndicatorPip _beatPip;

        [SerializeField]
        private IndicatorPip _finalPip;

        [SerializeField]
        private float _delay;

        [Header("Shake")]

        [SerializeField]
        private float _shakeDuration;

        [SerializeField]
        private float _shakeStrength;

        [SerializeField]
        private int _shakeVibrato;

        private readonly List<IndicatorPip> _pips = new();

        private bool _initialized;

        private int _prevBeatRemaining;

        private bool _flashOnNext;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * _totalDistance);
        }

        public void FlashOnNext()
        {
            _flashOnNext = true;
        }

        private void Initialize(BeatmapConfigSo beatmapConfigSo)
        {
            int beatsPerMeasure = beatmapConfigSo.BeatsPerMeasure;

            int totalPips = beatsPerMeasure + 1;

            for (var i = 0; i < totalPips; i++)
            {
                IndicatorPip prefab = i == 0 ? _finalPip : _beatPip;
                IndicatorPip pip = Instantiate(prefab, _visualContainer);
                pip.Setup();

                float t = (float)i / (totalPips - 1);

                pip.transform.localPosition = _layoutDirection.normalized * (_totalDistance * t);

                pip.SetOn(false);
                _pips.Add(pip);
            }
        }

        public void SetVisible(bool visible)
        {
            _visualContainer.gameObject.SetActive(visible);
        }

        public async UniTaskVoid Tick(BeatNote.NoteTickInfo tickInfo)
        {
            await UniTask.Delay((int)_delay * 1000, cancellationToken: destroyCancellationToken);
            if (!_initialized)
            {
                Initialize(tickInfo.BeatmapTickInfo.CurrentBeatmap);
                _initialized = true;
            }

            NoteInteraction interaction = tickInfo.NoteSegment.Interaction;

            int subdivsPerBeat = tickInfo.BeatmapTickInfo.CurrentBeatmap.Subdivisions;
            int currentSubdivision = tickInfo.SubdivisionIndex;

            // We need to calculate in subdivisions for off beats
            double interactionTargetTime = interaction.TargetTime;
            int targetSubdiv = tickInfo.BeatmapTickInfo.GetClosestSubdivisionIndex(interactionTargetTime);

            int beatsRemaining = (targetSubdiv - currentSubdivision + 1) / subdivsPerBeat;
            float normalized = 1 - (float)beatsRemaining / (_pips.Count - 1);

            bool changed = _prevBeatRemaining != beatsRemaining;
            _prevBeatRemaining = beatsRemaining;

            // Don't show anything at all if the target subdiv is before any of the pips
            bool shouldShowPips = beatsRemaining < _pips.Count;

            for (var i = 0; i < _pips.Count; i++)
            {
                bool isVisible = i <= beatsRemaining && shouldShowPips;
                _pips[i].SetVisible(isVisible);

                if (!isVisible)
                {
                    _pips[i].SetOn(false);
                    continue;
                }

                bool isOn = i == beatsRemaining;
                bool wasOn = _pips[i].IsOn;
                _pips[i].SetOn(isOn);
                _pips[i].SetAlpha(normalized);

                if (i == 1 && isOn && !wasOn)
                {
                    _pips[i].transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato);
                }

                if (isOn && !wasOn && _flashOnNext)
                {
                    _flashOnNext = false;
                    _pips[i].Flash();
                }

                if (changed)
                {
                    _pips[i].DoSquish();
                }
            }
        }
    }
}