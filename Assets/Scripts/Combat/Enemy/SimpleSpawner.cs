using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public Transform poolRoot;
    public BaseHealth baseHealth;

    [Header("Pattern")]
    public EnemyDefinitionSO enemyDef;
    public float interval = 1.2f;
    public int count = 10;
    public float laneY = 0f;
    public float spawnX = 10f;

    private ObjectPool<Enemy> pool;
    private float t;
    private int spawned;

    void Start()
    {
        pool = new ObjectPool<Enemy>(enemyPrefab, poolRoot);
    }

    void Update()
    {
        if (spawned >= count) return;
        t += Time.deltaTime;
        if (t >= interval)
        {
            t = 0f;
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        var e = pool.Get();
        e.def = enemyDef;
        e.targetBase = baseHealth;
        e.laneY = laneY;
        e.spawnX = spawnX;
        e.transform.position = new Vector3(spawnX, laneY, 0);
        spawned++;
    }
}
