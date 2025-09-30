// Assets/Editor/AutoAssignUpgradeShop.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AutoAssignUpgradeShop
{
    [MenuItem("Tools/CR/Auto-Assign Upgrade Shop (from selection)")]
    private static void Run()
    {
        var go = Selection.activeGameObject;
        if (!go)
        {
            EditorUtility.DisplayDialog("Auto-Assign", "Hãy chọn GameObject chứa script panel trước.", "OK");
            return;
        }

        // Lấy component target (script panel). Dò theo tên property để khỏi phụ thuộc class name
        var comp = go.GetComponents<MonoBehaviour>()
                     .FirstOrDefault(c => HasNeededProperties(new SerializedObject(c)));
        if (!comp)
        {
            EditorUtility.DisplayDialog("Auto-Assign",
                "Không thấy component nào trên object có các mảng:\nperTypes, typeButtons, typePriceTexts, typeLevelTexts, typeNameTexts",
                "OK");
            return;
        }

        var so = new SerializedObject(comp);
        var pPerTypes = so.FindProperty("perTypes");
        var pButtons = so.FindProperty("typeButtons");
        var pPriceTexts = so.FindProperty("typePriceTexts");
        var pLevelTexts = so.FindProperty("typeLevelTexts");
        var pNameTexts = so.FindProperty("typeNameTexts");

        // Tìm Content
        var content = FindDeepChild(go.transform, new[] { "Scroll_View", "Viewport", "Content" });
        if (content == null)
        {
            EditorUtility.DisplayDialog("Auto-Assign",
                "Không tìm thấy đường dẫn Scroll_View/Viewport/Content dưới GameObject đã chọn.",
                "OK");
            return;
        }

        // Index các nhóm theo tên (normalize: lower, bỏ khoảng trắng/underscore)
        var groupMap = new Dictionary<string, Transform>();
        foreach (Transform child in content)
        {
            var key = Normalize(child.name);
            if (!groupMap.ContainsKey(key))
                groupMap.Add(key, child);
        }

        // Lấy danh sách tên type theo thứ tự mảng Per Types
        var typeNames = new List<string>();
        for (int i = 0; i < pPerTypes.arraySize; i++)
        {
            var elem = pPerTypes.GetArrayElementAtIndex(i);
            string name = null;

            // Trường hợp là enum: dùng string của enum
            if (elem.propertyType == SerializedPropertyType.Enum)
                name = elem.enumDisplayNames[elem.enumValueIndex];
            else
            {
                // Trường hợp là object reference (SO)
                var obj = elem.objectReferenceValue;
                if (obj) name = obj.name;
            }

            if (string.IsNullOrEmpty(name)) name = $"Element{i}";
            typeNames.Add(name);
        }

        // Resize các mảng còn lại theo Per Types
        EnsureSize(pButtons, typeNames.Count);
        EnsureSize(pPriceTexts, typeNames.Count);
        EnsureSize(pLevelTexts, typeNames.Count);
        EnsureSize(pNameTexts, typeNames.Count);

        int assigned = 0;
        var warnings = new List<string>();

        for (int i = 0; i < typeNames.Count; i++)
        {
            var key = Normalize(typeNames[i]);
            if (!groupMap.TryGetValue(key, out var group))
            {
                warnings.Add($"• Không tìm thấy group con khớp tên type “{typeNames[i]}”.");
                continue;
            }

            // Tìm các node con theo convention
            var buyBtnTr = FindDeepChild(group, new[] { "BuyBtn" });
            var priceTr = FindDeepChild(group, new[] { "BuyBtn", "PriceText" });
            var levelTr = FindAny(group, new[] { "Level Text", "LevelText", "LvlText" });
            var nameTr = FindAny(group, new[] { "NameText", "Name Text", "Title", "TypeName" });

            var btn = buyBtnTr ? buyBtnTr.GetComponent<Button>() : null;
            var priceText = priceTr ? priceTr.GetComponent<TMP_Text>() : null;
            var levelText = levelTr ? levelTr.GetComponent<TMP_Text>() : null;
            var nameText = nameTr ? nameTr.GetComponent<TMP_Text>() : null;

            if (!btn) warnings.Add($"• [{typeNames[i]}] thiếu Button tại BuyBtn.");
            if (!priceText) warnings.Add($"• [{typeNames[i]}] thiếu TMP_Text tại BuyBtn/PriceText.");
            if (!levelText) warnings.Add($"• [{typeNames[i]}] thiếu TMP_Text tại Level Text.");
            if (!nameText) warnings.Add($"• [{typeNames[i]}] thiếu TMP_Text tại NameText.");

            pButtons.GetArrayElementAtIndex(i).objectReferenceValue = btn;
            pPriceTexts.GetArrayElementAtIndex(i).objectReferenceValue = priceText;
            pLevelTexts.GetArrayElementAtIndex(i).objectReferenceValue = levelText;
            pNameTexts.GetArrayElementAtIndex(i).objectReferenceValue = nameText;

            assigned++;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(comp);

        var msg = $"Đã auto-assign {assigned} mục theo thứ tự Per Types.";
        if (warnings.Count > 0) msg += "\n\nCảnh báo:\n" + string.Join("\n", warnings);
        EditorUtility.DisplayDialog("Auto-Assign", msg, "OK");
    }

    // ===== helpers =====
    private static bool HasNeededProperties(SerializedObject so)
    {
        return so.FindProperty("perTypes") != null &&
               so.FindProperty("typeButtons") != null &&
               so.FindProperty("typePriceTexts") != null &&
               so.FindProperty("typeLevelTexts") != null &&
               so.FindProperty("typeNameTexts") != null;
    }

    private static Transform FindDeepChild(Transform root, IEnumerable<string> path)
    {
        Transform cur = root;
        foreach (var p in path)
        {
            if (!cur) return null;
            cur = cur.Cast<Transform>().FirstOrDefault(t => Normalize(t.name) == Normalize(p));
        }
        return cur;
    }

    private static Transform FindAny(Transform root, IEnumerable<string> names)
    {
        foreach (var n in names)
        {
            var tr = root.Cast<Transform>().FirstOrDefault(t => Normalize(t.name) == Normalize(n));
            if (tr) return tr;
        }
        // deep search fallback
        foreach (Transform t in root)
        {
            var tr = FindAny(t, names);
            if (tr) return tr;
        }
        return null;
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }

    private static void EnsureSize(SerializedProperty arrayProp, int size)
    {
        if (!arrayProp.isArray) return;
        arrayProp.arraySize = size;
    }
}
#endif
