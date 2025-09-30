using UnityEngine;
public enum UpgradeKind { Team, PerType }

[CreateAssetMenu(menuName = "CR/Upgrades/Definition")]
public class UpgradeDefinitionSO : ScriptableObject
{
    [Header("Meta")]
    public string id;
    public string displayName;
    public UpgradeKind kind;
    public ChickenType typeTarget; // dùng nếu PerType

    [Header("Economy")]
    public long baseCost = 2000;
    public float growth = 1.22f; // team dmg/as 1.22; mps 1.30; per-type 1.25

    [Header("Effect per level")]
    public float teamDamagePct = 0.08f; // 8%/lv
    public float teamASPct = 0.06f; // 6%/lv
    public float eggSeconds = 0.2f;  // 0.2s/lv
    public long mpsValue = 2;     // 2 coin/s/lv
    public float discountPct = 0.03f; // 3%/lv (cap 50%)
    public float perTypeDpsPct = 0.10f; // 10%/lv
}
