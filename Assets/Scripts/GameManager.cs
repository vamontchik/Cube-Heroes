using UnityEngine;
using System;

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
        this.ally = new Cube("ally");
        this.enemy = new Cube("enemy");
        this.turn = new Turn(PLAYER_AMOUNT);
        this.currentTurn = ALLY_TURN;
    }

    // Update is called once per frame
    void Update() {
        AttackResult result = new AttackResult();
        this.currentTurn = turn.NextTurn();

        if (this.currentTurn == ALLY_TURN)
        {
            result = ally.Attack(enemy);
        }
        else
        {
            result = enemy.Attack(ally);
        }

        Debug.Log(String.Format("{0}, Ally Health: {1}, Enemy Health: {2}, Turn: {3}", result, ally.GetHealth(), enemy.GetHealth(), this.currentTurn));

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
