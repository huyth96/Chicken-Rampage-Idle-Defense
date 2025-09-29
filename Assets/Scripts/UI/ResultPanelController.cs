// Assets/Scripts/UI/ResultPanelController.cs
using TMPro;
using UnityEngine;

public class ResultPanelController : MonoBehaviour
{
    public WaveManager wm;
    public GameObject root;
    public TMP_Text title;
    public TMP_Text waveText;

    void Start()
    {
        if (root) root.SetActive(false);
        if (wm)
        {
            wm.OnWin += ShowWin;
            wm.OnFail += ShowFail;
        }
    }
    void OnDestroy()
    {
        if (wm)
        {
            wm.OnWin -= ShowWin;
            wm.OnFail -= ShowFail;
        }
    }

    void ShowWin()
    {
        if (!root) return;
        root.SetActive(true);
        if (title) title.text = "WIN!";
        if (waveText) waveText.text = $"Wave {wm.CurrentWave - 1} cleared";
    }

    void ShowFail()
    {
        if (!root) return;
        root.SetActive(true);
        if (title) title.text = "FAIL!";
        if (waveText) waveText.text = $"Failed at Wave {wm.CurrentWave}";
    }

    // Hook UI buttons
    public void OnClaim() { wm.ClaimAndContinue(); if (root) root.SetActive(false); }
    public void OnRetry() { wm.Retry(); if (root) root.SetActive(false); }
}
