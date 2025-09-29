using System;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    // Singleton để WaveManager gọi nhanh
    public static EnemyRegistry I { get; private set; }

    public event Action OnAllCleared;

    private int _alive = 0;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }

    /// <summary>Gọi khi Enemy spawn (EnemyHook.OnEnable)</summary>
    public void Register()
    {
        _alive++;
    }

    /// <summary>Gọi khi Enemy chết hoặc bị disable (EnemyHook.OnDisable)</summary>
    public void Unregister()
    {
        _alive = Mathf.Max(0, _alive - 1);
        if (_alive == 0) OnAllCleared?.Invoke();
    }

    public int AliveCount => _alive;
}
