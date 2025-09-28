using TMPro;
using UnityEngine;

public class BaseHPBinder : MonoBehaviour
{
    public BaseHealth baseHealth;
    public TMP_Text hpText;

    void Update()
    {
        if (baseHealth && hpText)
            hpText.text = $"Base HP: {baseHealth.CurrentHp}";
    }
}
