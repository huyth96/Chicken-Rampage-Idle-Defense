// Assets/Scripts/Chicken/ChickenDefinitionSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "CR/Chicken Definition")]
public class ChickenDefinitionSO : ScriptableObject
{
    [Header("Meta")]
    public string id = "chicken_soldier";
    public ChickenType type = ChickenType.Soldier;

    [Header("Stats (per unit)")]
    public long baseDamage = 10;         // sát thương/viên
    public float rateOfFire = 2.0f;      // viên/giây
    [Range(0, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 2.0f;
    public float range = 8f;

    [Header("Targeting")]
    public bool prioritizeClosest = true;  // ưu tiên gần nhất
    public bool allowThroughWalls = true;  // nếu false sẽ raycast line-of-sight

    [Header("Firing Mode")]
    public Projectile projectilePrefab;     // nếu để trống => Instant Hit
    public float projectileSpeed = 16f;
    public bool areaOfEffect = false;
    public float aoeRadius = 1.5f;
    [Header("Shop")]
    public long basePrice = 100; // chỉnh theo loại gà
    [Header("Visuals")]
    public Sprite icon;
}
