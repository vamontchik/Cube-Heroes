using System;

public class Cube
{
    private int health;
    private int maxHealth;
    private int attackMin;
    private int attackMax;
    private int defense;
    // private int speed;
    private int critRate;
    private double critDamage;
    // private int resistance;
    // private int accuracy;

    // needs to be static, or all cubes
    // have duplicate moves... all get same initial seed?
    private static Random rand = new Random();

    private string name;

    // TODO: use builder pattern here...
    public Cube(int health, int maxHealth, int attackMin, int attackMax, int defense, int critRate, double critDamage, string name)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.attackMin = attackMin;
        this.attackMax = attackMax;
        this.defense = defense;
        // this.speed = 0;
        this.critRate = critRate;
        this.critDamage = critDamage;
        // this.resistance = 0;
        // this.accuracy = 0;

        this.name = name;
    }

    public int GetHealth()
    {
        return this.health;
    }

    public int GetMaxHealth()
    {
        return this.maxHealth;
    }

    //public string GetName()
    //{
    //    return this.name;
    //}

    public AttackResult Attack(Cube enemy)
    {
        AttackResult resultObj = new AttackResult();

        resultObj.isCrit = rand.Next(/*inclusive*/ 0, /*exclusive*/ 101) < critRate;
        
        int tempAttack = rand.Next(/*inclusive*/ this.attackMin, /*exclusive*/ this.attackMax);
        int actualAttack = tempAttack + (int)Math.Floor(resultObj.isCrit ? tempAttack * this.critDamage : 0);
        
        resultObj.enemyDefense = enemy.defense;
        
        resultObj.damageApplied = Math.Max(0, actualAttack - enemy.defense);
        
        enemy.health -= resultObj.damageApplied;
        
        resultObj.isEnemyDead = enemy.health <= 0;

        return resultObj;
    }
}