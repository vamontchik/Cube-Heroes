using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

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

    // Unity objects
    public Transform allyTransform;
    public Transform enemyTransform;

    public Slider allyHealthSlider;
    public Slider enemyHealthSlider;
    public Slider allySpeedSlider;
    public Slider enemySpeedSlider;

    public TextMeshPro damageNumbers;

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
        listOfDamageNumbers = listOfDamageNumbers.Where(dmgNum => dmgNum.color.a > DELETE_THRESHOLD).ToList();
        toDelete.ForEach(x => Destroy(x));
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

        TextMeshPro created = Instantiate(damageNumbers, attackedCubeTransform.position + 3 * Vector3.up, Quaternion.Euler(0, -90, 0));
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

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    private bool GameFinished()
    {
        return ally.Health <= 0 || enemy.Health <= 0;
    }

    // Update is called once per frame
    void Update() {
        CleanUpDamageNumbers();
        UpdatePositionsForDamageNumbers();

        if (GameFinished()) return;
        
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
                StartCoroutine(DelayBeforeExit());
            }
        });
    }
}
