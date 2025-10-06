// Assets/Scripts/Wave/WaveManager.cs
using CR.Wave;
using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Refs")]
    public BaseHealth baseHealth;
    public WaveSpawner spawner;
    public ProgressionCurveSO progression; // để tính thưởng cơ bản

    [Header("Definitions")]
    public WaveDefinitionSO[] presets; // Wave 1-20 có thể preset tay
    public bool autoStart = true;

    [Header("Runtime (read-only)")]
    [SerializeField] private WaveState state = WaveState.Prepare;
    [SerializeField] private int currentWaveIndex; // wave đang sắp chơi (1-based)
    [SerializeField] private bool lastWin;

    public event Action OnWin;
    public event Action OnFail;

    // === Chờ clear khi spawn chưa xong / hoặc registry tới muộn ===
    bool _pendingClear;
    float _pendingSince;             // dùng làm fallback timeout
    const float PendingClearTimeout = 0.75f; // fallback nếu spawner không báo xong

    // === RESULT SNAPSHOT (để panel hiển thị & claim) ===
    int _resultWaveIndex;
    int _resultTotalKill;
    long _resultWaveReward;
    bool _claimed; // chặn claim lại

    // Expose cho UI
    public WaveState State => state;
    public int CurrentWave => currentWaveIndex;     // wave sắp đánh
    public bool LastWin => lastWin;
    public int ResultWaveIndex => _resultWaveIndex;     // wave vừa clear
    public int ResultTotalKill => _resultTotalKill;
    public long ResultWaveReward => _resultWaveReward;
    public bool CanClaim => lastWin && !_claimed && _resultWaveReward > 0;

    void Awake()
    {
        // wave đã clear nằm trong Save, nên wave sắp đánh là +1
        currentWaveIndex = Mathf.Max(1, SaveManager.Data.waveIndex + 1);
        Debug.Log($"WM: Awake, CurrentWave={currentWaveIndex}");
    }

    void OnEnable()
    {
        if (baseHealth) baseHealth.OnBaseDead += HandleBaseDead;
        TrySubscribeRegistry();
    }

    void OnDisable()
    {
        if (baseHealth) baseHealth.OnBaseDead -= HandleBaseDead;
        TryUnsubscribeRegistry();
    }

    void Start()
    {
        if (autoStart || SaveManager.Data.autoStart) GoPrepare();
    }

    void Update()
    {
        // 1) EnemyRegistry có thể sinh sau → cố gắng subscribe muộn
        if (!_subscribedRegistry) TrySubscribeRegistry();

        // 2) Nếu đã clear hết sớm nhưng còn đợt spawn, chờ đến khi spawn xong rồi chốt thắng
        if (state == WaveState.Combat && _pendingClear)
        {
            bool finished = (spawner == null) || spawner.IsFinishedSpawning;
            bool noEnemy = (EnemyRegistry.I == null) || (EnemyRegistry.I.AliveCount == 0);

            // Điều kiện chuẩn: spawn xong + không còn quái
            if (finished && noEnemy)
            {
                Debug.Log("WM: Update pending → FinalizeWin (finished && noEnemy)");
                FinalizeWin();
            }
            else
            {
                // Fallback sau một khoảng trễ vẫn không có quái → chốt thắng
                if (noEnemy && (Time.time - _pendingSince) > PendingClearTimeout)
                {
                    Debug.LogWarning("WM: Fallback FinalizeWin (timeout reached)");
                    FinalizeWin();
                }
            }
        }
    }

    public void GoPrepare()
    {
        state = WaveState.Prepare;
        lastWin = false;
        _pendingClear = false;

        // clear snapshot
        _resultWaveIndex = 0;
        _resultTotalKill = 0;
        _resultWaveReward = 0;
        _claimed = false;

        Debug.Log("WM: GoPrepare");
        if (autoStart || SaveManager.Data.autoStart) StartCombat();
    }

    public void StartCombat()
    {
        if (state != WaveState.Prepare) return;
        state = WaveState.Combat;

        var def = GetWaveDefinition(currentWaveIndex);
        if (!def) Debug.LogWarning($"[WaveManager] Missing Wave {currentWaveIndex} def");
        spawner.Begin(def);

        Debug.Log("WM: StartCombat");
        // BaseHP đã tự scale trong BaseHealth theo Save.waveIndex
    }

    // === Tính thống kê cho 1 wave bất kỳ (không phụ thuộc state) ===
    public (int totalKill, long waveReward) CalcWaveStats(int waveIndex)
    {
        var def = GetWaveDefinition(waveIndex);
        if (def == null) return (0, 0);

        int totalKill = 0;
        foreach (var g in def.groups) totalKill += g.count;

        // Ví dụ công thức thưởng:
        // reward = 20_000 * (waveRewardGrowth ^ (waveIndex - 1))
        float growth = progression ? Mathf.Pow(progression.waveRewardGrowth, waveIndex - 1) : 1f;
        long baseRw = 20000;
        long reward = Mathf.RoundToInt(baseRw * growth);
        return (totalKill, reward);
    }

    WaveDefinitionSO GetWaveDefinition(int waveIndex)
    {
        if (presets != null && waveIndex - 1 < presets.Length && waveIndex - 1 >= 0)
            return presets[waveIndex - 1];

        return (presets != null && presets.Length > 0)
            ? presets[Mathf.Clamp(presets.Length - 1, 0, presets.Length - 1)]
            : null;
    }

    // ===== EnemyRegistry subscription (chắc chắn) =====
    bool _subscribedRegistry = false;

    void TrySubscribeRegistry()
    {
        if (_subscribedRegistry) return;
        if (EnemyRegistry.I == null) return;

        EnemyRegistry.I.OnAllCleared += HandleAllCleared;
        _subscribedRegistry = true;
        Debug.Log("WM: Subscribed EnemyRegistry.OnAllCleared");
    }

    void TryUnsubscribeRegistry()
    {
        if (!_subscribedRegistry) return;
        if (EnemyRegistry.I != null)
            EnemyRegistry.I.OnAllCleared -= HandleAllCleared;
        _subscribedRegistry = false;
    }

    void HandleAllCleared()
    {
        if (state != WaveState.Combat) return;

        if (spawner != null && !spawner.IsFinishedSpawning)
        {
            // Hết quái tạm thời, nhưng còn đợt spawn sau ⇒ chờ
            _pendingClear = true;
            _pendingSince = Time.time;
            Debug.Log("WM: OnAllCleared (pending until spawner finished)");
            return;
        }

        Debug.Log("WM: OnAllCleared → FinalizeWin immediately");
        FinalizeWin();
    }

    void FinalizeWin()
    {
        // WIN → chốt snapshot (không cộng coin, không advance ở đây)
        lastWin = true;
        _resultWaveIndex = currentWaveIndex;
        (_resultTotalKill, _resultWaveReward) = CalcWaveStats(currentWaveIndex);
        _claimed = false;

        state = WaveState.Result;
        _pendingClear = false;

        Debug.Log($"WM: FinalizeWin → wave={_resultWaveIndex}, kill={_resultTotalKill}, reward={_resultWaveReward}");
        OnWin?.Invoke(); // UI bật panel
    }

    void HandleBaseDead()
    {
        if (state != WaveState.Combat) return;

        lastWin = false;
        _resultWaveIndex = 0;
        _resultTotalKill = 0;
        _resultWaveReward = 0;
        _claimed = true; // chặn claim

        state = WaveState.Result;
        _pendingClear = false;

        Debug.Log("WM: HandleBaseDead → FAIL");
        OnFail?.Invoke();
    }

    // Claim thưởng và chuyển sang Prepare. multiplier = 1 (Claim), = 2 (Claim 2x)...
    public void ClaimAndContinue(float multiplier = 1f)
    {
        if (state != WaveState.Result) return;

        if (lastWin && !_claimed && _resultWaveReward > 0)
        {
            long reward = Mathf.RoundToInt(_resultWaveReward * Mathf.Max(1f, multiplier));
            EconomyManager.I.AddCoin(reward);

            // Advance wave sau khi đã claim
            SaveManager.Data.waveIndex = Mathf.Max(SaveManager.Data.waveIndex, _resultWaveIndex);
            SaveManager.MarkDirtyAndSave();
            currentWaveIndex = SaveManager.Data.waveIndex + 1;

            _claimed = true;
            Debug.Log($"WM: Claim reward={reward}, advance to {currentWaveIndex}");
        }

        GoPrepare();
    }

    public void Retry()
    {
        if (state != WaveState.Result) return;
        // Không advance, không cộng coin
        state = WaveState.Prepare;
        Debug.Log("WM: Retry");
        StartCombat();
    }
}
