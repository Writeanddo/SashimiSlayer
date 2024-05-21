using UnityEngine;

public class SwordIndicator : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private Material _idleMaterial;

    [SerializeField]
    private Material _unsheathedMaterial;

    private float _angle;

    private Vector3 _position;

    public void SetSheatheState(Gameplay.SheathState state)
    {
        _lineRenderer.material =
            state == Gameplay.SheathState.Sheathed ? _idleMaterial : _unsheathedMaterial;
    }

    public void SetAngle(float angle)
    {
        _angle = angle;
        UpdateOrientation();
    }

    public void SetPosition(Vector3 position)
    {
        _position = position;
        UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        Quaternion rotation = Quaternion.Euler(0, 0, _angle);
        _lineRenderer.SetPosition(0, _position + rotation * Vector3.left * 1000f);
        _lineRenderer.SetPosition(1, _position + rotation * Vector3.right * 1000f);
    }
}