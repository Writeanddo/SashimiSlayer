using UnityEngine;
using UnityEngine.Playables;

public class MoveTargetMarkerReceiver : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        var jumpMarker = notification as MoveTargetMarker;
        if (jumpMarker == null)
        {
            return;
        }

        Protaganist.Instance.SetSlashPosition(jumpMarker.Position);
    }
}