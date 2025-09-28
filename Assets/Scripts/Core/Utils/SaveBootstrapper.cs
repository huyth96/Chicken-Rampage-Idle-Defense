using System;
using UnityEngine;

public class SaveBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        // Chạm SaveManager.Data để kích hoạt Load() lần đầu
        _ = SaveManager.Data;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveManager.Data.lastOnlineUtcTicks = DateTime.UtcNow.Ticks;
            SaveManager.Save();
        }
    }

    private void OnApplicationQuit()
    {
        SaveManager.Data.lastOnlineUtcTicks = DateTime.UtcNow.Ticks;
        SaveManager.Save();
    }
}
