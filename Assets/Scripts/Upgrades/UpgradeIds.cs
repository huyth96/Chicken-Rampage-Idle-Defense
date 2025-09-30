public static class UpgradeIds
{
    // Team
    public const string Team_Damage = "team_damage";       // +8%/lv
    public const string Team_AS = "team_attack_speed"; // +6%/lv
    public const string Team_EggSpeed = "team_egg_speed";    // -0.2s/lv
    public const string Team_MPS = "team_mps";          // +2 coin/s/lv
    public const string Team_Discount = "team_discount";     // -3%/lv (cap 50%)

    // Per-type
    public static string TypeKey(ChickenType t) => $"type_{t.ToString().ToLower()}";
}
