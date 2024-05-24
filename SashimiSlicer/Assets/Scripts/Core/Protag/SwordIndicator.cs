using Events.Core;
using UnityEngine;

public class SwordIndicator : MonoBehaviour
{
    [Header("Visuals")]

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private Material _idleMaterial;

    [SerializeField]
    private Material _unsheathedMaterial;

    [Header("Events")]

    [SerializeField]
    private ProtagSwordStateEvent _onSwordStateChange;

    private float _angle;

    private Vector3 _position;

    private Vector3 _cPos;

    private void Awake()
    {
        _onSwordStateChange.AddListener(OnSwordStateChange);
        SetSheatheState(SharedTypes.SheathState.Sheathed);
    }

    private void Update()
    {
        _cPos = Vector3.Lerp(_cPos, _position, Time.deltaTime * 10f);
        UpdateOrientation();
    }

    private void OnDestroy()
    {
        _onSwordStateChange.RemoveListener(OnSwordStateChange);
    }

    private void OnSwordStateChange(Protaganist.ProtagSwordState swordState)
    {
        SetSheatheState(swordState.SheathState);
        SetAngle(swordState.SwordAngle);
        SetPosition(swordState.SwordPosition);
    }

    private void SetSheatheState(SharedTypes.SheathState state)
    {
        _lineRenderer.material =
            state == SharedTypes.SheathState.Sheathed ? _idleMaterial : _unsheathedMaterial;

        if (state == SharedTypes.SheathState.Sheathed)
        {
            _lineRenderer.widthMultiplier = 0.01f;
        }
        else
        {
            _lineRenderer.widthMultiplier = 0.04f;
        }
    }

    private void SetAngle(float angle)
    {
        _angle = angle;
    }

    private void SetPosition(Vector3 position)
    {
        _position = position;
    }

    private void UpdateOrientation()
    {
        Quaternion rotation = Quaternion.Euler(0, 0, _angle);
        _lineRenderer.SetPosition(0, _cPos + rotation * Vector3.left * 1000f);
        _lineRenderer.SetPosition(1, _cPos + rotation * Vector3.right * 1000f);
    }
}