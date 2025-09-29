using UnityEngine;

public class TeamAutoAssign : MonoBehaviour
{
    public EquipSlots team;
    public ChickenDefinitionSO soldier;

    void Start()
    {
        if (!team) team = GetComponent<EquipSlots>();
        if (team && soldier)
        {
            team.Assign(0, soldier); // gán 1 gà vào Slot0
        }
    }
}
