using TMPro;
using UnityEngine;

public class HUDCoinBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void Start()
    {
        if (coinText == null)
            coinText = GetComponentInChildren<TMP_Text>();

        // init from save
        coinText.text = $"Coin: {EconomyManager.I.Coin}";
        EconomyManager.I.OnCoinChanged += OnCoinChanged;
    }

    private void OnDestroy()
    {
        if (EconomyManager.I != null)
            EconomyManager.I.OnCoinChanged -= OnCoinChanged;
    }

    private void OnCoinChanged(long value)
    {
        coinText.text = $"{value}" ;
    }
}
