using UnityEngine;

public class DiscreteHealthInstance : MonoBehaviour
{
    [SerializeField]
    private GameObject _filled;

    public void SetFilled(bool filled)
    {
        _filled.SetActive(filled);
    }
}