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
        private Style _style;

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