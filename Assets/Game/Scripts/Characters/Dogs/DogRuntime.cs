using UnityEngine;

namespace Game.Characters.Dogs
{
    [RequireComponent(typeof(DogStat))]
    [RequireComponent(typeof(Animator))]
    public class DogRuntime : MonoBehaviour
    {
        private DogStat stat;
        private Animator animator;

        public int CurrentHP { get; private set; }
        public DogStat Stat => stat;

        private void Awake()
        {
            stat = GetComponent<DogStat>();
            animator = GetComponent<Animator>();
            CurrentHP = stat.maxHP;
        }

        public void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} chết, thưởng {stat.goldReward} vàng!");

            animator.SetTrigger("Dead");

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        public void OnDeadAnimationEnd()
        {
            Destroy(gameObject);
        }
    }
}
