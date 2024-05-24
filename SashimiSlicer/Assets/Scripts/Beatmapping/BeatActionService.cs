using System.Collections.Generic;
using Events.Core;
using UnityEngine;

public class BeatActionService : MonoBehaviour
{
    [Header("Events")]

    [SerializeField]
    private ProtagSwordStateEvent _onBlockByProtag;

    [SerializeField]
    private ProtagSwordStateEvent _onSliceByProtag;

    private readonly List<BnHActionCore> _activeBeatActions = new();

    private void Awake()
    {
        _onBlockByProtag.AddListener(OnBlockByProtag);
        _onSliceByProtag.AddListener(OnSliceByProtag);
    }

    private void Update()
    {
        BnHActionCore[] hits = _activeBeatActions.ToArray();
        foreach (BnHActionCore hit in hits)
        {
            hit.Tick();
        }
    }

    private void OnDestroy()
    {
        _onBlockByProtag.RemoveListener(OnBlockByProtag);
        _onSliceByProtag.RemoveListener(OnSliceByProtag);
    }

    private void OnBlockByProtag(Protaganist.ProtagSwordState swordState)
    {
        foreach (BnHActionCore action in _activeBeatActions)
        {
            action.ApplyPlayerBlock(swordState);
        }
    }

    private void OnSliceByProtag(Protaganist.ProtagSwordState swordState)
    {
        foreach (BnHActionCore action in _activeBeatActions)
        {
            action.ApplyProtagSlice(swordState);
        }
    }

    public BnHActionCore SpawnSimpleHit(BnHActionSo hitConfig, BnHActionCore.BnHActionInstanceConfig data)
    {
        BnHActionCore blockAndHit = Instantiate(hitConfig.Prefab, transform);
        blockAndHit.Setup(hitConfig, data);
        _activeBeatActions.Add(blockAndHit);
        blockAndHit.OnReadyForCleanup += HandleCleanupRequested;
        return blockAndHit;
    }

    public void CleanupBnHHit(BnHActionCore blockAndHit)
    {
        _activeBeatActions.Remove(blockAndHit);
        if (Application.isPlaying)
        {
            Destroy(blockAndHit.gameObject);
        }
        else
        {
            DestroyImmediate(blockAndHit.gameObject);
        }
    }

    private void HandleCleanupRequested(BnHActionCore action)
    {
        action.OnReadyForCleanup -= HandleCleanupRequested;
        CleanupBnHHit(action);
    }
}