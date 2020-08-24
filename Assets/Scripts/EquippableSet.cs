
using System;

[Serializable]
public class EquippableSet 
{
    // NOTE: not using properties because JsonUtility does not support them... :/
    public Item Weapon;
    public Item Helmet;
    public Item Shield;
    public Item Gloves;
    public Item Chest;
    public Item Boots;

    public override string ToString()
    {
        return string.Format("Weapon: {0}, Helmet: {1}, Shield {2}, Gloves: {3}, Chest: {4}, Boots: {5}", Weapon, Helmet, Shield, Gloves, Chest, Boots);
    }
}
