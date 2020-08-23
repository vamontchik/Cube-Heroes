using System;
using System.Collections.Generic;

public class Cube
{
    // private int resistance;
    // private int accuracy;

    // needs to be static, or all cubes have duplicate moves... all get same initial seed?
    private static Random rand = new Random();

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int AttackMin { get; set; }
    public int AttackMax { get; set; }
    public int Defense { get; set; }
    public float TurnValue { get; set; }
    public int MaxTurnValue { get; set; }
    public int Speed { get; set; }
    public int CritRate { get; set; }
    public double CritDamage { get; set; }
    public string Name { get; set; }
    public List<Item> Equipped { get; set; }

    public AttackResult Attack(Cube enemy)
    {
        AttackResult resultObj = new AttackResult();

        resultObj.isCrit = rand.Next(/*inclusive*/ 0, /*exclusive*/ 101) < CritRate;
        
        int tempAttack = rand.Next(/*inclusive*/ AttackMin, /*exclusive*/ AttackMax);
        int actualAttack = tempAttack + (int)Math.Floor(resultObj.isCrit ? tempAttack * CritDamage : 0);
        
        resultObj.enemyDefense = enemy.Defense;
        
        resultObj.damageApplied = Math.Max(0, actualAttack - enemy.Defense);
        
        enemy.Health -= resultObj.damageApplied;
        
        resultObj.isEnemyDead = enemy.Health <= 0;

        return resultObj;
    }
}