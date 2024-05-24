using Events;
using UnityEngine;

public class DebugService : MonoBehaviour
{
    [SerializeField]
    private bool _showGuiLabel;

    [SerializeField]
    private VoidEvent _onDrawGuiEvent;

    public static DebugService Instance { get; private set; }

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _showGuiLabel = !_showGuiLabel;
        }
    }

    private void OnGUI()
    {
        if (!_showGuiLabel)
        {
            return;
        }

        _onDrawGuiEvent.Raise();
    }
}