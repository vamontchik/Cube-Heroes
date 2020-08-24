using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // OO objects
    private Cube ally;
    private Cube enemy;
    private List<Cube> allCubes;
    private Dictionary<Cube, List<Slider>> cubeToSliders;
    private readonly int HEALTH_SLIDER = 0;
    private readonly int SPEED_SLIDER = 1;
    private Queue<Action> uiQueue;

    private List<TextMeshPro> listOfDamageNumbers;
    private readonly float DELETE_THRESHOLD = 0.01f;

    private Rigidbody attackingCube = null;
    private Rigidbody defendingCube = null;
    
    private bool towards = true;
    private bool objectsMoving = false;

    private readonly float _60_HZ = 0.01667f;

    private int level;

    // Unity objects
    public Transform allyTransform;
    public Transform enemyTransform;

    public Slider allyHealthSlider;
    public Slider enemyHealthSlider;
    public Slider allySpeedSlider;
    public Slider enemySpeedSlider;

    public TextMeshPro damageNumbersPrefab;

    public TextMeshProUGUI weaponDamageText;
    public TextMeshProUGUI helmetHPText;
    public TextMeshProUGUI shieldDefText;
    public TextMeshProUGUI glovesText;
    public TextMeshProUGUI chestText;
    public TextMeshProUGUI bootsText;

    public Rigidbody allyRigidbody;
    public Rigidbody enemyRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        (int temp, bool success1) = DataManager.LoadLevelIndex();
        if (!success1)
        {
            // fallback on level one and log an error
            Debug.LogError("Unable to load level index!");
            level = 1;
        }
        else
        {
            Debug.Log("Loaded level index!");
            level = temp;
        }

        Debug.Log(string.Format("level: {0}", level));

        uiQueue = new Queue<Action>();

        // BASE ALLY STATS
        ally = new Cube
        {
            Health = 25,
            MaxHealth = 25,
            AttackMin = 5,
            AttackMax = 10,
            Defense = 1,
            TurnValue = 0.0,
            MaxTurnValue = 100.0,
            Speed = 2.0,              // only difference from enemy base @ level 1
            CritRate = 1,
            CritDamage = 1.5,
            Name = "ally",
            Equipped = new List<Item>()
        };

        (EquippableSet items, bool success) = DataManager.LoadAllyInventory();
        if (!success)
        {
            Debug.LogWarning("Could not load ally inventory!");

            uiQueue.Enqueue(() => weaponDamageText.SetText("+0"));
            uiQueue.Enqueue(() => helmetHPText.SetText("+0"));
            uiQueue.Enqueue(() => shieldDefText.SetText("+0"));
            uiQueue.Enqueue(() => glovesText.SetText("+0"));
            uiQueue.Enqueue(() => chestText.SetText("+0.00"));
            uiQueue.Enqueue(() => bootsText.SetText("+0.000"));
        }
        else
        {
            Debug.Log("Loaded ally inventory!");

            ally.Equipped.Add(items.Weapon);
            
            ally.Equipped.Add(items.Helmet);
            ally.Apply(items.Helmet);

            ally.Equipped.Add(items.Shield);
            ally.Apply(items.Shield);

            ally.Equipped.Add(items.Gloves);
            ally.Apply(items.Gloves);

            ally.Equipped.Add(items.Chest);
            ally.Apply(items.Chest);

            ally.Equipped.Add(items.Boots);
            ally.Apply(items.Boots);

            uiQueue.Enqueue(() => weaponDamageText.SetText(string.Format("+{0}", items.Weapon.StatIncrease)));
            uiQueue.Enqueue(() => helmetHPText.SetText(string.Format("+{0}", items.Helmet.StatIncrease)));
            uiQueue.Enqueue(() => shieldDefText.SetText(string.Format("+{0}", items.Shield.StatIncrease)));            
            uiQueue.Enqueue(() => glovesText.SetText(string.Format("+{0}", items.Gloves.StatIncrease)));
            uiQueue.Enqueue(() => chestText.SetText(string.Format("+{0:F3}", items.Chest.StatIncrease / 1000.0)));
            uiQueue.Enqueue(() => bootsText.SetText(string.Format("+{0:F1}", items.Boots.StatIncrease / 10.0)));
        }

        // BASE ENEMY STATS
        enemy = new Cube
        {
            Health = 25 * level,                 // every level, health increased by 25,      start at 25 hp
            MaxHealth = 25 * level,              // every level, maxHealth increased by 25,   start at 25 hp
            AttackMin = 5 + ((level - 1) * 5),   // every level, attackMin increased by 5,    start at 5  min
            AttackMax = 10 + ((level - 1) * 5),  // every level, attackMax increased by 5,    start at 10 max
            Defense = 1 + 1 * (level - 1),       // every level, defense increased by 1,      start at 1 defense
            TurnValue = 0.0,
            MaxTurnValue = 100.0,
            Speed = 1.0 + ((level - 1) / 10.0),                // every level, increase speed by 0.1,                 start at 1 speed
            CritRate = 1 + (int)Math.Floor((level - 1) / 4.0), // every 4 levels, increase crit rate by 1%            start at 1% crit rate
            CritDamage = 1.5 + ((level - 1) / 1000.0),         // +150% , every level increased crit damage by 0.01%  start at x1.5
            Name = "enemy",
            Equipped = new List<Item>()
        };

        Debug.Log(ally);
        Debug.Log(enemy);

        allCubes = new List<Cube> 
        { 
            ally, enemy 
        };

        listOfDamageNumbers = new List<TextMeshPro>();

        cubeToSliders = new Dictionary<Cube, List<Slider>>
        {
            { ally, new List<Slider>() { allyHealthSlider, allySpeedSlider } },
            { enemy, new List<Slider>() { enemyHealthSlider, enemySpeedSlider } }
        };

        StartCoroutine(LogicUpdate());
    }

    private List<Cube> UpdateSpeeds()
    {
        List<Cube> cubesToMove = new List<Cube>();

        allCubes.ForEach(cube => {
            cube.TurnValue += cube.Speed;
            if (cube.TurnValue >= cube.MaxTurnValue)
            {
                cube.TurnValue -= cube.MaxTurnValue;
                cubesToMove.Add(cube);
            }
            Slider speedSlider = cubeToSliders[cube][SPEED_SLIDER];
            uiQueue.Enqueue(() => UpdateSlider(speedSlider, cube.TurnValue, cube.MaxTurnValue));
        });

        return cubesToMove;
    }

    private void CleanUpDamageNumbers()
    {
        List<TextMeshPro> toDelete = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a <= DELETE_THRESHOLD).ToList();
        listOfDamageNumbers.RemoveAll(dmgNum => dmgNum.color.a <= DELETE_THRESHOLD);
        toDelete.ForEach(x => Destroy(x.gameObject));
    }

    private void UpdateAlphasAndPositionsForDamageNumbers()
    {
        // assumed: this method is only called inside of the Update() loop, hence the use of 'Time.deltaTime'
        listOfDamageNumbers.ForEach(dmgNum => {
            dmgNum.color = new Color(dmgNum.color.r, dmgNum.color.g, dmgNum.color.b, dmgNum.color.a - (0.5f * Time.deltaTime));
            dmgNum.transform.position += new Vector3(0, 2f) * Time.deltaTime;
        });
    }
    private Cube GetEnemy(Cube cube)
    {
        return cube == ally ? enemy : ally;
    }

    private void CreateDamagePopup(Cube attackedCube, AttackResult result)
    {
        Transform attackedCubeTransform = attackedCube == ally ? allyTransform : enemyTransform;
        TextMeshPro created = Instantiate(damageNumbersPrefab, attackedCubeTransform.position + 3 * Vector3.up, Quaternion.Euler(0, -90, 0));
        created.SetText(result.damageApplied.ToString());
        if (result.isCrit)
        {
            created.faceColor = Color.red;
        }
        listOfDamageNumbers.Add(created);
    }

    private void UpdateSlider(Slider slider, double current, double max)
    {
        slider.value = (float) (current / max);
    }

    //
    // LINES OF EXECUTION
    //

    IEnumerator DelayBeforeExit()
    {
        yield return new WaitForSeconds(2f);

        if (enemy.Health <= 0)
        {
            // on win, go to scene w/ reward

            Debug.Log("Win! Going to reward scene...");
            SceneManager.LoadScene(SceneIndex.FIGHT_REWARD_INDEX);
        } 
        else
        {
            // on loss, go back to map w/o reward

            Debug.Log("Loss! Returning to map...");
            SceneManager.LoadScene(SceneIndex.MAP_INDEX);
        }


    }

    private IEnumerator MoveCubes(List<Cube> cubes, List<AttackResult> results)
    {
        if (cubes.Count != results.Count)
            Debug.LogError(string.Format("size mismatch - cubes: {0}, results: {1}", cubes.Count, results.Count));

        // create moving actions
        List<MovingAction> actions = new List<MovingAction>();
        int count = 0;
        foreach (Cube cube in cubes)
        {
            MovingAction newAction;
            if (cube == ally)
            {
                newAction = new MovingAction()
                {
                    movingCube = cube,
                    movingRigidbody = allyRigidbody,
                    targetCube = enemy,
                    targetRigidbody = enemyRigidbody,
                    result = results[count]
                };
            }
            else
            {
                newAction = new MovingAction()
                {
                    movingCube = enemy,
                    movingRigidbody = enemyRigidbody,
                    targetCube = ally,
                    targetRigidbody = allyRigidbody,
                    result = results[count]
                };
            }
            actions.Add(newAction);
            ++count;
        }

        // execute moving actions
        objectsMoving = true;
        foreach (MovingAction movingAction in actions)
        {
            attackingCube = movingAction.movingRigidbody;
            defendingCube = movingAction.targetRigidbody;
            towards = true;
            yield return new WaitUntil(() => Vector3.Magnitude(movingAction.targetRigidbody.position - movingAction.movingRigidbody.position) < 1.5f);
            
            uiQueue.Enqueue(() => UpdateSlider(cubeToSliders[movingAction.targetCube][HEALTH_SLIDER], movingAction.targetCube.Health, movingAction.targetCube.MaxHealth));
            uiQueue.Enqueue(() => CreateDamagePopup(movingAction.targetCube, movingAction.result));
            towards = false;
            yield return new WaitUntil(() => Vector3.Magnitude(movingAction.movingRigidbody.position - movingAction.targetRigidbody.position) > 6f);

            attackingCube = null;
            defendingCube = null;
        }
        objectsMoving = false;
    }

    private IEnumerator LogicUpdate()
    {
        while(true)
        {
            if (objectsMoving) yield break; // effectively "return"

            List<Cube> cubesToMove = UpdateSpeeds();
            List<AttackResult> results = new List<AttackResult>();
            bool fightOver = false;
            foreach (Cube cube in cubesToMove)
            {
                Cube enemy = GetEnemy(cube);
                AttackResult result = cube.Attack(enemy);
                results.Add(result);
                if (result.isEnemyDead)
                {
                    fightOver = true;
                    break; // if an attack kills the other, prevent additional moves
                }
            }

            // if there's a mismatch between results.Count and cubesToMove.Count,
            // then trim off cubes from cubesToMove to match results size.
            // note: this only happens at the end of the game, 
            //       when the 'result.isEnemyDead' body executes
            if (results.Count != cubesToMove.Count)
            {
                cubesToMove.RemoveRange(results.Count, cubesToMove.Count - results.Count);
            }

            yield return StartCoroutine(MoveCubes(cubesToMove, results)); // move cubes in one go before resuming execution

            if (fightOver)
            {
                yield return StartCoroutine(DelayBeforeExit());
                yield break; // effectively "return" , instead of waiting another 1/60 seconds end logic updates
            }

            yield return new WaitForSeconds(_60_HZ);
        }
    }

    private void FixedUpdate()
    {
        if (!objectsMoving) return;

        Vector3 direction;
        if (towards)
        {
            direction = (attackingCube != null && defendingCube != null) ? defendingCube.position - attackingCube.position : Vector3.zero;
        }
        else
        {
            direction = (attackingCube != null && defendingCube != null) ? attackingCube.position - defendingCube.position : Vector3.zero;
        }

        if (attackingCube != null)
        {
            attackingCube.MovePosition(attackingCube.transform.position + direction.normalized * 20f * Time.deltaTime);
        }
    }

    // Update is called once per frame
    private void Update() 
    {
        CleanUpDamageNumbers();
        UpdateAlphasAndPositionsForDamageNumbers();
        
        // clear through current ui actions in queue
        int queueLengthCapture = uiQueue.Count;
        for (int i = 0; i < queueLengthCapture; ++i)
        {
            uiQueue.Dequeue().Invoke();
        }
    }
}
