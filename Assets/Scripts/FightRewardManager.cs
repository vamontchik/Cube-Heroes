using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DataSaver;

public class FightRewardManager : MonoBehaviour
{
    // OO objects
    private Queue<Action> uiQueue;

    // Unity objects
    public TextMeshProUGUI newItemStr;

    public void Start()
    {
        uiQueue = new Queue<Action>();

        // create random item
        Item drop = new Item()
        {
            Name = "test item",
            ItemType = ItemType.WEAPON,
            StatType = StatType.ATTACK,
            StatIncrease = new System.Random().Next(/*inclusive*/ 5, /*exclusive*/ 10)
        };

        // save item to player inventory file
        SaveItemData(drop);

        // enqueue ui update for new item text
        string old = newItemStr.text;
        uiQueue.Enqueue(() => newItemStr.SetText(string.Format("{0} {1}", old, drop.Name)));
    }

    public void Update()
    {
        int amount = uiQueue.Count;
        for (int i = 0; i < amount; ++i)
        {
            uiQueue.Dequeue().Invoke();
        }
    }

    public void LoadMap()
    {
        SceneManager.LoadScene(SceneIndex.MAP_INDEX);
    }
}
