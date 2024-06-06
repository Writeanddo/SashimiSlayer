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
    private ParticleSystem _sliceParticle;

    [Header("Events")]

    [SerializeField]
    private SOEvent _protagSuccessfulSliceEvent;

    [SerializeField]
    private ProtagSwordStateEvent _onSwordStateChange;

    private float _angle;

    private Vector3 _position;

    private Vector3 _cPos;

    private void Awake()
    {
        _onSwordStateChange.AddListener(OnSwordStateChange);
        SetSheatheState(SharedTypes.SheathState.Sheathed);
        _protagSuccessfulSliceEvent.AddListener(OnSuccessfulSlice);
    }

    private void Update()
    {
        _cPos = Vector3.Lerp(_cPos, _position, Time.deltaTime * 10f);
        UpdateOrientation(_sheathedLineRen);
        UpdateOrientation(_unsheathedLineRen);
    }

    private void OnDestroy()
    {
        _onSwordStateChange.RemoveListener(OnSwordStateChange);
        _protagSuccessfulSliceEvent.RemoveListener(OnSuccessfulSlice);
    }

    private void OnSuccessfulSlice()
    {
        _sliceParticle.transform.position = _cPos;
        ParticleSystem.MainModule main = _sliceParticle.main;
        main.startRotation = -_angle * Mathf.Deg2Rad;
        _sliceParticle.transform.rotation = Quaternion.Euler(0, 0, _angle);
        _sliceParticle.Play();
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

    private void SetPosition(Vector3 position)
    {
        _position = position;
    }

    private void UpdateOrientation(LineRenderer lineRen)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, _angle);
        lineRen.SetPosition(0, _cPos + rotation * Vector3.left * 1000f);
        lineRen.SetPosition(1, _cPos + rotation * Vector3.right * 1000f);
    }
}