using Events;
using Events.Core;
using UnityEngine;

public class ProtagAnimation : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private VoidEvent _protagSuccessfulBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _protagTryBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _protagUnsheatheEvent;

    [SerializeField]
    private ProtagSwordStateEvent _protagSliceEvent;

    [SerializeField]
    private VoidEvent _protagTakeDamageEvent;

    [SerializeField]
    private Animator _animator;

    private void Awake()
    {
        _protagSuccessfulBlockEvent.AddListener(OnProtagSuccessfulBlock);
        _protagTryBlockEvent.AddListener(OnProtagTryBlock);
        _protagUnsheatheEvent.AddListener(OnProtagUnsheathe);
        _protagSliceEvent.AddListener(OnProtagSlice);
        _protagTakeDamageEvent.AddListener(OnProtagTakeDamage);
    }

    private void OnDestroy()
    {
        _protagSuccessfulBlockEvent.RemoveListener(OnProtagSuccessfulBlock);
        _protagTryBlockEvent.RemoveListener(OnProtagTryBlock);
        _protagUnsheatheEvent.RemoveListener(OnProtagUnsheathe);
        _protagSliceEvent.RemoveListener(OnProtagSlice);
        _protagTakeDamageEvent.RemoveListener(OnProtagTakeDamage);
    }

    private void OnProtagSuccessfulBlock()
    {
        _animator.SetTrigger("SuccessfulBlock");
    }

    private void OnProtagTryBlock(Protaganist.ProtagSwordState state)
    {
        _animator.SetTrigger("TryBlock");
    }

    private void OnProtagUnsheathe(Protaganist.ProtagSwordState state)
    {
        _animator.SetBool("Sheathed", false);
    }

    private void OnProtagSlice(Protaganist.ProtagSwordState state)
    {
        _animator.SetBool("Sheathed", true);
        _animator.SetTrigger("SuccessfulBlock");
    }

    private void OnProtagTakeDamage()
    {
        _animator.SetTrigger("TakeDamage");
    }
}