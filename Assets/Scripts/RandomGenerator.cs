using System;

public class RandomGenerator
{
    public static readonly Random rand = new Random();

    public Item RandomItem(int level)
    {
        ItemType newItemType = (ItemType) rand.Next(/*inclusive*/ 0, /*exclusive*/ Enum.GetNames(typeof(ItemType)).Length); // see: https://stackoverflow.com/a/856165
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
        int value;

        switch (statType)
        {
            // NOTE:
            // for "even distribution around enemy stat" drops, 
            // where 50% will be greater than next level up cube,
            // and 50% will be less than next level up cube

            case StatType.ATTACK:
                // Note: even distribution around enemy stat
                // [5*level - 5, 5*level + 5] , level starts @ 1 so starts @ [0, 10]
                value = 5 * (level - 1) + rand.Next(/*inclusive*/ 0, /*exclusive*/ 10 + 1);    
                break;
            case StatType.HEALTH:
                // Note: even distribution around
                // [25*level - 25, 25*level + 25] , level starts @ 1 so starts @ [0, 50]
                value = 25 * (level - 1) + rand.Next(/*inclusive*/ 0, /* exclusive */ 50 + 1); 
                break;
            case StatType.DEFENSE:
                // Note: even distribution around enemy stat
                // [level - 1, level + 1] , level starts @ 1 so starts @ [0, 2]
                value = (level - 1) + rand.Next(/*inc*/ 0, /*ex*/ 2 + 1);
                break;
            case StatType.CRIT_RATE:
                // Note: match enemy
                // [1, 1] --(every 4)--> [2, 2]
                value = 1 + (int)((level - 1.0) / 4.0);
                break;
            case StatType.CRIT_DAMAGE:
                // Note: match enemy
                // [0.001, 0.001] --> [0.002, 0.002] , except multi by 1000 for decimal pts... divide later
                value = level;
                break;
            case StatType.SPEED:
                // NOTE: always maintain (at most) double speed for next level!
                // [0.1, 0.2] -> [0.2, 0.4] , except multi by 10 for decimal pts... divide later
                value = rand.Next((2 * level) / 2, 2 * level);
                break;
            default:
                value = 0; // should be impossible to hit here...?
                break;
        }

        return value;
    }

    private StatType GetCorrespondingStat(ItemType itemType)
    {
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
            case ItemType.GLOVES:
                ret = StatType.CRIT_RATE;
                break;
            case ItemType.CHEST:
                ret = StatType.CRIT_DAMAGE;
                break;
            case ItemType.BOOTS:
                ret = StatType.SPEED;
                break;
            default:
                ret = StatType.ATTACK; // should be impossible to hit here...?
                break;
        }

        return ret;
    }
}
