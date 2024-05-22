using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class SimpleAnimator : MonoBehaviour, IAnimationClipSource
{
    [FormerlySerializedAs("animationClip")]
    [SerializeField]
    private AnimationClip _animationClip;

    [FormerlySerializedAs("animator")]
    [SerializeField]
    private Animator _animator;

    [FormerlySerializedAs("playOnAwake")]
    [SerializeField]
    private bool _playOnAwake;

    public PlayableGraph PlayablePlayableGraph => _playableGraph;

    private PlayableGraph _playableGraph;

    private bool _graphCreated;

    private void Awake()
    {
        if (_playOnAwake)
        {
            Play();
        }
    }

    private void OnDestroy()
    {
        if (_playableGraph.IsValid())
        {
            _playableGraph.Destroy();
        }
    }

    public void GetAnimationClips(List<AnimationClip> results)
    {
        results.Add(_animationClip);
    }

    public void Play()
    {
        // Lazy instatiation
        if (!_graphCreated)
        {
            _graphCreated = true;
            CreateGraph();
        }

        if (_playableGraph.IsValid())
        {
            _playableGraph.Play();
        }
    }

    public void Stop()
    {
        _playableGraph.Stop();
    }

    private void CreateGraph()
    {
        _playableGraph = PlayableGraph.Create($"{gameObject.name}.SimpleAnimator");

        var output = AnimationPlayableOutput.Create(_playableGraph, string.Empty, _animator);

        var animationClipPlayable = AnimationClipPlayable.Create(_playableGraph, _animationClip);

        output.SetSourcePlayable(animationClipPlayable);
    }
}