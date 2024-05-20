using System;
using UnityEngine;

public class EventService : MonoBehaviour
{
    public static EventService Instance { get; private set; }

    #region Public events

    public event Action<float> OnPlayerHealthChange;

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}