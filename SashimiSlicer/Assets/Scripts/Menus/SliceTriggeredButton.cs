using Cysharp.Threading.Tasks;
using Events;
using Events.Core;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
///     Generic component for triggering an event when the player slices it
/// </summary>
public class SliceTriggeredButton : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private ProtagSwordStateEvent _protagSliceEvent;

    [Header("Invoking Events")]

    [SerializeField]
    private VoidEvent _buttonSlicedEvent;

    [SerializeField]
    private UnityEvent _buttonSlicedUnityEvent;

    [SerializeField]
    private UnityEvent _buttonSlicedDelayedEvent;

    [Header("Config")]

    [SerializeField]
    private float _radius;

    [SerializeField]
    private bool _singleUse;

    [SerializeField]
    private float _delay;

    private bool _used;

    private void Awake()
    {
        _protagSliceEvent.AddListener(OnProtagSlice);
    }

    private void OnDestroy()
    {
        _protagSliceEvent.RemoveListener(OnProtagSlice);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    private void OnProtagSlice(Protaganist.ProtagSwordState swordState)
    {
        if (_used && _singleUse)
        {
            return;
        }

        float dist = Protaganist.Instance.DistanceToSwordPlane(transform.position);

        if (dist < _radius)
        {
            _used = true;
            Protaganist.Instance.SuccessfulSlice();

            _buttonSlicedUnityEvent?.Invoke();

            TriggerDelayedEvent(_delay).Forget();
        }
    }

    private async UniTaskVoid TriggerDelayedEvent(float delay)
    {
        await UniTask.Delay((int)(delay * 1000));
        _buttonSlicedDelayedEvent?.Invoke();
        if (_buttonSlicedEvent != null)
        {
            _buttonSlicedEvent.Raise();
        }
    }
}