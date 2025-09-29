// Assets/Scripts/Wave/EnemyHook.cs
using UnityEngine;

// Gắn vào prefab Enemy để WaveManager biết khi nào đã clear hết
[RequireComponent(typeof(Enemy))]
public class EnemyHook : MonoBehaviour
{
    void OnEnable()
    {
        if (EnemyRegistry.I) EnemyRegistry.I.Register();
    }

    void OnDisable()
    {
        if (EnemyRegistry.I) EnemyRegistry.I.Unregister();
    }
}
