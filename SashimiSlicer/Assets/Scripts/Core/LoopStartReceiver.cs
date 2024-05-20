using UnityEngine;
using UnityEngine.Playables;

public class LoopStartReceiver : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
    }
}