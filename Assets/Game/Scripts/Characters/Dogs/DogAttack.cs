using UnityEngine;
using Game.Characters.Walls;

namespace Game.Characters.Dogs
{
    [RequireComponent(typeof(DogRuntime))]
    [RequireComponent(typeof(DogMovement))]
    [RequireComponent(typeof(Animator))]
    public class DogAttack : MonoBehaviour
    {
        [Tooltip("Thời gian giữa 2 đòn tấn công (giây)")]
        public float attackCooldown = 1f;

        private DogRuntime runtime;
        private DogMovement movement;
        private Animator animator;

        private float attackTimer;
        private Wall targetWall;

        private void Awake()
        {
            runtime = GetComponent<DogRuntime>();
            movement = GetComponent<DogMovement>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (targetWall != null && targetWall.gameObject != null)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    AttackWall();
                    attackTimer = attackCooldown;
                }
            }
            else if (targetWall != null)
            {
                ClearTarget();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Wall"))
            {
                targetWall = other.GetComponent<Wall>();
                if (targetWall != null)
                {
                    movement.enabled = false;

                    var rb = GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = Vector2.zero;

                    animator.SetBool("isWalking", false);
                    animator.SetBool("isAttacking", true); 

                    attackTimer = 0f;
                }
            }
        }

        private void AttackWall()
        {
            if (targetWall == null || targetWall.gameObject == null)
            {
                ClearTarget();
                return;
            }

            int damage = runtime.Stat.damage;
            targetWall.TakeDamage(damage);
           
            if (targetWall.CurrentHP <= 0)
            {
                ClearTarget();
            }
        }

        private void ClearTarget()
        {
            targetWall = null;
            movement.enabled = false; 
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false); 
        }
    }
}
