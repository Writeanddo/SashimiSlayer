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
        private Vector2 _valueRange;

        [SerializeField]
        private Style _style;

        public void SetValue(float value)
        {
            if (_style == Style.Percentage)
            {
                _label.text = $"{Mathf.Lerp(_valueRange.x, _valueRange.y, value) * 100:F0}%";
                return;
            }

            if (_style == Style.Value)
            {
                _label.text = $"{Mathf.Lerp(_valueRange.x, _valueRange.y, value):F2}";
            }
        }
    }
}