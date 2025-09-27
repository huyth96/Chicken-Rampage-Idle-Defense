using UnityEngine;

[ExecuteAlways]
public class AutoCenterUIChildren : MonoBehaviour
{
    void OnTransformChildrenChanged()
    {
        foreach (RectTransform child in transform)
        {
            child.anchorMin = new Vector2(0.5f, 0.5f);
            child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = Vector2.zero;
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }
}
