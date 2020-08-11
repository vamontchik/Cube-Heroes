using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

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

    private float sinceLastUpdate = 0f;

    // Unity objects
    public Rigidbody allyRigidBody;
    public Rigidbody enemyRigidBody;

    public Slider allySlider;
    public Slider enemySlider;

    // Start is called before the first frame update
    void Start() 
    {
        this.ally = new Cube(100, 100, 6, 16, 5, 25, 1.5, "ally");
        this.enemy = new Cube(100, 100, 6, 16, 5, 25, 1.5, "enemy");
        this.turn = new Turn(PLAYER_AMOUNT);
        this.currentTurn = ALLY_TURN;
    }

    // Update is called once per frame
    void Update() {
        sinceLastUpdate += Time.deltaTime;
        if (sinceLastUpdate < 1.0) return;
        sinceLastUpdate = 0f;

        AttackResult result = new AttackResult();
        this.currentTurn = turn.NextTurn();

        string toPrint;
        if (this.currentTurn == ALLY_TURN)
        {
            result = ally.Attack(enemy);
            enemySlider.value = 1.0f * enemy.GetHealth() / enemy.GetMaxHealth();

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
            allySlider.value = 1.0f * ally.GetHealth() / ally.GetMaxHealth();

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
