using Cinemachine;
using UnityEngine;

public class ScreenShakeService : MonoBehaviour
{
    [SerializeField]
    private CinemachineImpulseSource _impulseSource;

    public static ScreenShakeService Instance { get; private set; }

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

    public void ShakeScreen(float duration, float magnitude, CinemachineImpulseDefinition.ImpulseShapes shapes)
    {
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
        _impulseSource.m_ImpulseDefinition.m_ImpulseShape = shapes;
        _impulseSource.GenerateImpulse(magnitude);
    }
}