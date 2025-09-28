using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager I { get; private set; }
    public event Action<long> OnCoinChanged;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public long Coin => SaveManager.Data.coin;

    public void AddCoin(long v)
    {
        if (v <= 0) return;
        SaveManager.Data.coin += v;
        OnCoinChanged?.Invoke(Coin);
        SaveManager.MarkDirtyAndSave();
    }

    public bool TrySpend(long v)
    {
        if (v <= 0) return true;
        if (SaveManager.Data.coin < v) return false;
        SaveManager.Data.coin -= v;
        OnCoinChanged?.Invoke(Coin);
        SaveManager.MarkDirtyAndSave();
        return true;
    }
}
