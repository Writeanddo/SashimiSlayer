using UnityEngine;
using UnityEngine.Playables;

public class LoopSyncMarkerReceiver : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        var jumpMarker = notification as LoopSyncMarker;
        if (jumpMarker == null)
        {
            return;
        }

        TimingService.Instance.Resync();
    }
}