// Assets/Scripts/Wave/WaveDefinitionSO.cs
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CR/Wave Definition")]
public class WaveDefinitionSO : ScriptableObject
{
    public int waveIndex = 1;
    public float targetDuration = 25f;
    [Serializable]
    public struct Group
    {
        public EnemyDefinitionSO enemy;
        public int count;
        public float interval;
        public float startTime;
        public float laneY;
        public float spawnX;
    }
    public Group[] groups;
    public bool isBoss;
}
