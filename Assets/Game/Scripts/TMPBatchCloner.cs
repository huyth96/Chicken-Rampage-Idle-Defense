using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TMPCloneToNamedChildren : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_Text templateTMP;        // kéo "Text (TMP)" mẫu vào đây
    public Transform searchRoot;        // kéo PrepareSlots vào đây
    public string targetChildName = "GameObject";

    [Header("Placement")]
    public string clonedName = "Text (TMP)";
    public bool stretchToParent = true;
    public Vector4 padding = Vector4.zero;   // L,T,R,B
    public bool centerKeepSize = false;

    [Header("TMP tweaks")]
    public bool forceCenterAlignment = true;
    public bool enableAutoSizing = true;
    public float autoMin = 12f, autoMax = 48f;

#if UNITY_EDITOR
    [CustomEditor(typeof(TMPCloneToNamedChildren))]
    private class E : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Clone To All"))
                ((TMPCloneToNamedChildren)target).CloneToAll();
        }
    }
#endif

    [ContextMenu("Clone To All")]
    public void CloneToAll()
    {
        // Guard
        if (templateTMP == null) { Debug.LogError("[TMPClone] TemplateTMP chưa gán."); return; }
        if (searchRoot == null) { Debug.LogError("[TMPClone] SearchRoot chưa gán."); return; }

        int count = 0;
        foreach (Transform t in searchRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != targetChildName) continue;

            // bảo đảm parent là UI
            if (t.GetComponent<RectTransform>() == null)
            {
                Debug.LogWarning($"[TMPClone] Bỏ qua '{t.GetHierarchyPath()}' vì không phải RectTransform (không nằm trong UI).");
                continue;
            }

            // Xóa bản cũ trùng tên nếu có
            var old = t.Find(clonedName);
            if (old != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) Undo.DestroyObjectImmediate(old.gameObject);
                else Destroy(old.gameObject);
#else
                DestroyImmediate(old.gameObject);
#endif
            }

            // Clone an toàn cho cả prefab lẫn scene object
            TMP_Text tmp = SafeInstantiateUnder(templateTMP, t);
            if (tmp == null)
            {
                Debug.LogError($"[TMPClone] Instantiate thất bại ở parent '{t.GetHierarchyPath()}'. Kiểm tra TemplateTMP có bị missing/prefab lỗi không?");
                continue;
            }
            tmp.name = clonedName;

            // Layout
            var rt = tmp.rectTransform;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.pivot = new Vector2(0.5f, 0.5f);

            if (stretchToParent && !centerKeepSize)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                // padding: L,T,R,B
                rt.offsetMin = new Vector2(padding.x, -padding.w);
                rt.offsetMax = new Vector2(-padding.z, padding.y);
            }
            else
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }

            // TMP settings
            if (forceCenterAlignment) tmp.alignment = TextAlignmentOptions.Center;
            if (enableAutoSizing)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = autoMin;
                tmp.fontSizeMax = autoMax;
            }

            count++;
        }

        Debug.Log($"[TMPClone] Done. Cloned into {count} '{targetChildName}' objects.");
    }

    // ---------- Helpers ----------
    private TMP_Text SafeInstantiateUnder(TMP_Text source, Transform parent)
    {
        TMP_Text inst = null;

#if UNITY_EDITOR
        var assetType = PrefabUtility.GetPrefabAssetType(source);
        bool isPrefabAsset = assetType != PrefabAssetType.NotAPrefab;
        if (!Application.isPlaying && isPrefabAsset)
        {
            var obj = PrefabUtility.InstantiatePrefab(source.gameObject, parent) as GameObject;
            inst = obj ? obj.GetComponent<TMP_Text>() : null;
        }
        else
        {
            inst = Instantiate(source, parent);
        }
#else
        inst = Instantiate(source, parent);
#endif
        return inst;
    }
}

// tiện log path đầy đủ trong Hierarchy
public static class TransformPathExt
{
    public static string GetHierarchyPath(this Transform t)
    {
        if (t == null) return "<null>";
        var path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}
