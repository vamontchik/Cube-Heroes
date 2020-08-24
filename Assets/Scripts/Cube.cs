using System;
using System.Collections.Generic;

public class Cube
{
    // private int resistance;
    // private int accuracy;

    // needs to be static, or all cubes have duplicate moves... all get same initial seed?
    private static readonly Random rand = new Random();

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int AttackMin { get; set; }
    public int AttackMax { get; set; }
    public int Defense { get; set; }
    public double TurnValue { get; set; }
    public double MaxTurnValue { get; set; }
    public double Speed { get; set; }
    public int CritRate { get; set; }
    public double CritDamage { get; set; }
    public string Name { get; set; }
    public List<Item> Equipped { get; set; }

    public override string ToString()
    {
        return string.Format("{10} - Health: {0}, " +
                             "MaxHealth: {1}, " +
                             "AttackMin: {2}, " +
                             "AttackMax: {3}, " +
                             "Defense: {4}, " +
                             "TurnValue: {5}, " +
                             "MaxTurnValue: {6}, " +
                             "Speed: {7}, " +
                             "CritRate: {8}, " +
                             "CritDamage: {9}, " +
                             "Equipped: [{11}]",
                             Health, MaxHealth,           // 0 , 1
                             AttackMin, AttackMax,        // 2 , 3
                             Defense,                     // 4
                             TurnValue, MaxTurnValue,     // 5, 6
                             Speed,                       // 7
                             CritRate, CritDamage,        // 8, 9
                             Name,                        // 10
                             string.Join(",", Equipped)); // 11
    }

    public void Apply(Item item)
    {
        switch (item.StatType)
        {
            case StatType.ATTACK:
            case StatType.DEFENSE:
                break;
            case StatType.HEALTH:
                Health += item.StatIncrease;
                MaxHealth += item.StatIncrease;
                break;
        }
    }

    public AttackResult Attack(Cube enemy)
    {
        AttackResult resultObj = new AttackResult();

        resultObj.isCrit = rand.Next(/*inclusive*/ 0, /*exclusive*/ 101) < CritRate;
        
        int baseAttack = rand.Next(/*inclusive*/ AttackMin, /*exclusive*/ AttackMax);
        foreach (Item item in Equipped)
        {
            if (item.StatType == StatType.ATTACK)
            {
                baseAttack += item.StatIncrease;
            }
        }
        int actualAttack = baseAttack + (int)Math.Floor(resultObj.isCrit ? baseAttack * CritDamage : 0);

        int baseEnemyDefense = enemy.Defense;
        foreach (Item item in Equipped)
        {
            if (item.StatType == StatType.DEFENSE)
            {
                baseEnemyDefense += item.StatIncrease;
            }
        }
        resultObj.enemyDefense = baseEnemyDefense;
        
        resultObj.damageApplied = Math.Max(0, actualAttack - enemy.Defense);
        
        enemy.Health -= resultObj.damageApplied;
        
        resultObj.isEnemyDead = enemy.Health <= 0;

        return resultObj;
    }
}