// Assets/Scripts/Chicken/EquipSlots.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipSlots : MonoBehaviour
{
    [Serializable]
    public class Slot
    {
        public Transform mountPoint;    // nơi đặt prefab gà (empty child)
        public ChickenUnit current;     // instance runtime
    }

    [Serializable]
    public struct PrefabMap
    {
        public ChickenType type;        // Soldier/Rapid/Sniper/Shotgun/Rocket/Support
        public ChickenUnit prefab;      // prefab riêng cho loại đó (nếu có)
    }

    [Header("Slots (5)")]
    public Slot[] slots = new Slot[5];

    [Header("Prefab mặc định (fallback)")]
    public ChickenUnit chickenPrefab;

    [Header("Prefab theo loại (tuỳ chọn)")]
    public PrefabMap[] typePrefabs;     // map type -> prefab riêng (ưu tiên dùng)

    public int SlotCount => slots?.Length ?? 0;
    public bool IsValidIndex(int index) => index >= 0 && index < SlotCount;

    // Sự kiện UI có thể nghe để cập nhật icon/hud
    public event Action OnRosterChanged;
    public event Action<int, ChickenUnit> OnAssigned;
    public event Action<int> OnRemoved;

    // ========== Public helpers ==========
    public bool IsEmpty(int index) => IsValidIndex(index) && slots[index].current == null;

    public bool HasEmpty()
    {
        for (int i = 0; i < SlotCount; i++) if (IsEmpty(i)) return true;
        return false;
    }

    public int FirstEmptyIndex()
    {
        for (int i = 0; i < SlotCount; i++) if (IsEmpty(i)) return i;
        return -1;
    }

    public ChickenUnit AssignFirstEmpty(ChickenDefinitionSO def)
    {
        int idx = FirstEmptyIndex();
        return idx >= 0 ? Assign(idx, def) : null;
    }

    // ========== Core ==========

    public ChickenUnit Assign(int slotIndex, ChickenDefinitionSO def)
    {
        if (!IsValidIndex(slotIndex) || def == null) return null;

        var prefab = GetPrefabFor(def);
        if (prefab == null)
        {
            Debug.LogWarning("[EquipSlots] Missing prefab for " + def.name);
            return null;
        }

        // Clear slot cũ (nếu có)
        Remove(slotIndex);

        // Spawn mới
        var s = slots[slotIndex];
        var parent = s.mountPoint != null ? s.mountPoint : transform;

        var inst = Instantiate(prefab, parent);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        inst.def = def;  // 👈 bind loại gà (stat/behaviour lấy từ def)
        s.current = inst;

        OnAssigned?.Invoke(slotIndex, inst);
        OnRosterChanged?.Invoke();
        return inst;
    }

    public void Remove(int slotIndex)
    {
        if (!IsValidIndex(slotIndex)) return;
        var s = slots[slotIndex];
        if (s.current != null)
        {
            Destroy(s.current.gameObject); // TODO: đổi sang pool nếu cần
            s.current = null;
            OnRemoved?.Invoke(slotIndex);
            OnRosterChanged?.Invoke();
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < SlotCount; i++) Remove(i);
    }

    public float GetTeamDPS()
    {
        float dps = 0f;
        for (int i = 0; i < SlotCount; i++)
        {
            var c = slots[i].current;
            if (c != null && c.def != null) dps += c.TheoreticalDPS;
        }
        return dps;
    }

    // ========== Internal ==========

    ChickenUnit GetPrefabFor(ChickenDefinitionSO def)
    {
        // Ưu tiên prefab theo loại (nếu map)
        if (typePrefabs != null)
        {
            for (int i = 0; i < typePrefabs.Length; i++)
            {
                if (typePrefabs[i].prefab != null && typePrefabs[i].type == def.type)
                    return typePrefabs[i].prefab;
            }
        }
        // Fallback về prefab mặc định
        return chickenPrefab;
    }
}
