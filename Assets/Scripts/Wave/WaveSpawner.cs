// Assets/Scripts/Wave/WaveSpawner.cs
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Enemy enemyPrefab;
    public Transform poolRoot;
    public BaseHealth baseHealth;

    [Header("Runtime")]
    public WaveDefinitionSO wave;
    private ObjectPool<Enemy> pool;
    private float t;
    private int[] spawnedPerGroup;
    private bool active;

    void Awake()
    {
        pool = new ObjectPool<Enemy>(enemyPrefab, poolRoot); // dùng ObjectPool có sẵn :contentReference[oaicite:4]{index=4}
    }

    public void Begin(WaveDefinitionSO def)
    {
        wave = def;
        if (wave == null) { Debug.LogWarning("[WaveSpawner] Wave null"); return; }
        t = 0f;
        spawnedPerGroup = new int[wave.groups.Length];
        active = true;
    }

    public bool IsFinishedSpawning
    {
        get
        {
            if (wave == null || spawnedPerGroup == null) return true;
            for (int i = 0; i < spawnedPerGroup.Length; i++)
            {
                if (spawnedPerGroup[i] < wave.groups[i].count) return false;
            }
            return true;
        }
    }

    void Update()
    {
        if (!active || wave == null) return;
        t += Time.deltaTime;

        for (int i = 0; i < wave.groups.Length; i++)
        {
            var g = wave.groups[i];
            if (t < g.startTime) continue;

            // Spawn đều theo interval
            if (spawnedPerGroup[i] < g.count)
            {
                // Mốc sinh theo đếm số lượng & thời gian
                int expect = Mathf.FloorToInt((t - g.startTime) / Mathf.Max(0.01f, g.interval)) + 1;
                int toSpawn = Mathf.Clamp(expect - spawnedPerGroup[i], 0, g.count - spawnedPerGroup[i]);
                for (int k = 0; k < toSpawn; k++) SpawnOne(g);
                spawnedPerGroup[i] += toSpawn;
            }
        }
        // Khi đã spawn xong hết → Spawner có thể idle; Win sẽ do EnemyRegistry báo
        if (IsFinishedSpawning) active = false;
    }

    void SpawnOne(WaveDefinitionSO.Group g)
    {
        var e = pool.Get();
        e.def = g.enemy;
        e.targetBase = baseHealth;  // BaseHealth đã có logic HP/Fail :contentReference[oaicite:5]{index=5}
        e.laneY = g.laneY;
        e.spawnX = g.spawnX != 0 ? g.spawnX : 10f;
        e.transform.position = new Vector3(e.spawnX, e.laneY, 0);

        // YÊU CẦU: Prefab Enemy nên có EnemyHook để đếm số lượng sống trong EnemyRegistry :contentReference[oaicite:6]{index=6}
        // Nếu prefab chưa có, add tạm:
        if (!e.GetComponent<EnemyHook>()) e.gameObject.AddComponent<EnemyHook>();
    }
}
