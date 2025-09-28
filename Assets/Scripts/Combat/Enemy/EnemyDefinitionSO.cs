using UnityEngine;

[CreateAssetMenu(menuName = "CR/Enemy Definition")]
public class EnemyDefinitionSO : ScriptableObject
{
    public string id = "zombie_basic";
    public long baseHP = 30;
    public float moveSpeed = 1.5f;
    public long touchDamage = 10;   // sát thương vào Base khi chạm
    public long bounty = 5;         // coin rơi khi chết
    public bool isBoss = false;
}
