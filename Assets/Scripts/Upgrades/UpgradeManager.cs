// Assets/Scripts/Upgrades/UpgradeManager.cs
// Sprint 4 - Upgrade core (Team + Per-Type)
// - Quản lý level, giá nâng cấp, và xuất các multiplier/effect để hệ thống khác dùng.
// - Phụ thuộc: SaveManager (Data.teamUpgrades/typeUpgrades), EconomyManager (TrySpend),
//              UpgradeDefinitionSO[], UpgradeIds, ChickenType.
//
// HƯỚNG DẪN INSPECTOR:
//   - Gắn script này lên 1 GameObject tồn tại suốt game (ví dụ _Systems).
//   - Kéo TẤT CẢ các asset UpgradeDefinitionSO vào mảng 'upgradeDefs'.
//   - Team IDs dùng: team_damage, team_attack_speed, team_egg_speed, team_mps, team_discount
//   - Per-type IDs dùng: type_soldier, type_rapid, type_sniper, type_shotgun, type_rocket, type_support
//
// API CHÍNH:
//   int   GetLevel(string id)
//   long  GetPriceNext(string id)         // baseCost * growth^level
//   bool  TryBuy(string id)               // trừ coin & tăng level
//
//   float TeamDamageMul()  // (1 + 0.08)^lv
//   float TeamASMul()      // (1 + 0.06)^lv
//   float TeamDiscountFrac() // lv*0.03 (áp cap 50% khi TÍNH GIÁ GÀ, thực hiện ở ShopManager)
//   float TeamEggSeconds() // lv*0.2s  (dùng giảm thời gian tải trứng)
//   long  TeamMPS()        // lv*2 coin/s
//   float PerTypeMul(ChickenType t) // (1+0.10)^lv theo từng loại gà
//
// GỢI Ý HOOK:
//   - Damage mỗi viên = baseDamage * TeamDamageMul() * PerTypeMul(type) * critMul
//   - ROF hiệu dụng   = baseROF * TeamASMul()
//   - Ticker MPS: mỗi giây EconomyManager.I.AddCoin(TeamMPS());

using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager I { get; private set; }

    [Header("Database")]
    [Tooltip("Kéo tất cả UpgradeDefinitionSO (team + per-type) vào đây")]
    public UpgradeDefinitionSO[] upgradeDefs;

    // Tra cứu nhanh theo id
    private Dictionary<string, UpgradeDefinitionSO> _byId;

    // ========================================
    // Unity lifecycle
    // ========================================
    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // Build index
        _byId = new Dictionary<string, UpgradeDefinitionSO>(StringComparer.Ordinal);
        if (upgradeDefs != null)
        {
            foreach (var d in upgradeDefs)
            {
                if (d == null || string.IsNullOrEmpty(d.id)) continue;
                _byId[d.id] = d;
            }
        }

        // Ensure save maps
        if (SaveManager.Data.teamUpgrades == null) SaveManager.Data.teamUpgrades = new Dictionary<string, int>();
        if (SaveManager.Data.typeUpgrades == null) SaveManager.Data.typeUpgrades = new Dictionary<string, int>();
    }

    // ========================================
    // Level: get/set + persist
    // ========================================
    public int GetLevel(string id)
    {
        if (string.IsNullOrEmpty(id) || !_byId.ContainsKey(id)) return 0;

        var def = _byId[id];
        if (def.kind == UpgradeKind.Team)
        {
            return SaveManager.Data.teamUpgrades.TryGetValue(id, out var lv) ? lv : 0;
        }
        else // PerType
        {
            return SaveManager.Data.typeUpgrades.TryGetValue(id, out var lv) ? lv : 0;
        }
    }

    private void SetLevel(string id, int level)
    {
        if (string.IsNullOrEmpty(id) || !_byId.ContainsKey(id)) return;

        level = Mathf.Max(0, level);
        var def = _byId[id];

        if (def.kind == UpgradeKind.Team)
        {
            SaveManager.Data.teamUpgrades[id] = level;
        }
        else
        {
            SaveManager.Data.typeUpgrades[id] = level;
        }
        Persist();
    }

    private static void Persist()
    {
        // Dùng SaveManager.Save() để tương thích mã sẵn có của bạn
        SaveManager.Save();
    }

    // ========================================
    // Giá nâng cấp & mua
    // ========================================
    public long GetPriceNext(string id)
    {
        if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out var def)) return long.MaxValue;

        int lv = GetLevel(id);
        double cost = def.baseCost * Math.Pow(def.growth, lv);
        if (cost < 1d) cost = 1d;
        return (long)Math.Round(cost);
    }

    public bool TryBuy(string id)
    {
        long price = GetPriceNext(id);
        if (!EconomyManager.I.TrySpend(price)) return false;

        SetLevel(id, GetLevel(id) + 1);
        return true;
    }

    // ========================================
    // Query: Team multipliers/effects
    // ========================================
    public float TeamDamageMul()
    {
        if (!_byId.TryGetValue(UpgradeIds.Team_Damage, out var d)) return 1f;
        int lv = GetLevel(d.id);
        return Mathf.Pow(1f + d.teamDamagePct, lv);
    }

    public float TeamASMul()
    {
        if (!_byId.TryGetValue(UpgradeIds.Team_AS, out var d)) return 1f;
        int lv = GetLevel(d.id);
        return Mathf.Pow(1f + d.teamASPct, lv);
    }

    /// <summary>
    /// Tổng phần trăm giảm giá (0..1). Khi tính giá gà nhớ áp CAP 50% (min với 0.5f).
    /// </summary>
    public float TeamDiscountFrac()
    {
        if (!_byId.TryGetValue(UpgradeIds.Team_Discount, out var d)) return 0f;
        int lv = GetLevel(d.id);
        float total = lv * d.discountPct; // vd 3%/lv
        return Mathf.Max(0f, total);
    }

    /// <summary>
    /// Tổng số giây giảm vào thời gian nạp trứng (Egg Load). Tuỳ nơi dùng.
    /// </summary>
    public float TeamEggSeconds()
    {
        if (!_byId.TryGetValue(UpgradeIds.Team_EggSpeed, out var d)) return 0f;
        int lv = GetLevel(d.id);
        return lv * d.eggSeconds;
    }

    /// <summary>
    /// Tổng coin/s cộng thêm.
    /// </summary>
    public long TeamMPS()
    {
        if (!_byId.TryGetValue(UpgradeIds.Team_MPS, out var d)) return 0;
        int lv = GetLevel(d.id);
        long v = lv * d.mpsValue;
        return v < 0 ? 0 : v;
    }

    // ========================================
    // Query: Per-type multiplier
    // ========================================
    public float PerTypeMul(ChickenType type)
    {
        string id = UpgradeIds.TypeKey(type);
        if (!_byId.TryGetValue(id, out var d)) return 1f;
        int lv = GetLevel(id);
        return Mathf.Pow(1f + d.perTypeDpsPct, lv);
    }

    // ========================================
    // Tiện ích (tùy chọn): kiểm tra asset/ID
    // ========================================
#if UNITY_EDITOR
    [ContextMenu("Validate Upgrade IDs in Console")]
    private void ValidateInConsole()
    {
        foreach (var kv in _byId)
        {
            var d = kv.Value;
            string extra = d.kind == UpgradeKind.PerType ? $" (PerType:{d.typeTarget})" : " (Team)";
            Debug.Log($"[UpgradeDef] id={d.id} base={d.baseCost} growth={d.growth}{extra}");
        }
    }
#endif
}
