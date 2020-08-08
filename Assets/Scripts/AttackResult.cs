using System;

public class AttackResult
{
    public bool isCrit { get; set; }
    public int damageApplied { get; set; }
    public int enemyDefense { get; set; }
    public bool isEnemyDead { get; set; }

    public override string ToString()
    {
        return String.Format("(isCrit, damageApplied, enemyDefense, isEnemyDead): ({0},{1},{2},{3})", isCrit, damageApplied, enemyDefense, isEnemyDead);
    }

}
