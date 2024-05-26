using System.Collections.Generic;
using DG.Tweening;
using Events;
using UnityEngine;

public class BeatSquish : MonoBehaviour
{
    [SerializeField]
    private float _squishScale;

    [SerializeField]
    private float _squishDuration;

    [SerializeField]
    private List<Transform> _squishTransform;

    [SerializeField]
    private VoidEvent _beatPassedEvent;

    private void Awake()
    {
        _beatPassedEvent.AddListener(HandleBeatPassed);
    }

    private void OnDestroy()
    {
        _beatPassedEvent.RemoveListener(HandleBeatPassed);
    }

    private void HandleBeatPassed()
    {
        foreach (Transform transform in _squishTransform)
        {
            transform.localScale = new Vector3(1 / _squishScale, _squishScale, 1);
            transform.DOScaleY(1, _squishDuration);
            transform.DOScaleX(1, _squishDuration);
            ;
        }
    }
}