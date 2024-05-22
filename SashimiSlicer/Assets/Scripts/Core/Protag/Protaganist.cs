using System;
using Cinemachine;
using Input;
using UnityEngine;

public class Protaganist : MonoBehaviour
{
    public struct BlockInstance
    {
        public Gameplay.BlockPoseStates BlockPose;
        public Vector3 SwordPosition;
        public float SwordAngle;
    }

    public struct AttackInstance
    {
        public Vector3 SwordPosition;
        public float SwordAngle;
    }

    [SerializeField]
    private BaseUserInputProvider _inputProvider;

    [SerializeField]
    private SwordIndicator _swordIndicator;

    [SerializeField]
    private HealthbarScript _healthbar;

    [SerializeField]
    private float _maxHealth;

    public static Protaganist Instance { get; private set; }
    public Gameplay.SheathState ProtagSheathState => _protagSheathState;
    public Vector3 SpritePosition { get; set; }

    public event Action<BlockInstance> OnBlockAction;
    public event Action OnDamageTaken;
    public event Action<AttackInstance> OnSliceAction;
    public event Action OnSuccessfulBlock;

    private float _health;

    private Gameplay.SheathState _protagSheathState;
    private float _swordAngle;

    private Vector3 _swordPosition;

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

        _healthbar.InitializeBar(_maxHealth);
        _health = _maxHealth;

        _inputProvider.OnSheathStateChanged += OnSheathStateChanged;
        _inputProvider.OnBlockPoseChanged += OnPoseStateChanged;
    }

    private void Update()
    {
        _swordAngle = _inputProvider.GetSwordAngle();
        _swordIndicator.SetAngle(_swordAngle);
    }

    private void OnDestroy()
    {
        _inputProvider.OnSheathStateChanged -= OnSheathStateChanged;
        _inputProvider.OnBlockPoseChanged -= OnPoseStateChanged;
    }

    private void OnGUI()
    {
        GUILayout.Label($"Sword angle: {_swordAngle}");
        GUILayout.Label($"Sword position: {_swordPosition}");
        GUILayout.Label($"Sheath state: {_protagSheathState}");
        GUILayout.Label($"Pose state: {_inputProvider.GetBlockPose()}");
    }

    private void OnPoseStateChanged(Gameplay.BlockPoseStates blockPoseStates)
    {
        var pose = new BlockInstance
        {
            BlockPose = blockPoseStates,
            SwordPosition = _swordPosition,
            SwordAngle = _swordAngle
        };
        OnBlockAction?.Invoke(pose);
    }

    private void OnSheathStateChanged(Gameplay.SheathState newState)
    {
        Gameplay.SheathState oldState = _protagSheathState;
        _protagSheathState = newState;

        // Sword is sheathed from unsheathed means a slice
        if (newState == Gameplay.SheathState.Sheathed
            && oldState == Gameplay.SheathState.Unsheathed)
        {
            OnSliceAction?.Invoke(new AttackInstance
            {
                SwordPosition = _swordPosition,
                SwordAngle = _swordAngle
            });
        }

        _swordIndicator.SetSheatheState(newState);
    }

    public void SetSwordPosition(Vector3 position)
    {
        _swordPosition = position;
    }

    public float DistanceToSwordPlane(Vector3 position)
    {
        Vector3 swordPlaneNormal = Quaternion.Euler(0, 0, _swordAngle) * Vector3.up;
        Vector3 swordPlanePoint = _swordPosition;

        Vector3 pointOnPlane = position - swordPlanePoint;
        float distance = Mathf.Abs(Vector3.Dot(pointOnPlane, swordPlaneNormal));
        return distance;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        _healthbar.TakeDamage(_health);
        OnDamageTaken?.Invoke();
        ScreenShakeService.Instance.ShakeScreen(0.1f, 1f, CinemachineImpulseDefinition.ImpulseShapes.Rumble);
    }

    public void SuccessfulBlock()
    {
        OnSuccessfulBlock?.Invoke();
        ScreenShakeService.Instance.ShakeScreen(0.05f, 0.15f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SuccessfulSlice()
    {
        ScreenShakeService.Instance.ShakeScreen(0.1f, 0.5f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SetSlashPosition(Vector3 position)
    {
        _swordIndicator.SetPosition(position);
        _swordPosition = position;
    }
}