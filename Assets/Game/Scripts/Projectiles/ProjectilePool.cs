using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Tooltip("Danh sách các viên đạn có sẵn (kéo vào trong Inspector)")]
    public List<GameObject> projectiles = new List<GameObject>();

    /// <summary>
    /// Lấy ra 1 projectile đang inactive để dùng.
    /// </summary>
    public GameObject Get()
    {
        foreach (var proj in projectiles)
        {
            if (!proj.activeSelf) 
            {
                proj.SetActive(true);
                return proj;
            }
        }

        Debug.LogWarning("ProjectilePool: Không còn viên đạn nào khả dụng!");
        return null;
    }

    /// <summary>
    /// Trả projectile về pool (inactive).
    /// </summary>
    public void Return(GameObject proj)
    {
        if (projectiles.Contains(proj))
        {
            proj.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ProjectilePool: Trả về một projectile không thuộc pool này.");
        }
    }
}
