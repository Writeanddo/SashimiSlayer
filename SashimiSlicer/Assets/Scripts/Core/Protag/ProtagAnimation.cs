using Events;
using UnityEngine;

public class ProtagAnimation : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private SOEvent _protagSuccessfulBlockEvent;

    [SerializeField]
    private SOEvent _protagTryBlockEvent;

    [SerializeField]
    private SOEvent _protagUnsheatheEvent;

    [SerializeField]
    private SOEvent _protagTrySliceEvent;

    [SerializeField]
    private SOEvent _protagSuccessfulSliceEvent;

    [SerializeField]
    private VoidEvent _protagTakeDamageEvent;

    [SerializeField]
    private SOEvent _protagVictoryEvent;

    [SerializeField]
    private SOEvent _protagLossEvent;

    [SerializeField]
    private Animator _animator;

    private void Awake()
    {
        _protagSuccessfulBlockEvent.AddListener(OnProtagSuccessfulBlock);
        _protagTryBlockEvent.AddListener(OnProtagTryBlock);

        _protagUnsheatheEvent.AddListener(OnProtagUnsheathe);

        _protagTrySliceEvent.AddListener(OnProtagTrySlice);
        _protagSuccessfulSliceEvent.AddListener(OnProtagSuccessfulSlice);

        _protagTakeDamageEvent.AddListener(OnProtagTakeDamage);

        _protagVictoryEvent.AddListener(OnProtagVictory);
        _protagLossEvent.AddListener(OnProtagLoss);
    }

    private void OnDestroy()
    {
        _protagSuccessfulBlockEvent.RemoveListener(OnProtagSuccessfulBlock);
        _protagTryBlockEvent.RemoveListener(OnProtagTryBlock);

        _protagUnsheatheEvent.RemoveListener(OnProtagUnsheathe);

        _protagTrySliceEvent.RemoveListener(OnProtagTrySlice);
        _protagSuccessfulSliceEvent.RemoveListener(OnProtagSuccessfulSlice);

        _protagTakeDamageEvent.RemoveListener(OnProtagTakeDamage);

        _protagVictoryEvent.RemoveListener(OnProtagVictory);
        _protagLossEvent.RemoveListener(OnProtagLoss);
    }

    private void OnProtagVictory()
    {
        _animator.Play("ProtagVictory", 0, 0);
    }

    private void OnProtagLoss()
    {
        _animator.Play("ProtagLoss", 0, 0);
    }

    private void OnProtagSuccessfulBlock()
    {
        _animator.SetTrigger("SuccessfulBlock");
    }

    private void OnProtagTryBlock()
    {
        _animator.SetTrigger("TryBlock");
    }

    private void OnProtagUnsheathe()
    {
        _animator.SetBool("Sheathed", false);
    }

    private void OnProtagTrySlice()
    {
        _animator.SetBool("Sheathed", true);
        _animator.SetTrigger("TrySlice");
    }

    private void OnProtagSuccessfulSlice()
    {
        _animator.SetTrigger("SuccessfulSlice");
    }

    private void OnProtagTakeDamage()
    {
        _animator.SetTrigger("TakeDamage");
    }
}