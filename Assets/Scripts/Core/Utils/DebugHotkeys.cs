#if UNITY_EDITOR
using UnityEngine;

public class DebugHotkeys : MonoBehaviour
{
    public long step = 10;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals)) EconomyManager.I.AddCoin(step);   // phím '=' hoặc '+'
        if (Input.GetKeyDown(KeyCode.Minus)) EconomyManager.I.TrySpend(step);  // phím '-'
        if (Input.GetKeyDown(KeyCode.Backspace)) SaveManager.WipeAndRecreate();
    }
}
#endif
