using System;
using System.Diagnostics;

public class Cube
{
    private int health;
    private int attackMin;
    private int attackMax;
    private int defense;
    // private int speed;
    private int critRate;
    private int critDamage;
    // private int resistance;
    // private int accuracy;

    private Random rand;

    private string name;

    public Cube(string name)
    {
        this.health = 25;
        this.attackMin = 0;
        this.attackMax = 10;
        this.defense = 3;
        // this.speed = 0;
        this.critRate = 25;
        this.critDamage = 0;
        // this.resistance = 0;
        // this.accuracy = 0;

        this.rand = new Random();

        this.name = name;
    }

    private bool IsCrit()
    {
        return rand.Next(/*inclusive*/ 0, /*exclusive*/ 101) < critRate;
    }

    private int CalcAttack()
    {
        return rand.Next(/*inclusive*/ this.attackMin, /*exclusive*/ this.attackMax);
    }

    public int GetHealth()
    {
        return this.health;
    }

    public AttackResult Attack(Cube enemy)
    {
        AttackResult resultObj = new AttackResult();

        resultObj.isCrit = IsCrit();
        int attack = CalcAttack();
        resultObj.damageApplied = attack + (resultObj.isCrit ? attack * this.critDamage : 0);
        resultObj.enemyDefense = enemy.defense;

        if (resultObj.damageApplied > resultObj.enemyDefense)
        {
            enemy.health -= (resultObj.damageApplied - resultObj.enemyDefense);
        }

        resultObj.isEnemyDead = enemy.health <= 0;

        return resultObj;
    }
}