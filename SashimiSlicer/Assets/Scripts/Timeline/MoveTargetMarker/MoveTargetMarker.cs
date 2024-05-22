using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[DisplayName("BeatMapping/Move Target Marker")]
public class MoveTargetMarker : Marker, INotification, INotificationOptionProvider
{
    [FormerlySerializedAs("emitOnce")]
    [SerializeField]
    public bool EmitOnce;

    [FormerlySerializedAs("emitInEditor")]
    [SerializeField]
    public bool EmitInEditor;

    [FormerlySerializedAs("position")]
    [SerializeField]
    public Vector3 Position;

    public PropertyName id { get; }

    NotificationFlags INotificationOptionProvider.flags =>
        (EmitOnce ? NotificationFlags.TriggerOnce : default) |
        (EmitInEditor ? NotificationFlags.TriggerInEditMode : default);
}