using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CopySpriteToImage : EditorWindow
{
    [MenuItem("Tools/Copy SpriteRenderer → Existing Image (Selected)")]
    private static void CopySprite()
    {
        GameObject[] objs = Selection.gameObjects;
        if (objs.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection",
                "Hãy chọn ít nhất 1 GameObject trong Hierarchy.",
                "OK");
            return;
        }

        int count = 0;

        foreach (GameObject go in objs)
        {
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            Image img = go.GetComponent<Image>();
            if (sr == null || img == null) continue; // cần cả hai

            // Gán sprite + color
            img.sprite = sr.sprite;
            img.color = sr.color;

            // Disable SpriteRenderer
            sr.enabled = false;

            count++;
        }

        Debug.Log($"✅ Đã sao chép sprite cho {count} GameObject (SpriteRenderer → Image).");
    }
}
