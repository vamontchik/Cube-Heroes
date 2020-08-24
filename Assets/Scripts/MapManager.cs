using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    // Unity objects
    public TextMeshProUGUI lastDropStats;
    public Image lastDropImage;
    public Sprite weaponSprite;
    public Sprite helmetSprite;
    public Sprite shieldSprite;
    public TextMeshProUGUI newItemText;
    public Button equipButton;
    public TextMeshProUGUI equipButtonText;
    public TextMeshProUGUI inventoryWeaponStat;
    public TextMeshProUGUI inventoryHelmetStat;
    public TextMeshProUGUI inventoryShieldStat;
    public TMP_InputField inputField;
    public EventSystem eventSystem;

    // OO objects
    private Queue<Action> uiQueue;
    private Item lastDrop;

    public void Start()
    {
        uiQueue = new Queue<Action>();
        (EquippableSet current, bool success1) = DataManager.LoadAllyInventory();
        if (!success1)
        {
            SetInventoryText(inventoryWeaponStat, 0);
            SetInventoryText(inventoryHelmetStat, 0);
            SetInventoryText(inventoryShieldStat, 0);
        } 
        else
        {
            // lambda capture of actual stat value, not reference to stat value, b/c copy <=== applies anymore?
            SetInventoryText(inventoryWeaponStat, current.Weapon.StatIncrease);
            SetInventoryText(inventoryHelmetStat, current.Helmet.StatIncrease);
            SetInventoryText(inventoryShieldStat, current.Shield.StatIncrease);
        }

        (Item drop, bool success2) = DataManager.LoadLevelRewardItem();
        if (!success2)
        {
            Debug.LogWarning("Could not load level reward item!");
            ClearNewItemFromUI();
        }
        else
        {
            Debug.Log(string.Format("Loaded level reward item: {0}", drop));
            AddNewItemToUI(drop);
        }
    }

    public void Update()
    {
        int length = uiQueue.Count;
        for (int i = 0; i < length; ++i)
        {
            uiQueue.Dequeue().Invoke();
        }
    }

    public void LoadArea()
    {
        int val;
        try
        {
            val = int.Parse(inputField.text);
        } 
        catch (FormatException)
        {
            // empty field?
            Debug.LogWarning("Invalid value entered into field...");
            inputField.text = "";
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
            return;
        }
        
        if (val <= 0)
        {
            // can't have negative or 0 level...
            Debug.LogWarning("Negative or zero level value entered into field...");
            inputField.text = "";
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
            return;
        }

        bool success = DataManager.SaveLevelIndex(val);
        if (!success)
        {
            Debug.LogError("Could not save level index!");
            inputField.text = "";
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
            return;
        }
        else
        {
            SceneManager.LoadScene(SceneIndex.FIGHT_INDEX);
        }
    }

    public void OnEquipButtonClick()
    {
        if (lastDrop == null)
        {
            return;
        }

        (EquippableSet current, bool success) = DataManager.LoadAllyInventory();

        if (!success)
        {
            Debug.LogWarning("Failed to read equippable set data!");
            current = new EquippableSet();
        }

        switch (lastDrop.ItemType)
        {
            case ItemType.WEAPON:
                current.Weapon = lastDrop;
                SetInventoryText(inventoryWeaponStat, lastDrop.StatIncrease);
                break;
            case ItemType.HELMET:
                current.Helmet = lastDrop;
                SetInventoryText(inventoryHelmetStat, lastDrop.StatIncrease);
                break;
            case ItemType.SHIELD:
                current.Shield = lastDrop;
                SetInventoryText(inventoryShieldStat, lastDrop.StatIncrease);
                break;
            case ItemType.GLOVES:
                current.Gloves = lastDrop;
                break;
            case ItemType.CHEST:
                current.Chest = lastDrop;
                break;
            case ItemType.BOOTS:
                current.Boots = lastDrop;
                break;
        }

        success = DataManager.SaveAllyInventory(current);
        if (!success)
        {
            Debug.LogError("Failed to save equipabble set data!");
            return; // prevent bad save from deleting item drop
        }

        ClearNewItemFromUI();

        success = DataManager.DeleteLevelRewardItem();
        if (!success)
        {
            Debug.LogError("Failed to delete level reward item!");
        }
    }

    private void ClearNewItemFromUI()
    {
        lastDrop = null;
        uiQueue.Enqueue(() => newItemText.SetText(""));
        uiQueue.Enqueue(() => lastDropImage.sprite = null);
        uiQueue.Enqueue(() => lastDropImage.enabled = false);
        uiQueue.Enqueue(() => lastDropStats.SetText(""));
        uiQueue.Enqueue(() => equipButton.image.enabled = false);
        uiQueue.Enqueue(() => equipButtonText.enabled = false);
    }

    private void AddNewItemToUI(Item drop)
    {
        lastDrop = drop;
        uiQueue.Enqueue(() => newItemText.SetText("New Item:"));   
        switch (drop.ItemType)
        {
            // TODO: impl for the rest of item types when they're done...
            case ItemType.WEAPON:
                uiQueue.Enqueue(() => lastDropImage.sprite = weaponSprite);
                break;
            case ItemType.HELMET:
                uiQueue.Enqueue(() => lastDropImage.sprite = helmetSprite);
                break;
            case ItemType.SHIELD:
                uiQueue.Enqueue(() => lastDropImage.sprite = shieldSprite);
                break;
            default:
                uiQueue.Enqueue(() => lastDropImage.sprite = null);
                break;
        }
        uiQueue.Enqueue(() => lastDropImage.enabled = true);
        uiQueue.Enqueue(() => lastDropStats.SetText(string.Format("+{0}", lastDrop.StatIncrease)));
        uiQueue.Enqueue(() => equipButton.image.enabled = true);
        uiQueue.Enqueue(() => equipButtonText.enabled = true);
    }

    private void SetInventoryText(TextMeshProUGUI inventoryItemStat, int value)
    {
        uiQueue.Enqueue(() => inventoryItemStat.SetText(string.Format("+{0}", value)));
    }
}
