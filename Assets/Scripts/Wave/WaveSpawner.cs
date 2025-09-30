// Assets/Scripts/Wave/WaveSpawner.cs
using CR.Wave;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPrefabEntry
{
    public EnemyDefinitionSO definition;   // SO của loại enemy
    public Enemy prefab;                   // Prefab tương ứng
}

public class WaveSpawner : MonoBehaviour
{
    [Header("Mapping SO → Prefab")]
    public EnemyPrefabEntry[] enemyPrefabs;   // Kéo thả trong Inspector

    [Header("Refs")]
    public Transform poolRoot;                // Container tùy chọn
    public BaseHealth baseHealth;             // Base để quái tìm đến

    [Header("Runtime")]
    public WaveDefinitionSO wave;

    // Nội bộ
    Dictionary<EnemyDefinitionSO, ObjectPool<Enemy>> _pools;
    Dictionary<EnemyDefinitionSO, Enemy> _prefabByDef;
    float _t;
    int[] _spawnedPerGroup;
    bool _active;

    void Awake()
    {
        _pools = new Dictionary<EnemyDefinitionSO, ObjectPool<Enemy>>();
        _prefabByDef = new Dictionary<EnemyDefinitionSO, Enemy>();

        if (enemyPrefabs == null) return;

        foreach (var entry in enemyPrefabs)
        {
            // ✅ dùng so sánh null, KHÔNG dùng !
            if (entry == null || entry.definition == null || entry.prefab == null)
                continue;

            _prefabByDef[entry.definition] = entry.prefab;
            _pools[entry.definition] = new ObjectPool<Enemy>(entry.prefab, poolRoot);
        }
    }

    public void Begin(WaveDefinitionSO def)
    {
        wave = def;
        _t = 0f;
        _active = true;
        _spawnedPerGroup = wave != null && wave.groups != null ? new int[wave.groups.Length] : null;
    }

    public bool IsFinishedSpawning
    {
        get
        {
            if (wave == null || _spawnedPerGroup == null) return true;
            for (int i = 0; i < _spawnedPerGroup.Length; i++)
                if (_spawnedPerGroup[i] < wave.groups[i].count) return false;
            return true;
        }
    }

    void Update()
    {
        if (!_active || wave == null) return;
        _t += Time.deltaTime;

        for (int i = 0; i < wave.groups.Length; i++)
        {
            var g = wave.groups[i];
            if (_t < g.startTime) continue;

            if (_spawnedPerGroup[i] < g.count)
            {
                int expect = Mathf.FloorToInt((_t - g.startTime) / Mathf.Max(0.01f, g.interval)) + 1;
                int toSpawn = Mathf.Clamp(expect - _spawnedPerGroup[i], 0, g.count - _spawnedPerGroup[i]);
                for (int k = 0; k < toSpawn; k++) SpawnOne(g);
                _spawnedPerGroup[i] += toSpawn;
            }
        }

        if (IsFinishedSpawning) _active = false;
    }

    private void SpawnOne(WaveDefinitionSO.Group g)
    {
        if (!_pools.TryGetValue(g.enemy, out var pool))
        {
            Debug.LogError($"[WaveSpawner] Missing prefab mapping for {g.enemy?.name}");
            return;
        }

        var e = pool.Get();
        e.def = g.enemy;
        e.targetBase = baseHealth;

        float y;
        switch (g.laneMode)
        {
            case LaneMode.RandomFromList:
                var list = g.laneList != null && g.laneList.Length > 0
                           ? g.laneList
                           : new[] { g.laneY };
                y = list[Random.Range(0, list.Length)];
                break;

            case LaneMode.RandomRangeY:
                var yr = g.yRange;
                if (yr.x > yr.y) (yr.x, yr.y) = (yr.y, yr.x);
                y = Random.Range(yr.x, yr.y);
                break;

            default: // FixedY
                y = g.laneY;
                break;
        }

        // Giữ X cố định (spawnX của group)
        float x = g.spawnX;

        e.transform.position = new Vector3(x, y, 0f);

        if (!e.TryGetComponent<EnemyHook>(out _))
            e.gameObject.AddComponent<EnemyHook>();
    }
}
