using System;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Basic/VoidEvent")]
    public class VoidEvent : SOEvent
    {
        private event Action _internalEvent;

        public void Raise()
        {
            _internalEvent?.Invoke();
        }

        public void AddListener(Action listener)
        {
            _internalEvent += listener;
        }

        public void RemoveListener(Action listener)
        {
            _internalEvent -= listener;
        }
    }
}