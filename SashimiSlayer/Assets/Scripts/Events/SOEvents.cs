using System;
using UnityEngine;

namespace Events
{
    public abstract class SOEvent : ScriptableObject
    {
        [SerializeField]
        [TextArea(3, 10)]
        private string _description;

        [SerializeField]
        protected bool _debug;

        protected Action _internalVoidEvent;

        public void AddListener(Action listener)
        {
            _internalVoidEvent += listener;
        }

        public void RemoveListener(Action listener)
        {
            _internalVoidEvent -= listener;
        }
    }

    public abstract class SOEvent<T> : SOEvent
    {
        private event Action<T> _internalEvent;

        public void Raise(T value)
        {
            _internalEvent?.Invoke(value);
            _internalVoidEvent?.Invoke();
            if (_debug)
            {
                Debug.Log($"Event {name} raised with value {value}");
            }
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