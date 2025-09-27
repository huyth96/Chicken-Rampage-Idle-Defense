// Scripts/Utils/StretchTextToParent.cs
using UnityEngine;
using TMPro;

[ExecuteAlways]
public class StretchTextToParent : MonoBehaviour
{
    public bool applyToAllTMPChildren = true;

    void OnEnable() { Apply(); }
    void OnTransformChildrenChanged() { if (applyToAllTMPChildren) Apply(); }

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        if (applyToAllTMPChildren)
        {
            var tmps = GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in tmps) Stretch(t.rectTransform);
        }
        else
        {
            var tmp = GetComponent<TMP_Text>();
            if (tmp) Stretch(tmp.rectTransform);
        }
    }

    void Stretch(RectTransform rt)
    {
        if (rt == null || rt.parent == null) return;
        rt.anchorMin = Vector2.zero;      // (0,0)
        rt.anchorMax = Vector2.one;       // (1,1)
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;      // Left/Bottom = 0
        rt.offsetMax = Vector2.zero;      // Right/Top = 0
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
    }
}
