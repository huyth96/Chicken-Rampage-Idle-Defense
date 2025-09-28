using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public EnemyDefinitionSO def;
    public BaseHealth targetBase;
    public float laneY = 0f;      // tuỳ chỉnh lane
    public float spawnX = 10f;    // vị trí spawn bên phải

    private long hp;

    void OnEnable()
    {
        hp = def != null ? def.baseHP : 10;
        var pos = transform.position;
        transform.position = new Vector3(spawnX, laneY, pos.z);
    }

    void Update()
    {
        transform.position += Vector3.left * (def != null ? def.moveSpeed : 1.5f) * Time.deltaTime;
    }

    public void TakeDamage(long dmg)
    {
        hp -= Mathf.Max(0, (int)dmg);
        if (hp <= 0) Die();
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        var b = c.GetComponent<BaseHealth>();
        if (!b) return;
        b.TakeDamage(def != null ? def.touchDamage : 10);
        Die(); // chạm base là “tiêu”
    }

    void Die()
    {
        EconomyManager.I.AddCoin(def != null ? def.bounty : 1);
        gameObject.SetActive(false);
    }
}
