using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[DisplayName("BeatMapping/Loop Sync Marker")]
public class LoopSyncMarker : Marker, INotification, INotificationOptionProvider
{
    [FormerlySerializedAs("emitOnce")]
    [SerializeField]
    public bool EmitOnce;

    [FormerlySerializedAs("emitInEditor")]
    [SerializeField]
    public bool EmitInEditor;

    public PropertyName id { get; }

    NotificationFlags INotificationOptionProvider.flags =>
        (EmitOnce ? NotificationFlags.TriggerOnce : default) |
        (EmitInEditor ? NotificationFlags.TriggerInEditMode : default);
}