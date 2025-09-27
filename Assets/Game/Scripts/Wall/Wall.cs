using UnityEngine;

namespace Game.Characters.Walls
{
    public class Wall : MonoBehaviour
    {
        [Header("Wall Stats")]
        [Tooltip("Máu tối đa của tường")]
        public int maxHP = 30;

        public int CurrentHP { get; private set; }

        private void Awake()
        {
            CurrentHP = maxHP;
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
            Destroy(gameObject);
        }
    }
}
