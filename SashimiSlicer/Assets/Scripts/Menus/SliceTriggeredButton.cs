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

    [SerializeField]
    private ProtagSwordStateEvent _protagSwordStateEvent;

    [Header("Invoking Events")]

    [SerializeField]
    private VoidEvent _buttonSlicedEvent;

    [SerializeField]
    private UnityEvent _buttonSlicedUnityEvent;

    [SerializeField]
    private UnityEvent _buttonSlicedDelayedEvent;

    [SerializeField]
    private UnityEvent _buttonHoveredEvent;

    [SerializeField]
    private UnityEvent _buttonUnhoveredEvent;

    [Header("Config")]

    [SerializeField]
    private float _radius;

    [SerializeField]
    private bool _singleUse;

    [SerializeField]
    private float _delay;

    [SerializeField]
    private bool _isInCanvasSpace;

    private bool _used;

    private bool _hovered;

    private void Awake()
    {
        _protagSliceEvent.AddListener(OnProtagSlice);
        _protagSwordStateEvent.AddListener(OnProtagSwordState);
    }

    private void OnDestroy()
    {
        _protagSliceEvent.RemoveListener(OnProtagSlice);
        _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    private void OnProtagSwordState(Protaganist.ProtagSwordState obj)
    {
        Vector2 pos = transform.position;
        if (_isInCanvasSpace)
        {
            pos = Camera.main.ScreenToWorldPoint(pos);
        }

        float dist = Protaganist.Instance.DistanceToSwordPlane(pos);

        if (dist < _radius)
        {
            _hovered = true;
            _buttonHoveredEvent?.Invoke();
        }
        else
        {
            _hovered = false;
            _buttonUnhoveredEvent?.Invoke();
        }
    }

    private void OnProtagSlice(Protaganist.ProtagSwordState swordState)
    {
        if (_used && _singleUse)
        {
            return;
        }

        if (_hovered)
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