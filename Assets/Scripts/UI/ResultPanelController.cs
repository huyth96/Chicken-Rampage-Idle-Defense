// Assets/Scripts/UI/ResultPanelController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelController : MonoBehaviour
{
    [Header("Refs")]
    public WaveManager wm;   // nếu để trống, sẽ auto-find
    public GameObject root;

    [Header("Texts")]
    public TMP_Text title;      // "WAVE 10 CLEARED!" hoặc "FAIL!"
    public TMP_Text killText;   // "76"
    public TMP_Text earnText;   // "20,000k"
    public TMP_Text waveText;   // "Wave 10"

    [Header("Buttons")]
    public Button claimBtn;     // Claim 1x
    public Button claim2xBtn;   // Claim 2x (optional)
    public Button retryBtn;     // Retry khi fail

    void Awake()
    {
        if (wm == null) wm = FindFirstObjectByType<WaveManager>();
        if (root) root.SetActive(false);

        if (claimBtn)
        {
            claimBtn.onClick.RemoveAllListeners();
            claimBtn.onClick.AddListener(() => OnClaim(1f));
        }
        if (claim2xBtn)
        {
            claim2xBtn.onClick.RemoveAllListeners();
            claim2xBtn.onClick.AddListener(() => OnClaim(2f));
        }
        if (retryBtn)
        {
            retryBtn.onClick.RemoveAllListeners();
            retryBtn.onClick.AddListener(OnRetry);
        }
        Debug.Log($"ResultPanel: Awake (wm={(wm ? "OK" : "null")})");
    }

    void OnEnable()
    {
        if (wm == null) wm = FindFirstObjectByType<WaveManager>();
        if (wm != null)
        {
            wm.OnWin += ShowWin;
            wm.OnFail += ShowFail;
            Debug.Log("ResultPanel: Subscribed OnWin/OnFail");
        }
        else
        {
            Debug.LogWarning("ResultPanel: WaveManager not found; cannot subscribe events.");
        }
    }

    void OnDisable()
    {
        if (wm != null)
        {
            wm.OnWin -= ShowWin;
            wm.OnFail -= ShowFail;
            Debug.Log("ResultPanel: Unsubscribed OnWin/OnFail");
        }
    }

    void ShowWin()
    {
        if (!root) return;
        root.SetActive(true);

        if (title) title.text = $"WAVE {wm.ResultWaveIndex} CLEARED!";
        if (waveText) waveText.text = $"Wave {wm.ResultWaveIndex}";
        if (killText) killText.text = $"{wm.ResultTotalKill}";
        if (earnText) earnText.text = $"{wm.ResultWaveReward:N0}";

        if (claimBtn) claimBtn.gameObject.SetActive(true);
        if (claim2xBtn) claim2xBtn?.gameObject.SetActive(true);
        if (retryBtn) retryBtn.gameObject.SetActive(false);

        if (claimBtn) claimBtn.interactable = wm.CanClaim;
        if (claim2xBtn) claim2xBtn.interactable = wm.CanClaim;

        Debug.Log("ResultPanel: ShowWin → panel enabled");
    }

    void ShowFail()
    {
        if (!root) return;
        root.SetActive(true);

        if (title) title.text = "FAIL!";
        if (waveText) waveText.text = $"Failed at Wave {wm.CurrentWave}";
        if (killText) killText.text = "-";
        if (earnText) earnText.text = "0";

        if (claimBtn) claimBtn.gameObject.SetActive(false);
        if (claim2xBtn) claim2xBtn?.gameObject.SetActive(false);
        if (retryBtn) retryBtn.gameObject.SetActive(true);
        if (retryBtn) retryBtn.interactable = true;

        Debug.Log("ResultPanel: ShowFail → panel enabled");
    }

    void OnClaim(float multiplier)
    {
        wm.ClaimAndContinue(multiplier);
        if (root) root.SetActive(false);
        Debug.Log($"ResultPanel: Claim x{multiplier}");
    }

    void OnRetry()
    {
        wm.Retry();
        if (root) root.SetActive(false);
        Debug.Log("ResultPanel: Retry");
    }
}
