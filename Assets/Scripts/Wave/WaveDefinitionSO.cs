using System;
using UnityEngine;

namespace CR.Wave
{
    public enum LaneMode { FixedY, RandomFromList, RandomRangeY }

    [CreateAssetMenu(menuName = "CR/Wave", fileName = "Wave_")]
    public class WaveDefinitionSO : ScriptableObject
    {
        public int waveIndex;
        public float targetDuration = 25f;
        public bool isBoss;

        [Serializable]
        public struct Group
        {
            public EnemyDefinitionSO enemy;
            [Min(1)] public int count;
            [Min(0.01f)] public float interval;
            [Min(0f)] public float startTime;

            [Header("Lane Y")]
            public LaneMode laneMode;       // NEW
            public float laneY;             // dùng cho FixedY
            public float[] laneList;        // dùng khi RandomFromList
            public Vector2 yRange;          // dùng khi RandomRangeY
            // Giữ spawnX cũ, KHÔNG random
            public float spawnX;

            [Header("Timing")]
            public Vector2 intervalJitterPct; // ±% jitter (tuỳ chọn)
        }

        public Group[] groups = Array.Empty<Group>();
    }
}
