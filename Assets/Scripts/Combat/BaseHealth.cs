using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [Header("Config")]
    public long baseHp = 500;
    public long hpPerWave = 40; // gợi ý công thức: 500 + wave×40

    [Header("Runtime (read only)")]
    [SerializeField] private long currentHp;
    public event Action OnBaseDead;

    void Start()
    {
        var wave = Mathf.Max(0, SaveManager.Data.waveIndex);
        currentHp = baseHp + hpPerWave * wave;
    }

    public long CurrentHp => currentHp;

    public void TakeDamage(long dmg)
    {
        if (currentHp <= 0) return;
        currentHp = Math.Max(0, currentHp - Math.Max(0, dmg));
        if (currentHp == 0) OnBaseDead?.Invoke();
    }
}
