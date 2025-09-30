// Assets/Scripts/UI/ChickenShopPanel.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChickenShopPanel : MonoBehaviour
{
    [Header("Refs")]
    public ShopManager shop;                 // drag từ scene (_Systems)
    public UpgradeManager upgrades;          // drag từ scene (_Systems)

    [Header("Chicken Catalog")]
    public ChickenDefinitionSO[] chickens;   // danh sách gà bán

    [Header("UI Bindings (1-1 với chickens)")]
    public Button[] buyButtons;              // mỗi con gà 1 nút Buy
    public TMP_Text[] priceTexts;            // label giá
    public TMP_Text[] nameTexts;             // label tên (optional)
    public TMP_Text[] dpsTexts;              // DPS lý thuyết (optional)

    [Header("Refresh")]
    public float uiRefreshInterval = 0.25f;  // tối ưu thay vì Update mỗi frame
    float _t;

    void Awake()
    {
        // Wire click
        for (int i = 0; i < chickens.Length; i++)
        {
            int idx = i;
            if (buyButtons != null && idx < buyButtons.Length && buyButtons[idx] != null)
                buyButtons[idx].onClick.AddListener(() => OnBuy(idx));
        }

        // Nếu EconomyManager có OnCoinChanged thì lắng nghe để refresh tức thì
        TrySubscribeCoinEvent();
    }

    void OnEnable()
    {
        RefreshAll();
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= uiRefreshInterval)
        {
            _t = 0f;
            RefreshAll();
        }
    }

    void OnDestroy()
    {
        TryUnsubscribeCoinEvent();
    }

    void OnBuy(int idx)
    {
        if (idx < 0 || idx >= chickens.Length) return;
        if (shop != null && chickens[idx] != null)
        {
            bool ok = shop.TryBuyChicken(chickens[idx]);
            if (ok) RefreshAll();
        }
    }

    void RefreshAll()
    {
        if (shop == null || chickens == null) return;

        for (int i = 0; i < chickens.Length; i++)
        {
            var c = chickens[i];
            if (!c) continue;

            long price = shop.GetChickenPrice(c);

            if (priceTexts != null && i < priceTexts.Length && priceTexts[i] != null)
                priceTexts[i].text = price.ToString();

            if (nameTexts != null && i < nameTexts.Length && nameTexts[i] != null)
                nameTexts[i].text = string.IsNullOrEmpty(c.name) ? $"Chicken {i + 1}" : c.name;

            if (dpsTexts != null && i < dpsTexts.Length && dpsTexts[i] != null)
            {
                // DPS preview: dùng công thức giống ChickenUnit
                double avgCritMul = 1.0 + (double)c.critChance * ((double)c.critMultiplier - 1.0);
                float dmgMul = (UpgradeManager.I ? UpgradeManager.I.TeamDamageMul() : 1f)
                              * (UpgradeManager.I ? UpgradeManager.I.PerTypeMul(c.type) : 1f);
                float asMul = (UpgradeManager.I ? UpgradeManager.I.TeamASMul() : 1f);
                double perShot = (double)c.baseDamage * dmgMul * avgCritMul;
                double dps = perShot * (double)(c.rateOfFire * asMul);
                dpsTexts[i].text = Mathf.RoundToInt((float)dps).ToString();
            }

            // enable/disable nút theo coin
            if (buyButtons != null && i < buyButtons.Length && buyButtons[i] != null)
            {
                bool canAfford = EconomyManager.I ? (EconomyManager.I.Coin >= price) : true;
                buyButtons[i].interactable = canAfford;
            }
        }
    }

    void TrySubscribeCoinEvent()
    {
        if (EconomyManager.I == null) return;
        EconomyManager.I.OnCoinChanged += _ => RefreshAll();
    }
    void TryUnsubscribeCoinEvent()
    {
        if (EconomyManager.I == null) return;
        EconomyManager.I.OnCoinChanged -= _ => RefreshAll(); // không hại nếu không trùng delegate
    }
}
