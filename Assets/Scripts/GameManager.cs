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

    // Unity objects
    public Transform allyTransform;
    public Transform enemyTransform;

    public Slider allyHealthSlider;
    public Slider enemyHealthSlider;
    public Slider allySpeedSlider;
    public Slider enemySpeedSlider;

    public TextMeshPro damageNumbersPrefab;

    public Rigidbody allyRigidbody;
    public Rigidbody enemyRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        ally = new Cube
        {
            Health = 200,
            MaxHealth = 200,
            AttackMin = 10,
            AttackMax = 20,
            Defense = 0,
            TurnValue = 0,
            MaxTurnValue = 100,
            Speed = 2,
            CritRate = 25,
            CritDamage = 1.5,
            Name = "ally"
        };


        enemy = new Cube
        {
            Health = 100,
            MaxHealth = 100,
            AttackMin = 5,
            AttackMax = 10,
            Defense = 0,
            TurnValue = 0,
            MaxTurnValue = 100,
            Speed = 1,
            CritRate = 10,
            CritDamage = 1.5,
            Name = "enemy"
        };

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

        uiQueue = new Queue<Action>();

        StartCoroutine(LogicUpdate());
        StartCoroutine(ActionUpdate());
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
            created.color = Color.red;
        }
        listOfDamageNumbers.Add(created);
    }

    private void UpdateSlider(Slider slider, int current, int max)
    {
        slider.value = 1.0f * current / max;
    }

    private void UpdateSlider(Slider slider, float current, int max)
    {
        slider.value = current / max;
    }

    //
    // LINES OF EXECUTION
    //

    IEnumerator DelayBeforeExit()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneIndex.MENU_INDEX);
    }

    private IEnumerator MoveCubes(List<Cube> cubes, List<AttackResult> results)
    {
        if (cubes.Count != results.Count)
            Debug.LogWarning(string.Format("size mismatch - cubes: {0}, results: {1}", cubes.Count, results.Count));

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

    private IEnumerator ActionUpdate()
    {
        while(true)
        {
            int queueLengthCapture = uiQueue.Count;
            for (int i = 0; i < queueLengthCapture; ++i)
            {
                uiQueue.Dequeue().Invoke();
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
    }
}
