using Events;
using Events.Core;
using UnityEngine;

public class ProtagAnimation : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private ProtagSwordStateEvent _protagBlockEvent;

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
        _protagBlockEvent.AddListener(OnProtagBlock);
        _protagUnsheatheEvent.AddListener(OnProtagUnsheathe);
        _protagSliceEvent.AddListener(OnProtagSlice);
        _protagTakeDamageEvent.AddListener(OnProtagTakeDamage);
    }

    private void OnDestroy()
    {
        _protagBlockEvent.RemoveListener(OnProtagBlock);
        _protagUnsheatheEvent.RemoveListener(OnProtagUnsheathe);
        _protagSliceEvent.RemoveListener(OnProtagSlice);
        _protagTakeDamageEvent.RemoveListener(OnProtagTakeDamage);
    }

    private void OnProtagBlock(Protaganist.ProtagSwordState state)
    {
        _animator.Play("ProtagParry");
    }

    private void OnProtagUnsheathe(Protaganist.ProtagSwordState state)
    {
        _animator.Play("ProtagParry");
    }

    private void OnProtagSlice(Protaganist.ProtagSwordState state)
    {
        _animator.Play("ProtagParry");
    }

    private void OnProtagTakeDamage()
    {
        _animator.Play("ProtagHurt");
    }
}