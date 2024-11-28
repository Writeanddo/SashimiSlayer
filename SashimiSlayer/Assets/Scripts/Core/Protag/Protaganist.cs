using Cinemachine;
using Events;
using Events.Core;
using Feel;
using UnityEngine;

public class Protaganist : MonoBehaviour
{
    public struct ProtagSwordState
    {
        public SharedTypes.BlockPoseStates BlockPose;
        public SharedTypes.SheathState SheathState;
        public SharedTypes.BlockPoseStates RecentHitBlockPose;
        public Vector3 SwordPosition;
        public float SwordAngle;

        public float DistanceToSwordPlane(Vector3 position)
        {
            Vector3 swordPlaneNormal = Quaternion.Euler(0, 0, SwordAngle) * Vector3.up;
            Vector3 swordPlanePoint = SwordPosition;

            Vector3 pointOnPlane = position - swordPlanePoint;
            float distance = Mathf.Abs(Vector3.Dot(pointOnPlane, swordPlaneNormal));
            return distance;
        }
    }

    [Header("Emitting Events")]

    [SerializeField]
    private FloatEvent _healthChangeEvent;

    [SerializeField]
    private FloatEvent _maxHealthChangeEvent;

    [SerializeField]
    private VoidEvent _damageTakenEvent;

    [SerializeField]
    private ProtagSwordStateEvent _successfulBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _tryBlockEvent;

    [SerializeField]
    private ProtagSwordStateEvent _trySliceEvent;

    [SerializeField]
    private ProtagSwordStateEvent _swordStateChangeEvent;

    [SerializeField]
    private VoidEvent _playerDeadEvent;

    [SerializeField]
    private ProtagSwordStateEvent _unsheatheEvent;

    [SerializeField]
    private ProtagSwordStateEvent _successfulSliceEvent;

    [Header("Listening Events")]

    [SerializeField]
    private BeatmapEvent _beatmapLoadedEvent;

    [SerializeField]
    private VoidEvent _onDrawDebugGuiEvent;

    [SerializeField]
    private Vector2Event _protagSetSwordPivot;

    public static Protaganist Instance { get; private set; }
    public Vector3 SpritePosition { get; set; }

    public Vector3 SwordPosition => _currentSwordState.SwordPosition;

    private BaseUserInputProvider _inputProvider;

    private float _health;
    private float _maxHealth;

    private ProtagSwordState _currentSwordState;

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

        _inputProvider = InputService.Instance;

        _health = _maxHealth;
        _healthChangeEvent.Raise(_health);

        _inputProvider.OnSheathStateChanged += OnSheathStateChanged;
        _inputProvider.OnBlockPoseChanged += OnPoseStateChanged;
        _beatmapLoadedEvent.AddListener(HandleBeatmapLoaded);
        _onDrawDebugGuiEvent.AddListener(HandleDrawDebugGUI);
        _protagSetSwordPivot.AddListener(SetSwordPosition);
    }

    private void Update()
    {
        _currentSwordState.SwordAngle = _inputProvider.GetSwordAngle();
        _swordStateChangeEvent.Raise(_currentSwordState);
    }

    private void OnDestroy()
    {
        _inputProvider.OnSheathStateChanged -= OnSheathStateChanged;
        _inputProvider.OnBlockPoseChanged -= OnPoseStateChanged;
        _beatmapLoadedEvent.RemoveListener(HandleBeatmapLoaded);
        _onDrawDebugGuiEvent.RemoveListener(HandleDrawDebugGUI);
        _protagSetSwordPivot.RemoveListener(SetSwordPosition);
    }

    private void HandleDrawDebugGUI()
    {
        GUILayout.Label($"Sword angle: {_currentSwordState.SwordAngle}");
        GUILayout.Label($"Sword position: {_currentSwordState.SwordPosition}");
        GUILayout.Label($"Sheath state: {_currentSwordState.SheathState}");
        GUILayout.Label($"Pose state: {_inputProvider.GetBlockPose()}");
    }

    private void HandleBeatmapLoaded(BeatmapConfigSo beatmap)
    {
        _maxHealth = beatmap.PlayerMaxHealth;
        _health = _maxHealth;
        _maxHealthChangeEvent.Raise(_maxHealth);
        _healthChangeEvent.Raise(_health);
    }

    private void OnPoseStateChanged(SharedTypes.BlockPoseStates blockPoseStates)
    {
        var oldState = (int)_currentSwordState.BlockPose;
        var newState = (int)blockPoseStates;

        _currentSwordState.BlockPose = blockPoseStates;
        for (var i = 0; i < SharedTypes.NumPoses; i++)
        {
            bool isInOldState = oldState.IsIndexInFlag(i);
            bool isInNewState = newState.IsIndexInFlag(i);
            if (isInNewState && !isInOldState)
            {
                // Only include the most recent hit block pose
                _currentSwordState.BlockPose = (SharedTypes.BlockPoseStates)(1 << i);
                _tryBlockEvent.Raise(_currentSwordState);
            }
        }
    }

    private void OnSheathStateChanged(SharedTypes.SheathState newState)
    {
        SharedTypes.SheathState oldState = _currentSwordState.SheathState;
        _currentSwordState.SheathState = newState;

        // Sword is sheathed from unsheathed means a slice
        if (newState == SharedTypes.SheathState.Sheathed
            && oldState == SharedTypes.SheathState.Unsheathed)
        {
            _trySliceEvent.Raise(_currentSwordState);
        }

        else if (newState == SharedTypes.SheathState.Unsheathed && oldState == SharedTypes.SheathState.Sheathed)
        {
            _unsheatheEvent.Raise(_currentSwordState);
        }
    }

    public void SetSwordPosition(Vector2 position)
    {
        _currentSwordState.SwordPosition = position;
    }

    public void TakeDamage(float damage)
    {
        ScreenShakeService.Instance.ShakeScreen(0.1f, 1f, CinemachineImpulseDefinition.ImpulseShapes.Rumble);

        if (_health <= 0)
        {
            return;
        }

        _health -= damage;
        _healthChangeEvent.Raise(_health);
        _damageTakenEvent.Raise();

        if (_health <= 0)
        {
            _playerDeadEvent.Raise();
        }
    }

    public void SuccessfulBlock()
    {
        _successfulBlockEvent.Raise(_currentSwordState);
        ScreenShakeService.Instance.ShakeScreen(0.05f, 0.15f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SuccessfulSlice()
    {
        _successfulSliceEvent.Raise(_currentSwordState);
        ScreenShakeService.Instance.ShakeScreen(0.1f, 0.5f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SetSlashPosition(Vector3 position)
    {
        _currentSwordState.SwordPosition = position;
        _swordStateChangeEvent.Raise(_currentSwordState);
    }

    public float DistanceToSwordPlane(Vector2 pos)
    {
        return _currentSwordState.DistanceToSwordPlane(pos);
    }
}