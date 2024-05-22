using System;
using UnityEngine;

namespace Events
{
    public abstract class SOEvent : ScriptableObject
    {
        [SerializeField]
        [TextArea]
        private string _description;
    }

    public abstract class SOEvent<T> : SOEvent
    {
        private event Action<T> _internalEvent;

        public void Raise(T value)
        {
            _internalEvent?.Invoke(value);
        }

        public void AddListener(Action<T> listener)
        {
            _internalEvent += listener;
        }

        public void RemoveListener(Action<T> listener)
        {
            _internalEvent -= listener;
        }
    }
}