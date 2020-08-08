using UnityEngine;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // OO objects
    private Cube ally;
    private Cube enemy;
    private readonly int PLAYER_AMOUNT = 2;

    private Turn turn;
    private int currentTurn;
    private readonly int ALLY_TURN = 0;
    private readonly int ENEMY_TURN = 1;

    // Unity objects
    public Rigidbody allyRigidBody;
    public Rigidbody enemyRigidBody;

    // Start is called before the first frame update
    void Start() 
    {
        this.ally = new Cube(100, 0, 10, 2, 25, 1.5, "ally");
        this.enemy = new Cube(100, 0, 10, 2, 25, 1.5, "enemy");
        this.turn = new Turn(PLAYER_AMOUNT);
        this.currentTurn = ALLY_TURN;
    }

    // Update is called once per frame
    void Update() {
        AttackResult result = new AttackResult();
        this.currentTurn = turn.NextTurn();

        string toPrint;
        if (this.currentTurn == ALLY_TURN)
        {
            result = ally.Attack(enemy);

            toPrint = String.Format(
                "{0} attacks {1} for {2} damage! {0} Health: {3}, {1} Health: {4}",
                ally.GetName(),
                enemy.GetName(),
                result.damageApplied,
                ally.GetHealth(),
                enemy.GetHealth()
            );

        }
        else
        {
            result = enemy.Attack(ally);

            toPrint = String.Format(
                "{0} attacks {1} for {2} damage! {1} Health: {3}, {0} Health: {4}",
                enemy.GetName(),
                ally.GetName(),
                result.damageApplied,
                ally.GetHealth(),
                enemy.GetHealth()
            );
        }

        if (result.isCrit)
        {
            toPrint = "Critical Hit! " + toPrint;
        }
        Debug.Log(toPrint);

        if (result.isEnemyDead)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
