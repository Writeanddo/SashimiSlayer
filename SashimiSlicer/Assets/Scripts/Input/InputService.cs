using Input;
using UnityEngine;

public class InputService : MonoBehaviour
{
    [SerializeField]
    private BaseUserInputProvider _gamepadInputProvider;

    public static InputService Instance { get; private set; }

    public BaseUserInputProvider InputProvider => _gamepadInputProvider;

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