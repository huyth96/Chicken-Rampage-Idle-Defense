// Assets/Scripts/Combat/Projectile.cs
using UnityEngine;

/// <summary>
/// Đạn homing đơn giản:
/// - Set: damage, speed, target (Transform)
/// - Tự bay về target; khi chạm thì gây sát thương (và AoE nếu bật).
/// - Có maxLifetime để tránh "mồ côi".
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Flight")]
    public float speed = 10f;
    public float maxLifetime = 5f;

    [Header("Payload")]
    public long damage;
    public Transform target;

    [Header("AoE on hit")]
    public bool areaOfEffect = false;
    public float aoeRadius = 0f;
    public LayerMask enemyLayer;

    bool _launched;

    void OnEnable()
    {
        _launched = true;
        CancelInvoke();
        Invoke(nameof(Despawn), maxLifetime);
    }

    void Update()
    {
        if (!_launched) return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Despawn();
            return;
        }

        Vector3 to = target.position - transform.position;
        float dist = to.magnitude;
        float step = speed * Time.deltaTime;

        if (dist <= step || dist < 0.01f)
        {
            // On-hit
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);

            if (areaOfEffect && aoeRadius > 0.01f)
            {
                var hits = Physics2D.OverlapCircleAll(target.position, aoeRadius, enemyLayer);
                foreach (var h in hits)
                {
                    var e = h.GetComponent<Enemy>();
                    if (e && (!enemy || e != enemy)) e.TakeDamage(damage);
                }
            }

            Despawn();
            return;
        }

        Vector3 dir = to / dist;
        transform.position += dir * step;
        if (dir.sqrMagnitude > 0.0001f) transform.right = dir; // quay đầu đạn
    }

    void OnDisable()
    {
        CancelInvoke();
        _launched = false;
    }

    void Despawn()
    {
        _launched = false;
        Destroy(gameObject); // TODO: đổi sang Object Pool nếu cần
    }
}
