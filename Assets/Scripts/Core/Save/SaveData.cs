using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public long coin = 0;
    public int waveIndex = 0;
    public long lastOnlineUtcTicks = DateTime.UtcNow.Ticks;

    // Flags / options
    public bool autoStart = false;

    // Upgrades
    public Dictionary<string, int> teamUpgrades = new();
    public Dictionary<string, int> typeUpgrades = new();

    // Slots
    public int equipSlots = 5;
    public int prepareSlots = 12;

    // Anti-tamper
    public string hash = ""; // lưu SHA256 cho các trường quan trọng
}
