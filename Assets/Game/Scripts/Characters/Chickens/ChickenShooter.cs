using game.scripts.characters.chickens;
using Game.Scripts.Projectiles;
using UnityEngine;

namespace Game.Scripts.Characters.Chickens
{
    public class ChickenShooter : MonoBehaviour
    {
        [Header("Shooting Stats")]
        [Tooltip("Tốc độ bắn (viên/giây)")]
        public float fireRate = 1f;

        [Tooltip("Tầm bắn của gà")]
        public float range = 8f;

        [Tooltip("Sát thương mỗi viên đạn")]
        public int damage = 2;

        [Header("References")]
        [Tooltip("Nòng súng để xoay và bắn (GameObject con Gun)")]
        public GunController gun;

        [Tooltip("Pool chứa sẵn projectile (GameObject con Pool)")]
        public ProjectilePool pool;

        private float fireCooldown;

        private void Update()
        {
            fireCooldown -= Time.deltaTime;

            Transform target = FindNearestEnemy();
            if (gun != null)
            {
                gun.SetTarget(target);
            }

            if (target != null && fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / Mathf.Max(0.001f, fireRate);
            }
        }

        private Transform FindNearestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
            Transform nearest = null;
            float minDist = float.MaxValue;

            foreach (var c in hits)
            {
                if (!c.CompareTag("Enemy")) continue;

                float d = (c.transform.position - transform.position).sqrMagnitude;
                if (d < minDist)
                {
                    minDist = d;
                    nearest = c.transform;
                }
            }

            return nearest;
        }

        private void Shoot()
        {
            if (gun == null || pool == null) return;

            GameObject go = pool.Get();
            if (go == null) return;

            var proj = go.GetComponent<Projectile>();
            if (proj == null) return;

            Vector2 dir = gun.GetShootDirection();
            proj.Launch(gun.firePoint.position, dir, damage, pool);

            // TODO: thêm muzzle flash / sound effect tại đây
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
