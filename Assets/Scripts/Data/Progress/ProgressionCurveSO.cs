using UnityEngine;

[CreateAssetMenu(menuName = "CR/Data/ProgressionCurve")]
public class ProgressionCurveSO : ScriptableObject
{
    [Header("Zombie / Economy growth")]
    public float hpLinear = 0.12f;      // HP tăng 12% mỗi wave
    public float bountyLinear = 0.05f;  // Coin drop tăng 5% mỗi wave
    public float waveRewardGrowth = 1.18f; // Mốc thưởng theo hàm mũ

    [Header("Session")]
    public float targetDuration = 25f;  // Thời gian mục tiêu 1 wave (giúp generator cân chỉnh số enemy)
    public float offlineFactor = 0.65f; // Hệ số thưởng offline
    public int offlineCapHours = 8;     // Giới hạn tính thưởng offline
}

