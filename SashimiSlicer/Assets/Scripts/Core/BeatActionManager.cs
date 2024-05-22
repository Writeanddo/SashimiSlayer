using System.Collections.Generic;
using UnityEngine;

public class BeatActionManager : MonoBehaviour
{
    [SerializeField]
    private Transform _simpleHitParent;

    public static BeatActionManager Instance { get; private set; }

    private readonly List<BnHActionCore> _simpleHits = new();

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
        BnHActionCore[] hits = _simpleHits.ToArray();
        foreach (BnHActionCore hit in hits)
        {
            hit.Tick();
        }
    }

    public BnHActionCore SpawnSimpleHit(BnHActionSo hitConfig, BnHActionCore.BnHActionInstanceConfig data)
    {
        BnHActionCore blockAndHit = Instantiate(hitConfig.Prefab, _simpleHitParent);
        blockAndHit.Setup(hitConfig, data);
        _simpleHits.Add(blockAndHit);
        return blockAndHit;
    }

    public void CleanupBnHHit(BnHActionCore blockAndHit)
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