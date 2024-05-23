using UnityEngine;
using UnityEngine.Playables;

public class LevelFinishReceiver : MonoBehaviour, INotificationReceiver
{
    [SerializeField]
    private GameLevelSO _levelResultLevel;

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        var jumpMarker = notification as LevelFinishMarker;
        if (jumpMarker == null)
        {
            return;
        }

        LevelLoader.Instance.LoadLevel(_levelResultLevel);
    }
}