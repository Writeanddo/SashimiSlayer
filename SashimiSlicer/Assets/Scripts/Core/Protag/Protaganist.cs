using System;
using Cinemachine;
using Input;
using UnityEngine;

public class Protaganist : MonoBehaviour
{
    public struct BlockPose
    {
        public BaseUserInputProvider.PoseState pose;
        public Vector3 swordPosition;
        public float swordAngle;
    }

    public struct AttackPose
    {
        public Vector3 swordPosition;
        public float swordAngle;
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
    public BaseUserInputProvider.SheathState SheathState => _sheathState;
    public Vector3 SpritePosition { get; set; }

    public event Action<BlockPose> OnBlockAction;
    public event Action OnDamageTaken;
    public event Action<AttackPose> OnSliceAction;
    public event Action OnSuccessfulBlock;

    private float _health;

    private BaseUserInputProvider.SheathState _sheathState;
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
        _inputProvider.OnPoseStateChanged += OnPoseStateChanged;
    }

    private void Update()
    {
        _swordAngle = _inputProvider.GetSwordAngle();
        _swordIndicator.SetAngle(_swordAngle);
    }

    private void OnDestroy()
    {
        _inputProvider.OnSheathStateChanged -= OnSheathStateChanged;
        _inputProvider.OnPoseStateChanged -= OnPoseStateChanged;
    }

    private void OnGUI()
    {
        GUILayout.Label($"Sword angle: {_swordAngle}");
        GUILayout.Label($"Sword position: {_swordPosition}");
        GUILayout.Label($"Sheath state: {_sheathState}");
        GUILayout.Label($"Pose state: {_inputProvider.GetPoseState()}");
    }

    private void OnPoseStateChanged(BaseUserInputProvider.PoseState poseState)
    {
        var pose = new BlockPose
        {
            pose = poseState,
            swordPosition = _swordPosition,
            swordAngle = _swordAngle
        };
        OnBlockAction?.Invoke(pose);
    }

    private void OnSheathStateChanged(BaseUserInputProvider.SheathState newState)
    {
        Debug.Log($"Sheath state changed to {newState}");
        BaseUserInputProvider.SheathState oldState = _sheathState;
        _sheathState = newState;

        // Sword is sheathed from unsheathed means a slice
        if (newState == BaseUserInputProvider.SheathState.Sheathed
            && oldState == BaseUserInputProvider.SheathState.Unsheathed)
        {
            OnSliceAction?.Invoke(new AttackPose
            {
                swordPosition = _swordPosition,
                swordAngle = _swordAngle
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
        Debug.Log("Successful slice");
        ScreenShakeService.Instance.ShakeScreen(0.1f, 0.5f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
    }

    public void SetSlashPosition(Vector3 position)
    {
        _swordIndicator.SetPosition(position);
        _swordPosition = position;
    }
}