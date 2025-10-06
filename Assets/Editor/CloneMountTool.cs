// Assets/Editor/CloneMountTool.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CloneMountTool : EditorWindow
{
    [Header("Source")]
    public RectTransform sourceMount;   // drag "Mount" gốc vào đây

    [Header("Targets")]
    public GameObject[] targetParents;  // kéo các GameObject đích (cha sẽ chứa Mount sao chép)

    [Header("Options")]
    public string clonedName = "Mount";
    public bool replaceIfExists = true;
    public float rectY = 0.36f;         // Y của RectTransform (anchoredPosition.y)
    public bool alsoSetLocalY = true;   // nếu parent không phải Canvas/RectTransform chuẩn

    [MenuItem("Tools/Clone Mount To Targets")]
    public static void Open() => GetWindow<CloneMountTool>("Clone Mount");

    void OnGUI()
    {
        var so = new SerializedObject(this);
        so.Update();
        EditorGUILayout.PropertyField(so.FindProperty(nameof(sourceMount)));
        EditorGUILayout.PropertyField(so.FindProperty(nameof(targetParents)), true);
        EditorGUILayout.PropertyField(so.FindProperty(nameof(clonedName)));
        EditorGUILayout.PropertyField(so.FindProperty(nameof(replaceIfExists)));
        EditorGUILayout.PropertyField(so.FindProperty(nameof(rectY)));
        EditorGUILayout.PropertyField(so.FindProperty(nameof(alsoSetLocalY)));
        so.ApplyModifiedProperties();

        GUI.enabled = sourceMount && targetParents != null && targetParents.Length > 0;
        if (GUILayout.Button("Clone to Targets")) DoClone();
        GUI.enabled = true;

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Kéo RectTransform 'Mount' nguồn vào ô Source.\n" +
            "Kéo tất cả GameObject đích (ví dụ các 'GameObject' con trong PrepareSlot) vào Targets.\n" +
            "Tool sẽ tạo/cập nhật child tên 'Mount' dưới mỗi target và set RectTransform Y = 0.36.",
            MessageType.Info);
    }

    void DoClone()
    {
        if (!sourceMount) return;

        foreach (var parent in targetParents)
        {
            if (!parent) continue;

            // tìm Mount sẵn có
            Transform existing = parent.transform.Find(clonedName);
            if (existing && !replaceIfExists) continue;

            Undo.RegisterFullObjectHierarchyUndo(parent, "Clone Mount");

            // nếu có cũ và replace bật -> xóa
            if (existing && replaceIfExists)
                Undo.DestroyObjectImmediate(existing.gameObject);

            // clone
            var clone = (RectTransform)PrefabUtility.InstantiatePrefab(sourceMount.gameObject) as RectTransform;
            if (clone == null) clone = Instantiate(sourceMount);

            clone.gameObject.name = clonedName;
            Undo.RegisterCreatedObjectUndo(clone.gameObject, "Create Cloned Mount");

            // parent
            clone.SetParent(parent.transform, false); // giữ RectTransform values (worldPositionStays=false)

            // set anchoredPosition.y = rectY, giữ nguyên X
            var rt = clone;
            var ap = rt.anchoredPosition;
            ap.y = rectY;
            rt.anchoredPosition = ap;

            // Phòng trường hợp parent không phải layout UI chuẩn
            if (alsoSetLocalY)
            {
                var lp = rt.localPosition;
                lp.y = rectY;
                rt.localPosition = lp;
            }

            EditorUtility.SetDirty(clone);
            EditorUtility.SetDirty(parent);
        }

        Debug.Log($"✅ Cloned Mount to {targetParents.Length} target(s), Y set to {rectY}.");
    }
}
