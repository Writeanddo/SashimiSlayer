using TMPro;
using UnityEngine;

namespace UI
{
    public class SliderLabel : MonoBehaviour
    {
        public enum Style
        {
            Value,
            Percentage
        }

        [SerializeField]
        private TMP_Text _label;

        [SerializeField]
        private UnityEngine.UI.Slider _slider;

        [SerializeField]
        private Style _style;

        private void Awake()
        {
            _slider.onValueChanged.AddListener(SetValue);

            SetValue(_slider.value);
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(SetValue);
        }

        public void SetValue(float value)
        {
            if (_style == Style.Percentage)
            {
                _label.text = $"{value * 100:F0}%";
                return;
            }

            if (_style == Style.Value)
            {
                _label.text = $"{value:F2}";
            }
        }
    }
}