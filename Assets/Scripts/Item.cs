using System;

//
// NOTE: not using properties because JsonUtility does not support them... :/
//

[Serializable]
public enum ItemType
{
    WEAPON, HELMET, SHIELD, GLOVES, CHEST, BOOTS
}

[Serializable]
public class Item
{
    //public string Name { get; set; }
    //public int StatIncrease { get; set; }

    public string Name = "";
    public ItemType Type = ItemType.WEAPON;
    public int StatIncrease = 0;

    public override string ToString()
    {
        return string.Format("Name: {0}, Type: {1}, StatIncrease: {2}", Name, Type, StatIncrease);
    }
}


