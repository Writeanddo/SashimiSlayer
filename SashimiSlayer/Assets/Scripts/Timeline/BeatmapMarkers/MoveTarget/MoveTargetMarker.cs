using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("BeatMapping/Move Target Marker")]
public class MoveTargetMarker : Marker, INotification, INotificationOptionProvider
{
    [SerializeField]
    public bool EmitOnce;

    [SerializeField]
    public bool EmitInEditor;

    [SerializeField]
    public Vector3 Position;

    public PropertyName id { get; }

    NotificationFlags INotificationOptionProvider.flags =>
        (EmitOnce ? NotificationFlags.TriggerOnce : default) |
        (EmitInEditor ? NotificationFlags.TriggerInEditMode : default);
}