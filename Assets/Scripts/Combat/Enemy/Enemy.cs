// Enemy.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public EnemyDefinitionSO def;
    public BaseHealth targetBase;
    public float laneY = 0f;
    public float spawnX = 10f;

    private long hp;
    private bool _dead;        // ✅ chốt chống double-die

    void OnEnable()
    {
        _dead = false;         // ✅ reset mỗi lần spawn lại
        hp = def != null ? def.baseHP : 10;
        var pos = transform.position;
        transform.position = new Vector3(spawnX, laneY, pos.z);
    }

    void Update()
    {
        if (_dead) return;     // ✅ đã chết thì thôi
        transform.position += Vector3.left * (def != null ? def.moveSpeed : 1.5f) * Time.deltaTime;
    }

    public void TakeDamage(long dmg)
    {
        if (_dead) return;     // ✅ guard
        hp -= Mathf.Max(0, (int)dmg);
        if (hp <= 0) Die();
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (_dead) return;     // ✅ guard
        var b = c.GetComponent<BaseHealth>();
        if (!b) return;
        b.TakeDamage(def != null ? def.touchDamage : 10);
        Die();                 // có thể trùng với kết liễu bằng đạn nếu không có guard
    }

    void Die()
    {
        if (_dead) return;     // ✅ guard
        _dead = true;
        EconomyManager.I.AddCoin(def != null ? def.bounty : 1);
        gameObject.SetActive(false);
    }
}
