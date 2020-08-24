using System;

public class RandomGenerator
{
    public static readonly Random rand = new Random();

    public Item RandomItem(int level)
    {
        // TODO: instead of 2, use enum length values once we impl all item types...
        ItemType newItemType = (ItemType) rand.Next(/*inclusive*/ 0, /*exclusive*/ 3); // Enum.GetNames(typeof(ItemType)).Length); // see: https://stackoverflow.com/a/856165
        StatType newStatType = GetCorrespondingStat(newItemType);
        int newStatIncrease = GetStatIncrease(level, newStatType);

        return new Item()
        {
            Name = "item",
            ItemType = newItemType,
            StatType = newStatType,
            StatIncrease = newStatIncrease
        };
    }

    private int GetStatIncrease(int level, StatType statType)
    {
        // TODO: impl the rest of StatType...

        int value;

        switch (statType)
        {
            // NOTE:
            // going for "fair" drops, 
            // where 50% will be greater than next level up cube,
            // and 50% will be less than next level up cube

            case StatType.ATTACK:
                // [5*level - 5, 5*level + 5] , level starts @ 1 so starts @ [0, 10]
                value = 5 * (level - 1) + rand.Next(/*inclusive*/ 0, /*exclusive*/ 10 + 1);    
                break;
            case StatType.HEALTH:
                // [25*level - 25, 25*level + 25] , level starts @ 1 so starts @ [0, 50]
                value = 25 * (level - 1) + rand.Next(/*inclusive*/ 0, /* exclusive */ 50 + 1); 
                break;
            case StatType.DEFENSE:
                // [level - 1, level + 1] , level starts @ 1 so starts @ [0, 2]
                value = (level - 1) + rand.Next(/*inc*/ 0, /*ex*/ 2 + 1);
                break;
            default:
                value = 0;
                break;
        }

        return value;
    }

    private StatType GetCorrespondingStat(ItemType itemType)
    {
        // TODO: impl the rest of ItemType...

        StatType ret;

        switch (itemType)
        {
            case ItemType.WEAPON:
                ret = StatType.ATTACK;
                break;
            case ItemType.HELMET:
                ret = StatType.HEALTH;
                break;
            case ItemType.SHIELD:
                ret = StatType.DEFENSE;
                break;
            default:
                ret = StatType.ATTACK;
                break;
        }
        return ret;
    }
}
