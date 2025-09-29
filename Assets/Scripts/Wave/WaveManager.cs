// Assets/Scripts/Wave/WaveManager.cs
using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Refs")]
    public BaseHealth baseHealth;
    public WaveSpawner spawner;
    public ProgressionCurveSO progression; // để tính thưởng cơ bản :contentReference[oaicite:7]{index=7}

    [Header("Definitions")]
    public WaveDefinitionSO[] presets; // Wave 1-20 có thể preset tay
    public bool autoStart = true;

    [Header("Runtime (read-only)")]
    [SerializeField] private WaveState state = WaveState.Prepare;
    [SerializeField] private int currentWaveIndex;
    [SerializeField] private bool lastWin;

    public event Action OnWin;
    public event Action OnFail;

    void Awake()
    {
        // Khởi động từ Save
        currentWaveIndex = Mathf.Max(1, SaveManager.Data.waveIndex + 1);
    }

    void OnEnable()
    {
        if (baseHealth) baseHealth.OnBaseDead += HandleBaseDead;
        if (EnemyRegistry.I) EnemyRegistry.I.OnAllCleared += HandleAllCleared; // win khi hết enemy :contentReference[oaicite:8]{index=8}
    }

    void OnDisable()
    {
        if (baseHealth) baseHealth.OnBaseDead -= HandleBaseDead;
        if (EnemyRegistry.I) EnemyRegistry.I.OnAllCleared -= HandleAllCleared;
    }

    void Start()
    {
        if (autoStart || SaveManager.Data.autoStart) GoPrepare();
    }

    public void GoPrepare()
    {
        state = WaveState.Prepare;
        lastWin = false;
        // Tại Prepare bạn có thể mở UI nâng cấp/mua gà...
        if (autoStart || SaveManager.Data.autoStart) StartCombat();
    }

    public void StartCombat()
    {
        if (state != WaveState.Prepare) return;
        state = WaveState.Combat;

        // Lấy wave def (ưu tiên preset)
        var def = GetWaveDefinition(currentWaveIndex);
        if (!def) { Debug.LogWarning($"[WaveManager] Missing Wave {currentWaveIndex} def"); }
        spawner.Begin(def);

        // Reset/scale Base HP theo wave đã có trong BaseHealth (đang tính bằng base + hpPerWave*wave) :contentReference[oaicite:9]{index=9}
    }

    WaveDefinitionSO GetWaveDefinition(int waveIndex)
    {
        // 1–20: dùng preset nếu có
        if (presets != null && waveIndex - 1 < presets.Length && waveIndex - 1 >= 0)
            return presets[waveIndex - 1];

        // ≥21: về sau gắn RuntimeWaveGenerator (Sprint 6)
        return presets != null && presets.Length > 0 ? presets[Mathf.Clamp(presets.Length - 1, 0, presets.Length - 1)] : null;
    }

    void HandleAllCleared()
    {
        if (state != WaveState.Combat) return;
        if (!spawner.IsFinishedSpawning) return; // chưa spawn xong mà hết là do timing, chờ tiếp
        // WIN
        lastWin = true;
        GrantRewards(currentWaveIndex);
        AdvanceWaveIndex();
        state = WaveState.Result;
        OnWin?.Invoke();
        // Mở UI Result Win (Claim). Sau Claim → GoPrepare();
    }

    void HandleBaseDead()
    {
        if (state != WaveState.Combat) return;
        // FAIL
        lastWin = false;
        // Không cộng WaveReward, chỉ giữ coin rơi trong combat (đã AddCoin khi enemy chết)
        state = WaveState.Result;
        OnFail?.Invoke();
        // UI Result Fail → Retry (replay wave) hoặc Back
    }

    void AdvanceWaveIndex()
    {
        SaveManager.Data.waveIndex = Mathf.Max(SaveManager.Data.waveIndex, currentWaveIndex);
        SaveManager.MarkDirtyAndSave();
        currentWaveIndex = SaveManager.Data.waveIndex + 1;
    }

    void GrantRewards(int waveIndex)
    {
        if (!progression) return;
        // Công thức gợi ý: WaveReward = 1000 * (waveRewardGrowth^waveIndex)
        double baseReward = 1000.0 * Math.Pow(progression.waveRewardGrowth, waveIndex);
        long reward = (long)Math.Round(baseReward);
        EconomyManager.I.AddCoin(reward); // EconomyManager đã có sẵn :contentReference[oaicite:10]{index=10}
    }

    // Public API cho UI
    public WaveState State => state;
    public int CurrentWave => currentWaveIndex;
    public bool LastWin => lastWin;

    public void ClaimAndContinue()
    {
        if (state != WaveState.Result) return;
        GoPrepare();
    }

    public void Retry()
    {
        if (state != WaveState.Result) return;
        // Retry lại cùng waveIndex hiện tại (không advance)
        state = WaveState.Prepare;
        StartCombat();
    }
}
