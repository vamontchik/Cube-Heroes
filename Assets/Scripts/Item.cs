using System;


[Serializable]
public enum ItemType
{
    WEAPON, HELMET, SHIELD, GLOVES, CHEST, BOOTS
}

[Serializable]
public enum StatType
{
    ATTACK, DEFENSE, HEALTH, CRIT_RATE, CRIT_DAMAGE, SPEED
}

[Serializable]
public class Item
{
    // NOTE: not using properties because JsonUtility does not support them... :/
    public string Name;
    public ItemType ItemType;
    public StatType StatType;
    public int StatIncrease;

    public override string ToString()
    {
        return string.Format("Name: {0}, ItemType: {1}, StatType: {2}, StatIncrease: {3}", Name, ItemType, StatType, StatIncrease);
    }
}


