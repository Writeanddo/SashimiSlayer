using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("BeatMapping/Loop Sync Marker")]
public class LoopSyncMarker : Marker, INotification, INotificationOptionProvider
{
    [SerializeField]
    public bool EmitOnce;

    [SerializeField]
    public bool EmitInEditor;

    public PropertyName id { get; }

    NotificationFlags INotificationOptionProvider.flags =>
        (EmitOnce ? NotificationFlags.TriggerOnce : default) |
        (EmitInEditor ? NotificationFlags.TriggerInEditMode : default);
}