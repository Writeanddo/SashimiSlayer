using System.Collections.Generic;
using Events;
using Events.Core;
using UnityEngine;

public class SwordIndicator : MonoBehaviour
{
    [Header("Visuals")]

    [SerializeField]
    private LineRenderer _sheathedLineRen;

    [SerializeField]
    private LineRenderer _unsheathedLineRen;

    [SerializeField]
    private ParticleSystem[] _sliceParticles;

    [Header("Events")]

    [SerializeField]
    private SOEvent _protagSuccessfulSliceEvent;

    [SerializeField]
    private ProtagSwordStateEvent _onSwordStateChange;

    [SerializeField]
    private Vector2Event _swordPivotPositionChangeEvent;

    private readonly List<ParticleSystem.MinMaxCurve> _initialParticleRot = new();

    private float _angle;

    private Vector3 _position;

    private Vector3 _cPos;

    private void Awake()
    {
        _onSwordStateChange.AddListener(OnSwordStateChange);
        SetSheatheState(SharedTypes.SheathState.Sheathed);
        _protagSuccessfulSliceEvent.AddListener(OnSuccessfulSlice);
        _swordPivotPositionChangeEvent.AddListener(SetPosition);

        foreach (ParticleSystem particle in _sliceParticles)
        {
            _initialParticleRot.Add(particle.main.startRotation);
        }
    }

    private void Update()
    {
        _cPos = _position;
        UpdateOrientation(_sheathedLineRen);
        UpdateOrientation(_unsheathedLineRen);
    }

    private void OnDestroy()
    {
        _onSwordStateChange.RemoveListener(OnSwordStateChange);
        _protagSuccessfulSliceEvent.RemoveListener(OnSuccessfulSlice);
        _swordPivotPositionChangeEvent.RemoveListener(SetPosition);
    }

    private void OnSuccessfulSlice()
    {
        for (var i = 0; i < _sliceParticles.Length; i++)
        {
            ParticleSystem particle = _sliceParticles[i];
            // particle.transform.position = _cPos;
            ParticleSystem.MinMaxCurve curve = _initialParticleRot[i];
            curve.constantMin += -_angle * Mathf.Deg2Rad;
            curve.constantMax += -_angle * Mathf.Deg2Rad;

            ParticleSystem.MainModule main = particle.main;
            main.startRotation = curve;

            particle.transform.rotation = Quaternion.Euler(0, 0, _angle);
            particle.Play();
        }
    }

    private void OnSwordStateChange(Protaganist.ProtagSwordState swordState)
    {
        SetSheatheState(swordState.SheathState);
        SetAngle(swordState.SwordAngle);
        SetPosition(swordState.SwordPosition);
    }

    private void SetSheatheState(SharedTypes.SheathState state)
    {
        _sheathedLineRen.enabled = state == SharedTypes.SheathState.Sheathed;
        _unsheathedLineRen.enabled = state == SharedTypes.SheathState.Unsheathed;
    }

    private void SetAngle(float angle)
    {
        _angle = angle;
    }

    private void SetPosition(Vector2 position)
    {
        _position = position;
    }

    private void UpdateOrientation(LineRenderer lineRen)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, _angle);
        lineRen.positionCount = 3;
        lineRen.SetPosition(0, _cPos + rotation * Vector3.left * 25f);
        lineRen.SetPosition(1, _cPos);
        lineRen.SetPosition(2, _cPos + rotation * Vector3.right * 25f);
    }
}