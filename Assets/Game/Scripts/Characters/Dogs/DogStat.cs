using UnityEngine;

namespace Game.Characters.Dogs
{
    public class DogStat : MonoBehaviour
    {
        [Header("Base Stats")]
        [Tooltip("Tốc độ di chuyển")]
        public float moveSpeed = 2f;

        [Tooltip("Máu tối đa")]
        public int maxHP = 10;

        [Tooltip("Sát thương mỗi đòn cắn / va chạm")]
        public int damage = 1;

        [Tooltip("Số vàng nhận được khi bị tiêu diệt")]
        public int goldReward = 5;
    }
}


