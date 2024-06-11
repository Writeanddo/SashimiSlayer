using System.Collections.Generic;
using Events;
using UnityEngine;

public class DiscreteHealthScript : MonoBehaviour
{
    [SerializeField]
    private DiscreteHealthInstance _healthPrefab;

    [SerializeField]
    private Transform _layoutGroup;

    [SerializeField]
    private FloatEvent _healthChangeEvent;

    [SerializeField]
    private FloatEvent _maxHealthChangeEvent;

    private readonly List<DiscreteHealthInstance> _healthUIs = new();

    private void Awake()
    {
        _healthChangeEvent.AddListener(UpdateHealth);
        _maxHealthChangeEvent.AddListener(InitializeHealth);
    }

    private void OnDestroy()
    {
        _healthChangeEvent.RemoveListener(UpdateHealth);
        _maxHealthChangeEvent.RemoveListener(InitializeHealth);
    }

    private void InitializeHealth(float maxHealth)
    {
        for (var i = 0; i < maxHealth; i++)
        {
            _healthUIs.Add(Instantiate(_healthPrefab, _layoutGroup));
        }
    }

    private void UpdateHealth(float newHealth)
    {
        for (var i = 0; i < _healthUIs.Count; i++)
        {
            _healthUIs[i].SetFilled(i < newHealth);
        }
    }
}