using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager I { get; private set; }
    void Awake() { if (I && I != this) { Destroy(gameObject); return; } I = this; DontDestroyOnLoad(gameObject); }

    [Header("Refs")]
    public EquipSlots equipSlots;   // 5 slot có sẵn
    public UpgradeManager upgrades; // để đọc discount

    public event Action<ChickenDefinitionSO> OnChickenPurchased;

    const string KEY_BOUGHT = "__stat_total_chicken_bought";

    int TotalBought()
    {
        var dict = SaveManager.Data.teamUpgrades; // tái dùng map có sẵn
        return dict != null && dict.TryGetValue(KEY_BOUGHT, out var v) ? v : 0;
    }
    void IncBought()
    {
        var dict = SaveManager.Data.teamUpgrades;
        if (!dict.ContainsKey(KEY_BOUGHT)) dict[KEY_BOUGHT] = 0;
        dict[KEY_BOUGHT] += 1;
        SaveManager.MarkDirtyAndSave();
    }

    public long GetChickenPrice(ChickenDefinitionSO def)
    {
        if (!def) return long.MaxValue;
        int total = TotalBought();
        double price = Math.Max(1, def.basePrice) * Math.Pow(1.15, total);
        float disc = Mathf.Min(0.5f, upgrades?.TeamDiscountFrac() ?? 0f); // cap 50%
        price *= (1.0 - disc);
        return (long)Math.Round(price);
    }

    public bool TryBuyChicken(ChickenDefinitionSO def)
    {
        long price = GetChickenPrice(def);
        if (!EconomyManager.I.TrySpend(price)) return false;

        IncBought();

        // ưu tiên gán vào EquipSlots còn trống
        if (equipSlots)
        {
            for (int i = 0; i < equipSlots.SlotCount; i++)
            {
                if (equipSlots.slots[i].current == null)
                {
                    equipSlots.Assign(i, def);
                    OnChickenPurchased?.Invoke(def);
                    return true;
                }
            }
        }
        // Nếu Equip full: phát sự kiện để Prepare/Inventory nhận
        OnChickenPurchased?.Invoke(def);
        return true;
    }
}
