using System.Collections.Generic;
using UnityEngine;

public class BeatActionManager : MonoBehaviour
{
    [SerializeField]
    private Transform _simpleHitParent;

    public static BeatActionManager Instance { get; private set; }

    private readonly List<BaseBnHAction> _simpleHits = new();

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

    private void Update()
    {
        BaseBnHAction[] hits = _simpleHits.ToArray();
        foreach (BaseBnHAction hit in hits)
        {
            hit.Tick();
        }
    }

    public BaseBnHAction SpawnSimpleHit(BnHActionSO hitConfig, BaseBnHAction.BnHActionInstance data)
    {
        BaseBnHAction blockAndHit = Instantiate(hitConfig.Prefab, _simpleHitParent);
        blockAndHit.Setup(hitConfig, data);
        _simpleHits.Add(blockAndHit);
        return blockAndHit;
    }

    public void CleanupBnHHit(BaseBnHAction blockAndHit)
    {
        _simpleHits.Remove(blockAndHit);
        if (Application.isPlaying)
        {
            Destroy(blockAndHit.gameObject);
        }
        else
        {
            DestroyImmediate(blockAndHit.gameObject);
        }
    }
}