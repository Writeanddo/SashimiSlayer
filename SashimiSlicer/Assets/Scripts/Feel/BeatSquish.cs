using System;
using System.Collections.Generic;
using DG.Tweening;
using Events;
using UnityEngine;

public class BeatSquish : MonoBehaviour
{
    [Serializable]
    public struct BeatSquishable
    {
        public Transform transform;
        public int beatInterval;
        public int beatOffset;
    }

    [SerializeField]
    private float _squishScale;

    [SerializeField]
    private float _squishDuration;

    [SerializeField]
    private List<BeatSquishable> _squishTransform;

    [SerializeField]
    private IntEvent _beatPassedEvent;

    private void Awake()
    {
        _beatPassedEvent.AddListener(HandleBeatPassed);
    }

    private void OnDestroy()
    {
        _beatPassedEvent.RemoveListener(HandleBeatPassed);
    }

    private void HandleBeatPassed(int beatNumber)
    {
        foreach (BeatSquishable squishable in _squishTransform)
        {
            if ((beatNumber + squishable.beatOffset) % squishable.beatInterval != 0)
            {
                continue;
            }

            squishable.transform.localScale = new Vector3(1 / _squishScale, _squishScale, 1);
            squishable.transform.DOScaleY(1, _squishDuration);
            squishable.transform.DOScaleX(1, _squishDuration);
        }
    }
}