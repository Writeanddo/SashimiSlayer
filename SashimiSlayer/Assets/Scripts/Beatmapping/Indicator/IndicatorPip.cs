using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Subdivision tick for the indicator. Can be turned on and off.
    /// </summary>
    public class IndicatorPip : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _onSprite;

        [SerializeField]
        private SpriteRenderer _offSprite;

        [SerializeField]
        private AnimationCurve _alphaCurve;

        [SerializeField]
        private float _squishScale;

        [SerializeField]
        private float _squishDuration;

        private float _onSpriteAlpha;
        private float _offSpriteAlpha;

        public void Setup()
        {
            _onSpriteAlpha = _onSprite.color.a;
            _offSpriteAlpha = _offSprite.color.a;
        }

        [Button("Set On")]
        public void SetOn()
        {
            SetOn(true);
        }

        [Button("Set Off")]
        public void SetOff()
        {
            SetOn(false);
        }

        public void SetOn(bool isOn)
        {
            _onSprite.enabled = isOn;
            _offSprite.enabled = !isOn;
        }

        public void DoSquish()
        {
            transform.localScale = new Vector3(1 / _squishScale, _squishScale, 1);
            transform.DOScaleY(1, _squishDuration);
            transform.DOScaleX(1, _squishDuration);
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetAlpha(float normalized)
        {
            float alpha = _alphaCurve.Evaluate(normalized);

            Color color = _onSprite.color;
            color.a = alpha * _onSpriteAlpha;
            _onSprite.color = color;

            color = _offSprite.color;
            color.a = alpha * _offSpriteAlpha;
            _offSprite.color = color;
        }
    }
}