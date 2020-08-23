using System;


[Serializable]
public enum ItemType
{
    WEAPON, HELMET, SHIELD, GLOVES, CHEST, BOOTS
}

public enum StatType
{
    ATTACK
}

[Serializable]
public class Item
{
    //
    // NOTE: not using properties because JsonUtility does not support them... :/
    //
    //public string Name { get; set; }
    //public int StatIncrease { get; set; }

    public string Name = "";
    public ItemType ItemType = ItemType.WEAPON;
    public StatType StatType = StatType.ATTACK; 
    public int StatIncrease = 0;

    public override string ToString()
    {
        return string.Format("Name: {0}, ItemType: {1}, StatType: {2}, StatIncrease: {3}", Name, ItemType, StatType, StatIncrease);
    }
}


