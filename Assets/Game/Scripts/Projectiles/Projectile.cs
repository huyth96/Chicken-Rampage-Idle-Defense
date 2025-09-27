using Game.Characters.Dogs;
using UnityEngine;

namespace Game.Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        public float speed = 10f;
        public float lifeTime = 3f;

        private Rigidbody2D rb;
        private ProjectilePool pool;
        private Animator animator;

        private int damage;
        private float timer;
        private bool isExploding;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            rb.gravityScale = 0f;
        }

        private void OnEnable()
        {
            timer = lifeTime;
            isExploding = false;
            animator.ResetTrigger("Explode");
        }

        private void Update()
        {
            if (isExploding) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                TriggerExplosion();
            }
        }

        public void Launch(Vector2 position, Vector2 direction, int damage, ProjectilePool poolRef)
        {
            transform.position = position;
            transform.right = direction;

            this.damage = damage;
            this.pool = poolRef;

            rb.linearVelocity = direction.normalized * speed;
            gameObject.SetActive(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isExploding) return;

            if (!other.CompareTag("Enemy")) return;

            var dog = other.GetComponent<DogRuntime>();
            if (dog != null)
            {
                dog.TakeDamage(damage);
            }

            TriggerExplosion();
        }

       
        private void TriggerExplosion()
        {
            isExploding = true;
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetTrigger("Explode");
            }
            else
            {
                ReturnToPool();
            }
        }

        // Gọi từ Animation Event cuối clip Explode
        public void OnExplosionEnd()
        {
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            rb.linearVelocity = Vector2.zero;
            isExploding = false;

            if (pool != null)
            {
                pool.Return(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
