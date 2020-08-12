using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // OO objects
    private Cube ally;
    private List<Cube> allies;
    private Cube enemy;
    private List<Cube> enemies;
    private List<Cube> allCubes;

    private List<TextMeshPro> listOfDamageNumbers;
    private readonly float DELETE_THRESHOLD = 0.01f;

    private Rigidbody attackingCube = null;
    private Rigidbody defendingCube = null;
    private bool towards = true;
    private bool objectsMoving = false;
    private bool stopUpdates = false;

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
            Health = 100,
            MaxHealth = 100,
            AttackMin = 6,
            AttackMax = 16,
            Defense = 5,
            TurnValue = 0,
            MaxTurnValue = 100,
            Speed = 100,
            CritRate = 25,
            CritDamage = 1.5,
            Name = "ally"
        };


        enemy = new Cube
        {
            Health = 200,
            MaxHealth = 200,
            AttackMin = 25,
            AttackMax = 50,
            Defense = 5,
            TurnValue = 0,
            MaxTurnValue = 100,
            Speed = 10,
            CritRate = 25,
            CritDamage = 1.5,
            Name = "enemy"
        };

        allies = new List<Cube>
        {
            ally
        };

        enemies = new List<Cube>
        {
            enemy
        };

        allCubes = new List<Cube> 
        { 
            ally, enemy 
        };

        listOfDamageNumbers = new List<TextMeshPro>();
    }

    private Slider GetCorrespondingSpeedSlider(Cube cube)
    {
        if (cube == ally)
        {
            return allySpeedSlider;
        }
        else
        {
            return enemySpeedSlider;
        }
    }

    private List<Cube> UpdateSpeeds()
    {
        List<Cube> cubesToMove = new List<Cube>();

        allCubes.ForEach(cube => {
            cube.TurnValue += cube.Speed * Time.deltaTime;
            if (cube.TurnValue >= cube.MaxTurnValue)
            {
                cube.TurnValue -= cube.MaxTurnValue;
                cubesToMove.Add(cube);
            }
            Slider speedSlider = GetCorrespondingSpeedSlider(cube);
            UpdateSlider(speedSlider, cube.TurnValue, cube.MaxTurnValue);
        });

        return cubesToMove;
        
    }

    private void CleanUpDamageNumbers()
    {
        List<TextMeshPro> toDelete = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a <= DELETE_THRESHOLD).ToList();
        List<TextMeshPro> newList = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a > DELETE_THRESHOLD).ToList();
        toDelete.ForEach(x => Destroy(x.gameObject));
        listOfDamageNumbers = newList;
    }

    private void UpdatePositionsForDamageNumbers()
    {
        listOfDamageNumbers.ForEach(dmgNum => {
            dmgNum.color = new Color(dmgNum.color.r, dmgNum.color.g, dmgNum.color.b, dmgNum.color.a - (0.5f * Time.deltaTime));
            dmgNum.transform.position += new Vector3(0, 2f) * Time.deltaTime;
        });
    }
    private Cube PickRandomEnemy(Cube cube)
    {
        if (cube == ally)
        {
            return enemy;
        } 
        else
        {
            return ally;
        }
    }

    private void ModifyWithCritAsNecessary(TextMeshPro created, AttackResult result)
    {
        if (result.isCrit)
        {
            created.color = Color.red;
        }
    }

    private TextMeshPro CreateDamagePopup(Cube attackedCube, AttackResult result)
    {
        Transform attackedCubeTransform;
        if (attackedCube == ally)
        {
            attackedCubeTransform = allyTransform;
        }
        else
        {
            attackedCubeTransform = enemyTransform;
        }

        TextMeshPro created = Instantiate(damageNumbersPrefab, attackedCubeTransform.position + 3 * Vector3.up, Quaternion.Euler(0, -90, 0));
        created.SetText(result.damageApplied.ToString());
        listOfDamageNumbers.Add(created);

        return created;
    }

    private void UpdateSlider(Slider slider, int current, int max)
    {
        slider.value = 1.0f * current / max;
    }

    private void UpdateSlider(Slider slider, float current, int max)
    {
        slider.value = current / max;
    }


    private Slider GetCorrespondingHealthSlider(Cube attackedCube)
    {
        if (attackedCube == ally)
        {
            return allyHealthSlider;
        }
        else
        {
            return enemyHealthSlider;
        }
    }

    IEnumerator DelayBeforeExit()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
    }

    private void MoveCubes(List<Cube> cubes)
    {
        List<MovingAction> actions = new List<MovingAction>();
        cubes.ForEach(cube =>
        {
            MovingAction newAction;
            if (cube == ally)
            {
                newAction = new MovingAction()
                {
                    moving = allyRigidbody,
                    target = enemyRigidbody
                };
            }
            else
            {
                newAction = new MovingAction()
                {
                    moving = enemyRigidbody,
                    target = allyRigidbody
                };
            }
            actions.Add(newAction);
        });

        StartCoroutine(MoveAll(actions));
    }

    private IEnumerator MoveAll(List<MovingAction> actions)
    {
        objectsMoving = true;

        foreach (MovingAction movingAction in actions)
        {
            attackingCube = movingAction.moving;
            defendingCube = movingAction.target;

            towards = true;
            yield return new WaitUntil(() => Vector3.Magnitude(movingAction.target.position - movingAction.moving.position) < 1.5f);
            towards = false;
            yield return new WaitUntil(() => Vector3.Magnitude(movingAction.moving.position - movingAction.target.position) > 6f);

            attackingCube = null;
            defendingCube = null;
        }

        objectsMoving = false;
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
        attackingCube.MovePosition(attackingCube.transform.position + direction.normalized * 20f * Time.deltaTime);
    }


    // Update is called once per frame
    private void Update() 
    {
        if (stopUpdates || objectsMoving) return;

        CleanUpDamageNumbers();
        UpdatePositionsForDamageNumbers();
        
        List<Cube> cubesToMove = UpdateSpeeds();
        cubesToMove.ForEach(cube =>
        {
            Cube randEnemy = PickRandomEnemy(cube);
            AttackResult result = cube.Attack(randEnemy);
            Slider enemySlider = GetCorrespondingHealthSlider(randEnemy);
            UpdateSlider(enemySlider, randEnemy.Health, randEnemy.MaxHealth);
            TextMeshPro created = CreateDamagePopup(randEnemy, result);
            ModifyWithCritAsNecessary(created, result);
            
            if (result.isEnemyDead)
            {
                stopUpdates = true;
                StartCoroutine(DelayBeforeExit());
            }
        });

        // group up all moveable cubes to ensure a single call to coroutine
        MoveCubes(cubesToMove);
    }
}
