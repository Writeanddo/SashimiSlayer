using Events;
using Events.Core;
using UnityEngine;
using UnityEngine.Events;

public class SliceTriggeredButton : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private ProtagSwordStateEvent _protagSliceEvent;

    [Header("Invoking Events")]

    [SerializeField]
    private VoidEvent _buttonSlicedEvent;

    [SerializeField]
    public UnityEvent _buttonSlicedUnityEvent;

    [Header("Config")]

    [SerializeField]
    private float _radius;

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
        float dist = Protaganist.Instance.DistanceToSwordPlane(transform.position);

        if (dist < _radius)
        {
            _buttonSlicedEvent.Raise();
            _buttonSlicedUnityEvent.Invoke();
        }
    }
}