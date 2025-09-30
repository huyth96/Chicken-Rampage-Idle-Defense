// Assets/Scripts/Chicken/ChickenUnit.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ChickenUnit : MonoBehaviour
{
    [Header("Bind")]
    public ChickenDefinitionSO def;

    [Header("Scan")]
    public LayerMask enemyLayer;      // set Layer "Enemy" nếu có
    public bool usePhysicsScan = true;
    public float scanInterval = 0.2f; // thời gian giữa mỗi lần tìm target

    float _cooldown;
    float _scanTimer;
    Enemy _currentTarget;

    // ===== Multiplier helpers (null-safe) =====
    float TeamDamageMul() => UpgradeManager.I ? UpgradeManager.I.TeamDamageMul() : 1f;
    float TeamASMul() => UpgradeManager.I ? UpgradeManager.I.TeamASMul() : 1f;
    float PerTypeMul() => (UpgradeManager.I && def) ? UpgradeManager.I.PerTypeMul(def.type) : 1f;

    float EffectiveCooldown()
    {
        if (def == null) return 9999f;
        float rofEff = Mathf.Max(0.01f, def.rateOfFire * TeamASMul());
        return 1f / rofEff;
    }

    long DamagePerShot(float critMul)
    {
        if (def == null) return 0;
        double dmg = (double)def.baseDamage * TeamDamageMul() * PerTypeMul() * critMul;
        if (dmg < 0d) dmg = 0d;
        if (dmg > long.MaxValue) dmg = long.MaxValue;
        return (long)Math.Round(dmg);
    }

    float RollCritMul()
    {
        if (def == null || def.critChance <= 0f || def.critMultiplier <= 1f) return 1f;
        return (UnityEngine.Random.value < def.critChance) ? def.critMultiplier : 1f;
    }

    // DPS lý thuyết = dmg * (1 + critChance*(critMul-1)) * ROF * multipliers
    public float TheoreticalDPS
    {
        get
        {
            if (def == null) return 0f;
            double avgCritMul = 1.0 + (double)def.critChance * ((double)def.critMultiplier - 1.0);
            double perShot = (double)def.baseDamage * TeamDamageMul() * PerTypeMul() * avgCritMul;
            double dps = perShot * (double)(def.rateOfFire * TeamASMul());
            return (float)dps;
        }
    }

    void Update()
    {
        if (def == null) return;

        // Tìm target định kỳ
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = scanInterval;
            AcquireTarget();
        }

        // Mất mục tiêu nếu ra khỏi tầm/không active
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (dist > def.range || !_currentTarget.gameObject.activeInHierarchy)
            {
                _currentTarget = null;
            }
        }

        // Bắn
        _cooldown -= Time.deltaTime;
        if (_currentTarget != null && _cooldown <= 0f)
        {
            FireAt(_currentTarget);
            _cooldown = EffectiveCooldown(); // đã nhân Team AS
        }
    }

    void AcquireTarget()
    {
        Vector3 center = transform.position;

        if (usePhysicsScan && enemyLayer.value != 0)
        {
            // quét tròn bằng Physics2D
            var hits = Physics2D.OverlapCircleAll(center, def.range, enemyLayer);
            Enemy best = null;
            float bestScore = float.PositiveInfinity; // khoảng cách làm score

            foreach (var h in hits)
            {
                Enemy e = h.GetComponent<Enemy>();
                if (e == null || !e.gameObject.activeInHierarchy) continue;

                float d = Vector3.SqrMagnitude(e.transform.position - center);
                if (def.prioritizeClosest)
                {
                    if (d < bestScore) { best = e; bestScore = d; }
                }
                else
                {
                    // TODO: có thể đổi sang tiêu chí "gần Base nhất" sau
                    if (d < bestScore) { best = e; bestScore = d; }
                }
            }
            _currentTarget = best;
            return;
        }

        // fallback: tìm mọi Enemy (ít tối ưu nhưng đủ dùng bản proto)
        Enemy[] all = GameObject.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Enemy best2 = null;
        float bs = float.PositiveInfinity;
        foreach (var e in all)
        {
            if (!e.gameObject.activeInHierarchy) continue;
            float d = Vector3.Distance(center, e.transform.position);
            if (d <= def.range && d < bs)
            {
                bs = d; best2 = e;
            }
        }
        _currentTarget = best2;
    }

    void FireAt(Enemy target)
    {
        if (target == null) return;

        float critMul = RollCritMul();
        long damage = DamagePerShot(critMul); // đã nhân Team/PerType

        // Nếu có projectile prefab => bắn đạn
        if (def.projectilePrefab != null)
        {
            var proj = Instantiate(def.projectilePrefab, transform.position, Quaternion.identity);
            proj.damage = damage;
            proj.speed = def.projectileSpeed;
            proj.target = target.transform;

            // Truyền thông tin AoE & mask (để xử lý khi va chạm)
            proj.areaOfEffect = def.areaOfEffect;
            proj.aoeRadius = def.aoeRadius;
            proj.enemyLayer = enemyLayer;
        }
        else
        {
            // Instant hit (raycast optional)
            if (!def.allowThroughWalls)
            {
                Vector2 dir = (target.transform.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, target.transform.position);
                // Nếu có Layer tường riêng bạn thêm mask chặn vào đây
                Physics2D.Raycast(transform.position, dir, dist);
            }
            target.TakeDamage(damage);

            // AoE instant
            if (def.areaOfEffect && def.aoeRadius > 0.01f)
            {
                var hits = Physics2D.OverlapCircleAll(target.transform.position, def.aoeRadius, enemyLayer);
                foreach (var h in hits)
                {
                    var e = h.GetComponent<Enemy>();
                    if (e != null && e != target) e.TakeDamage(damage);
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (def == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, def.range);
        if (def.areaOfEffect)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.8f);
            // vẽ minh hoạ AOE ngay cạnh gà (chỉ là gizmo tham khảo)
            Gizmos.DrawWireSphere(transform.position + Vector3.right * 0.5f, Mathf.Max(0.1f, def.aoeRadius));
        }
    }
#endif
}
