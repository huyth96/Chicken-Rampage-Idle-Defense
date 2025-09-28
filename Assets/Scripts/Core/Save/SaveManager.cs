using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

public static class SaveManager
{
    private const string FileName = "save.json";
    private const string Salt = "CR_IDLE_DEFENSE_v1_SALT"; // đổi khi phát hành
    private static SaveData _cache;

    public static SaveData Data => _cache ??= Load();

    static string FullPath => Path.Combine(Application.persistentDataPath, FileName);

    public static SaveData Load()
    {
        try
        {
            if (!File.Exists(FullPath))
            {
                var fresh = new SaveData();
                fresh.hash = ComputeHash(fresh);
                return fresh;
            }

            var json = File.ReadAllText(FullPath);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return Fresh();

            // Verify hash (chỉ kiểm các trường kinh tế/tiến trình)
            var expected = ComputeHash(data);
            if (!string.Equals(expected, data.hash, StringComparison.Ordinal))
            {
                Debug.LogWarning("[SaveManager] Hash mismatch! Possible tamper. Resetting sensitive fields.");
                // Phòng thủ mềm: giữ cấu trúc, reset trường nhạy cảm
                data.coin = Math.Max(0, data.coin);
                data.waveIndex = Math.Max(0, data.waveIndex);
                data.lastOnlineUtcTicks = DateTime.UtcNow.Ticks;
                data.teamUpgrades ??= new();
                data.typeUpgrades ??= new();
                data.hash = ComputeHash(data);
            }
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load error: {e.Message}");
            return Fresh();
        }
    }

    public static void Save()
    {
        try
        {
            if (_cache == null) _cache = Fresh();
            _cache.hash = ComputeHash(_cache);
            var json = JsonUtility.ToJson(_cache, prettyPrint: true);
            File.WriteAllText(FullPath, json, Encoding.UTF8);
#if UNITY_EDITOR
            Debug.Log($"[SaveManager] Saved → {FullPath}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save error: {e.Message}");
        }
    }

    public static void MarkDirtyAndSave() => Save();

    public static void WipeAndRecreate()
    {
        try { if (File.Exists(FullPath)) File.Delete(FullPath); } catch { }
        _cache = Fresh();
        Save();
    }

    private static SaveData Fresh()
    {
        var s = new SaveData();
        s.hash = ComputeHash(s);
        return s;
    }

    private static string ComputeHash(SaveData d)
    {
        // Gộp các trường quan trọng để hash
        var payload = $"{Salt}|coin:{d.coin}|wave:{d.waveIndex}|ticks:{d.lastOnlineUtcTicks}|eq:{d.equipSlots}|prep:{d.prepareSlots}";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
