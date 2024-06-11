using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[DisplayName("BeatMapping/Level Finish Marker")]
public class LevelFinishMarker : Marker, INotification, INotificationOptionProvider
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