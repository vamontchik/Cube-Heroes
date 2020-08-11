using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
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
    //private readonly int ENEMY_TURN = 1;

    private List<TextMeshPro> listOfDamageNumbers;
    private readonly float DELETE_THRESHOLD = 0.01f;

    private float sinceLastUpdate = 0f;

    // Unity objects
    public Transform allyTransform;
    public Transform enemyTransform;

    public Slider allySlider;
    public Slider enemySlider;

    public TextMeshPro damageNumbers;

    // Start is called before the first frame update
    void Start() 
    {
        ally = new Cube(100, 100, 6, 16, 5, 25, 1.5, "ally");
        enemy = new Cube(100, 100, 6, 16, 5, 25, 1.5, "enemy");
        turn = new Turn(PLAYER_AMOUNT);
        currentTurn = ALLY_TURN;

        listOfDamageNumbers = new List<TextMeshPro>();
    }

    // Update is called once per frame
    void Update() {
        List<TextMeshPro> toDelete = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a <= DELETE_THRESHOLD).ToList();
        listOfDamageNumbers = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a > DELETE_THRESHOLD).ToList();
        toDelete.ForEach(x => Destroy(x));
        
        listOfDamageNumbers.ForEach(dmgNum => {
            dmgNum.color = new Color(dmgNum.color.r, dmgNum.color.g, dmgNum.color.b, dmgNum.color.a - (0.5f * Time.deltaTime));
            dmgNum.transform.position += new Vector3(0, 2f) * Time.deltaTime;
        });

        sinceLastUpdate += Time.deltaTime;
        if (sinceLastUpdate < 1.0) return;
        sinceLastUpdate = 0f;

        AttackResult result = new AttackResult();
        currentTurn = turn.NextTurn();

        //string toPrint;
        TextMeshPro created;
        if (currentTurn == ALLY_TURN)
        {
            result = ally.Attack(enemy);
            enemySlider.value = 1.0f * enemy.GetHealth() / enemy.GetMaxHealth();

            created = Instantiate(damageNumbers, enemyTransform.position + 2 * Vector3.up, Quaternion.Euler(0, -90, 0));
            created.SetText(result.damageApplied.ToString());
            listOfDamageNumbers.Add(created);

            //toPrint = String.Format(
            //    "{0} attacks {1} for {2} damage! {0} Health: {3}, {1} Health: {4}",
            //    ally.GetName(),
            //    enemy.GetName(),
            //    result.damageApplied,
            //    ally.GetHealth(),
            //    enemy.GetHealth()
            //);

        }
        else
        {
            result = enemy.Attack(ally);
            allySlider.value = 1.0f * ally.GetHealth() / ally.GetMaxHealth();

            created = Instantiate(damageNumbers, allyTransform.position + 2 * Vector3.up, Quaternion.Euler(0, -90, 0));
            created.SetText(result.damageApplied.ToString());
            listOfDamageNumbers.Add(created);

            //toPrint = String.Format(
            //    "{0} attacks {1} for {2} damage! {1} Health: {3}, {0} Health: {4}",
            //    enemy.GetName(),
            //    ally.GetName(),
            //    result.damageApplied,
            //    ally.GetHealth(),
            //    enemy.GetHealth()
            //);
        }

        if (result.isCrit)
        {
            //toPrint = "Critical Hit! " + toPrint;
            created.color = Color.red;
        }
        //Debug.Log(toPrint);

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
