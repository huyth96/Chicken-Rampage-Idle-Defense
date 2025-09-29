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
        public ChickenUnit current;     // instance đang gắn (runtime)
    }

    [Header("Slots (5)")]
    public Slot[] slots = new Slot[5];

    [Header("Prefab gà mặc định")]
    public ChickenUnit chickenPrefab; // prefab có ChickenUnit + sprite/renderer

    public int SlotCount => slots != null ? slots.Length : 0;

    public bool IsValidIndex(int index) => index >= 0 && index < SlotCount;

    public ChickenUnit Assign(int slotIndex, ChickenDefinitionSO def)
    {
        if (!IsValidIndex(slotIndex) || def == null || chickenPrefab == null) return null;

        // Clear nếu đã có
        Remove(slotIndex);

        var s = slots[slotIndex];
        var inst = Instantiate(chickenPrefab, s.mountPoint != null ? s.mountPoint : transform);
        inst.transform.localPosition = Vector3.zero;
        inst.def = def;
        s.current = inst;

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

    // (tùy chọn) Lưu/đọc roster rất gọn: lưu id SO theo Resources path hoặc GUID.
    // Ở bản Sprint 2 này mình để runtime; bạn có thể gắn vào SaveManager sau.
}
