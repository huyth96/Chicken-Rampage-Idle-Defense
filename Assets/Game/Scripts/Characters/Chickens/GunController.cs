using Game.Scripts.Projectiles;
using UnityEngine;

namespace game.scripts.characters.chickens
{
    public class GunController : MonoBehaviour
    {
        [Header("Gun Settings")]
        public Transform firePoint;
        public float rotationSpeed = 720f;

        private Transform target;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Update()
        {
            if (target == null) return;

            Vector2 dir = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime / 360f);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public Vector2 GetShootDirection()
        {
            if (target == null) return Vector2.right; 
            return (target.position - firePoint.position).normalized;
        }

        public bool HasTarget()
        {
            return target != null;
        }
    }
}
