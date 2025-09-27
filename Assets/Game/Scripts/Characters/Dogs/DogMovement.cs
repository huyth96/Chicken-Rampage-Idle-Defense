using UnityEngine;

namespace Game.Characters.Dogs
{
    [RequireComponent(typeof(DogStat))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class DogMovement : MonoBehaviour
    {
        private DogStat stat;
        private Rigidbody2D rb;
        private Animator animator;

        private void Awake()
        {
            stat = GetComponent<DogStat>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            Vector2 direction = Vector2.left;
            Vector2 velocity = direction * stat.moveSpeed;
            rb.linearVelocity = velocity;

            bool isWalking = velocity.sqrMagnitude > 0.01f;
            animator.SetBool("isWalking", isWalking);
        }
    }
}
