using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("BeatMapping/Move Target Marker")]
public class MoveTargetMarker : Marker, INotification, INotificationOptionProvider
{
    [SerializeField]
    public bool emitOnce;

    [SerializeField]
    public bool emitInEditor;

    [SerializeField]
    public Vector3 position;

    public PropertyName id { get; }

    NotificationFlags INotificationOptionProvider.flags =>
        (emitOnce ? NotificationFlags.TriggerOnce : default) |
        (emitInEditor ? NotificationFlags.TriggerInEditMode : default);
}