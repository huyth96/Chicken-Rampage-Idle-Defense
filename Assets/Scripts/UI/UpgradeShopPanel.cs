// Assets/Scripts/UI/UpgradeShopPanel.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopPanel : MonoBehaviour
{
    [Header("Refs")]
    public UpgradeManager upgrades;  // drag từ scene (_Systems)

    [Header("Team Upgrades")]
    public string[] teamIds = {
        UpgradeIds.Team_Damage,
        UpgradeIds.Team_AS,
        UpgradeIds.Team_EggSpeed,
        UpgradeIds.Team_MPS,
        UpgradeIds.Team_Discount
    };
    public Button[] teamButtons;
    public TMP_Text[] teamPriceTexts;
    public TMP_Text[] teamLevelTexts;
    public TMP_Text[] teamNameTexts; // optional đẹp UI

    [Header("Per-Type Upgrades")]
    public ChickenType[] perTypes = {
        ChickenType.Soldier, ChickenType.Rapid, ChickenType.Sniper,
        ChickenType.Shotgun, ChickenType.Rocket, ChickenType.Support
    };
    public Button[] typeButtons;
    public TMP_Text[] typePriceTexts;
    public TMP_Text[] typeLevelTexts;
    public TMP_Text[] typeNameTexts; // optional

    [Header("Refresh")]
    public float uiRefreshInterval = 0.25f;
    float _t;

    void Awake()
    {
        // Wire team
        for (int i = 0; i < teamIds.Length; i++)
        {
            int idx = i;
            if (teamButtons != null && idx < teamButtons.Length && teamButtons[idx] != null)
                teamButtons[idx].onClick.AddListener(() => OnBuyTeam(teamIds[idx]));
        }

        // Wire per-type
        for (int i = 0; i < perTypes.Length; i++)
        {
            int idx = i;
            if (typeButtons != null && idx < typeButtons.Length && typeButtons[idx] != null)
                typeButtons[idx].onClick.AddListener(() => OnBuyType(perTypes[idx]));
        }

        TrySubscribeCoinEvent();
    }

    void OnEnable() { RefreshAll(); }
    void OnDestroy() { TryUnsubscribeCoinEvent(); }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= uiRefreshInterval)
        {
            _t = 0f;
            RefreshAll();
        }
    }

    void OnBuyTeam(string id)
    {
        if (upgrades != null && !string.IsNullOrEmpty(id))
        {
            bool ok = upgrades.TryBuy(id);
            if (ok) RefreshAll();
        }
    }

    void OnBuyType(ChickenType t)
    {
        if (upgrades == null) return;
        bool ok = upgrades.TryBuy(UpgradeIds.TypeKey(t));
        if (ok) RefreshAll();
    }

    void RefreshAll()
    {
        if (upgrades == null) return;

        // TEAM
        for (int i = 0; i < teamIds.Length; i++)
        {
            string id = teamIds[i];
            long price = upgrades.GetPriceNext(id);
            int lv = upgrades.GetLevel(id);

            if (teamPriceTexts != null && i < teamPriceTexts.Length && teamPriceTexts[i] != null)
                teamPriceTexts[i].text = price.ToString();

            if (teamLevelTexts != null && i < teamLevelTexts.Length && teamLevelTexts[i] != null)
                teamLevelTexts[i].text = $"Lv {lv}";

            if (teamNameTexts != null && i < teamNameTexts.Length && teamNameTexts[i] != null)
                teamNameTexts[i].text = id.Replace('_', ' ').ToUpperInvariant();

            // enable/disable theo coin
            if (teamButtons != null && i < teamButtons.Length && teamButtons[i] != null)
            {
                bool canAfford = EconomyManager.I ? (EconomyManager.I.Coin >= price) : true;
                teamButtons[i].interactable = canAfford;
            }
        }

        // PER-TYPE
        for (int i = 0; i < perTypes.Length; i++)
        {
            string id = UpgradeIds.TypeKey(perTypes[i]);
            long price = upgrades.GetPriceNext(id);
            int lv = upgrades.GetLevel(id);

            if (typePriceTexts != null && i < typePriceTexts.Length && typePriceTexts[i] != null)
                typePriceTexts[i].text = price.ToString();

            if (typeLevelTexts != null && i < typeLevelTexts.Length && typeLevelTexts[i] != null)
                typeLevelTexts[i].text = $"Lv {lv}";

            if (typeNameTexts != null && i < typeNameTexts.Length && typeNameTexts[i] != null)
                typeNameTexts[i].text = perTypes[i].ToString().ToUpperInvariant();

            if (typeButtons != null && i < typeButtons.Length && typeButtons[i] != null)
            {
                bool canAfford = EconomyManager.I ? (EconomyManager.I.Coin >= price) : true;
                typeButtons[i].interactable = canAfford;
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
        EconomyManager.I.OnCoinChanged -= _ => RefreshAll();
    }
}
