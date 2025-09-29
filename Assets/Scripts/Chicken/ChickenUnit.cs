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

    // DPS lý thuyết = dmg * (1 + critChance*(critMul-1)) * ROF
    public float TheoreticalDPS
    {
        get
        {
            double avgCritMul = 1.0 + (double)def.critChance * ((double)def.critMultiplier - 1.0);
            double perShot = (double)def.baseDamage * avgCritMul;
            return (float)(perShot * (double)def.rateOfFire);
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

        // Bắn
        if (_currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (dist > def.range || !_currentTarget.gameObject.activeInHierarchy)
            {
                _currentTarget = null;
            }
        }

        _cooldown -= Time.deltaTime;
        if (_currentTarget != null && _cooldown <= 0f)
        {
            FireAt(_currentTarget);
            _cooldown = 1f / Mathf.Max(0.01f, def.rateOfFire);
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
            float bestScore = float.PositiveInfinity; // dùng khoảng cách làm score

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
                    // có thể thêm chiến lược khác sau này (tiến gần Base nhất, v.v.)
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

        long damage = RollDamage();

        // Nếu có projectile prefab => bắn đạn
        if (def.projectilePrefab != null)
        {
            var proj = Instantiate(def.projectilePrefab, transform.position, Quaternion.identity);
            proj.damage = damage;
            proj.speed = def.projectileSpeed;
            proj.target = target.transform;
        }
        else
        {
            // Instant hit (raycast optional)
            if (!def.allowThroughWalls)
            {
                Vector2 dir = (target.transform.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, target.transform.position);
                var hit = Physics2D.Raycast(transform.position, dir, dist, enemyLayer);
                // nếu cần check tường, bạn tạo LayerMask cho "Wall" và raycast hai lần
            }
            target.TakeDamage(damage);

            // nếu AoE
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

    long RollDamage()
    {
        bool isCrit = UnityEngine.Random.value < def.critChance;
        double mul = isCrit ? (double)def.critMultiplier : 1.0;
        double val = (double)def.baseDamage * mul;
        if (val <= long.MinValue) return long.MinValue;
        if (val >= long.MaxValue) return long.MaxValue;
        return (long)System.Math.Round(val);
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
            Gizmos.DrawWireSphere(transform.position + Vector3.right * 0.5f, Mathf.Max(0.1f, def.aoeRadius));
        }
    }
#endif
}
