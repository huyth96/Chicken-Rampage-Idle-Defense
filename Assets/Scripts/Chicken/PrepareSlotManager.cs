// Assets/Scripts/Chicken/PrepareSlots.cs
using System;
using UnityEngine;

/// <summary>
/// Bãi chờ 12 slot cho gà.
/// - Hoạt động tương tự EquipSlots.
/// - Tự lắng nghe sự kiện ShopManager.OnChickenPurchased.
/// - Khi Equip full, gà mới mua sẽ tự động vào Prepare.
/// </summary>
public class PrepareSlots : MonoBehaviour
{
    [Serializable]
    public class Slot
    {
        public Transform mountPoint;    // nơi đặt prefab
        public ChickenUnit current;     // instance đang có
    }

    [Serializable]
    public struct PrefabMap
    {
        public ChickenType type;
        public ChickenUnit prefab;
    }

    [Header("References")]
    [Tooltip("Equip chính trong trận, dùng để kiểm tra full hay chưa")]
    public EquipSlots equipSlots;
    [Tooltip("ShopManager (để nghe sự kiện mua gà)")]
    public ShopManager shop;

    [Header("Prepare Slots (12)")]
    public Slot[] slots = new Slot[12];

    [Header("Prefabs")]
    public ChickenUnit chickenPrefab;
    public PrefabMap[] typePrefabs;

    public int SlotCount => slots?.Length ?? 0;
    public bool IsValidIndex(int index) => index >= 0 && index < SlotCount;

    public event Action OnRosterChanged;
    public event Action<int, ChickenUnit> OnAssigned;
    public event Action<int> OnRemoved;

    // ==================== UNITY ====================
    private void Awake()
    {
        if (!shop) shop = FindFirstObjectByType<ShopManager>();
        if (!equipSlots) equipSlots = FindFirstObjectByType<EquipSlots>();
    }

    private void OnEnable()
    {
        if (shop != null)
            shop.OnChickenPurchased += OnChickenBought;
    }

    private void OnDisable()
    {
        if (shop != null)
            shop.OnChickenPurchased -= OnChickenBought;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // đảm bảo luôn 12 slot
        if (slots == null || slots.Length != 12)
        {
            var newSlots = new Slot[12];
            if (slots != null)
            {
                int copy = Mathf.Min(slots.Length, 12);
                for (int i = 0; i < copy; i++) newSlots[i] = slots[i];
            }
            slots = newSlots;
        }

        // cảnh báo thiếu mount
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].mountPoint == null)
                Debug.LogWarning($"[PrepareSlots] Slot {i} thiếu mountPoint.", this);
        }

        if (!chickenPrefab)
            Debug.LogWarning("[PrepareSlots] Chưa gán chickenPrefab fallback.", this);
    }
#endif

    // ==================== CORE ====================

    private void OnChickenBought(ChickenDefinitionSO def)
    {
        if (def == null) return;
        // ShopManager luôn bắn event dù đã add vào Equip,
        // nên chỉ xử lý khi Equip full
        if (equipSlots != null && equipSlots.HasEmpty()) return;

        AssignFirstEmpty(def);
    }

    public bool IsEmpty(int index) => IsValidIndex(index) && slots[index].current == null;

    public bool HasEmpty()
    {
        for (int i = 0; i < SlotCount; i++)
            if (IsEmpty(i)) return true;
        return false;
    }

    public int FirstEmptyIndex()
    {
        for (int i = 0; i < SlotCount; i++)
            if (IsEmpty(i)) return i;
        return -1;
    }

    public ChickenUnit AssignFirstEmpty(ChickenDefinitionSO def)
    {
        int idx = FirstEmptyIndex();
        return idx >= 0 ? Assign(idx, def) : null;
    }

    public ChickenUnit Assign(int slotIndex, ChickenDefinitionSO def)
    {
        if (!IsValidIndex(slotIndex) || def == null) return null;

        var prefab = GetPrefabFor(def);
        if (prefab == null)
        {
            Debug.LogWarning("[PrepareSlots] Missing prefab for " + def.name);
            return null;
        }

        Remove(slotIndex);

        var s = slots[slotIndex];
        var parent = s.mountPoint != null ? s.mountPoint : transform;

        var inst = Instantiate(prefab, parent);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        inst.def = def;
        s.current = inst;
        slots[slotIndex] = s;

        OnAssigned?.Invoke(slotIndex, inst);
        OnRosterChanged?.Invoke();

        Debug.Log($"[PrepareSlots] Đã thêm {def.name} vào slot {slotIndex + 1}");
        return inst;
    }

    public void Remove(int slotIndex)
    {
        if (!IsValidIndex(slotIndex)) return;
        var s = slots[slotIndex];
        if (s.current != null)
        {
            Destroy(s.current.gameObject);
            s.current = null;
            OnRemoved?.Invoke(slotIndex);
            OnRosterChanged?.Invoke();
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < SlotCount; i++) Remove(i);
    }

    // ==================== UTILS ====================
    private ChickenUnit GetPrefabFor(ChickenDefinitionSO def)
    {
        if (typePrefabs != null)
        {
            for (int i = 0; i < typePrefabs.Length; i++)
            {
                if (typePrefabs[i].prefab != null && typePrefabs[i].type == def.type)
                    return typePrefabs[i].prefab;
            }
        }
        return chickenPrefab;
    }

    public float GetTotalDPS()
    {
        float dps = 0f;
        for (int i = 0; i < SlotCount; i++)
        {
            var c = slots[i].current;
            if (c != null && c.def != null) dps += c.TheoreticalDPS;
        }
        return dps;
    }
}
