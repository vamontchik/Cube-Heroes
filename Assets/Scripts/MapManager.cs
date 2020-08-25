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
    public Sprite glovesSprite;
    public Sprite chestSprite;
    public Sprite bootsSprite;
    public TextMeshProUGUI newItemText;
    public Button equipButton;
    public Button delButton;
    public TextMeshProUGUI equipButtonText;
    public TextMeshProUGUI delButtonText;
    public TextMeshProUGUI inventoryWeaponStat;
    public TextMeshProUGUI inventoryHelmetStat;
    public TextMeshProUGUI inventoryShieldStat;
    public TextMeshProUGUI inventoryGlovesStat;
    public TextMeshProUGUI inventoryChestStat;
    public TextMeshProUGUI inventoryBootsStat;
    public TMP_InputField inputField;
    public EventSystem eventSystem;

    // OO objects
    private Queue<Action> uiQueue;
    private Item lastDrop;

    private static readonly int MIN_LEVEL = 1;
    private static readonly int MAX_LEVEL = 1000 + 1;

    public void Start()
    {
        uiQueue = new Queue<Action>();
        (EquippableSet current, bool success) = DataManager.LoadAllyInventory();
        if (!success)
        {
            InventoryTextManager.SetInventoryText(ref uiQueue, inventoryWeaponStat, 0);
            InventoryTextManager.SetInventoryText(ref uiQueue, inventoryHelmetStat, 0);
            InventoryTextManager.SetInventoryText(ref uiQueue, inventoryShieldStat, 0);
            InventoryTextManager.SetInventoryTextGloves(ref uiQueue, inventoryGlovesStat, 0.00);
            InventoryTextManager.SetInventoryTextChest(ref uiQueue, inventoryChestStat, 0);
            InventoryTextManager.SetInventoryTextBoots(ref uiQueue, inventoryBootsStat, 0.0);
        } 
        else
        {
            InventoryTextManager.SetInventoryText(ref uiQueue, inventoryWeaponStat, current.Weapon.StatIncrease);
            InventoryTextManager.SetInventoryText(ref uiQueue, inventoryHelmetStat, current.Helmet.StatIncrease);
            InventoryTextManager.SetInventoryText(ref uiQueue, inventoryShieldStat, current.Shield.StatIncrease);
            InventoryTextManager.SetInventoryTextGloves(ref uiQueue, inventoryGlovesStat, current.Gloves.StatIncrease);
            InventoryTextManager.SetInventoryTextChest(ref uiQueue, inventoryChestStat, current.Chest.StatIncrease);
            InventoryTextManager.SetInventoryTextBoots(ref uiQueue, inventoryBootsStat, current.Boots.StatIncrease);
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

        (int index, bool success3) = DataManager.LoadLevelIndex();
        if (!success2)
        {
            uiQueue.Enqueue(() => inputField.text = "1");
        } 
        else
        {
            uiQueue.Enqueue(() => inputField.text = string.Format("{0}", index));
        }
    }

    private int ParseInputText()
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
            uiQueue.Enqueue(() => inputField.text = string.Format("{0}", MIN_LEVEL)); // TODO : what value makes sense here?
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
            return -1;
        }

        if (val < MIN_LEVEL)
        {
            // can't have negative or 0 level...
            Debug.LogWarning("Negative or zero level value entered into field...");
            uiQueue.Enqueue(() => inputField.text = string.Format("{0}", MIN_LEVEL));
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
            return -1;
        }

        if (val > MAX_LEVEL)
        {
            // past max level
            Debug.LogWarning("max level reached...");
            uiQueue.Enqueue(() => inputField.text = string.Format("{0}", MAX_LEVEL));
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
            return -1;
        }

        return val;
    }

    public void OnClickPlusArea()
    {
        int val = ParseInputText();
        if (val == -1) return;
        if (val == MAX_LEVEL) return; // can't go above max level

        eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
        uiQueue.Enqueue(() => inputField.text = string.Format("{0}", val + 1));
    }

    public void OnClickMinusArea()
    {
        int val = ParseInputText();
        if (val == -1) return;
        if (val == MIN_LEVEL) return; // can't go below min level

        eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
        uiQueue.Enqueue(() => inputField.text = string.Format("{0}", val - 1));
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
        int val = ParseInputText();
        if (val == -1) return;

        bool success = DataManager.SaveLevelIndex(val);
        if (!success)
        {
            Debug.LogError("Could not save level index!");
            uiQueue.Enqueue(() => inputField.text = "");
            eventSystem.SetSelectedGameObject(null); // FIX for buttons acting wierd on selection...
        }
        else
        {
            SceneManager.LoadScene(SceneIndex.FIGHT_INDEX);
        }
    }

    public void OnDelButtonClick()
    {
        if (lastDrop == null)
        {
            return;
        }

        ClearNewItemFromUI();

        bool success = DataManager.DeleteLevelRewardItem();
        if (!success)
        {
            Debug.LogError("Failed to delete level reward item!");
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
                InventoryTextManager.SetInventoryText(ref uiQueue, inventoryWeaponStat, lastDrop.StatIncrease);
                break;
            case ItemType.HELMET:
                current.Helmet = lastDrop;
                InventoryTextManager.SetInventoryText(ref uiQueue, inventoryHelmetStat, lastDrop.StatIncrease);
                break;
            case ItemType.SHIELD:
                current.Shield = lastDrop;
                InventoryTextManager.SetInventoryText(ref uiQueue, inventoryShieldStat, lastDrop.StatIncrease);
                break;
            case ItemType.GLOVES:
                current.Gloves = lastDrop;
                InventoryTextManager.SetInventoryTextGloves(ref uiQueue, inventoryGlovesStat, lastDrop.StatIncrease);
                break;
            case ItemType.CHEST:
                current.Chest = lastDrop;
                InventoryTextManager.SetInventoryTextChest(ref uiQueue, inventoryChestStat, lastDrop.StatIncrease);
                break;
            case ItemType.BOOTS:
                current.Boots = lastDrop;
                InventoryTextManager.SetInventoryTextBoots(ref uiQueue, inventoryBootsStat, lastDrop.StatIncrease);
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
        uiQueue.Enqueue(() => newItemText.enabled = false);
        uiQueue.Enqueue(() => lastDropImage.sprite = null);
        uiQueue.Enqueue(() => lastDropImage.enabled = false);
        uiQueue.Enqueue(() => lastDropStats.enabled = false);
        uiQueue.Enqueue(() => equipButton.image.enabled = false);
        uiQueue.Enqueue(() => equipButtonText.enabled = false);
        uiQueue.Enqueue(() => delButton.image.enabled = false);
        uiQueue.Enqueue(() => delButtonText.enabled = false);
    }

    private void AddNewItemToUI(Item drop)
    {
        lastDrop = drop;
        uiQueue.Enqueue(() => newItemText.SetText("New Item:"));
        uiQueue.Enqueue(() => newItemText.enabled = true);

        switch (drop.ItemType)
        {
            case ItemType.WEAPON:
                uiQueue.Enqueue(() => lastDropImage.sprite = weaponSprite);
                break;
            case ItemType.HELMET:
                uiQueue.Enqueue(() => lastDropImage.sprite = helmetSprite);
                break;
            case ItemType.SHIELD:
                uiQueue.Enqueue(() => lastDropImage.sprite = shieldSprite);
                break;
            case ItemType.GLOVES:
                uiQueue.Enqueue(() => lastDropImage.sprite = glovesSprite);
                break;            
            case ItemType.CHEST:
                uiQueue.Enqueue(() => lastDropImage.sprite = chestSprite);
                break;            
            case ItemType.BOOTS:
                uiQueue.Enqueue(() => lastDropImage.sprite = bootsSprite);
                break;
        }
        uiQueue.Enqueue(() => lastDropImage.enabled = true);

        uiQueue.Enqueue(() => lastDropStats.enabled = true);
        if (drop.ItemType == ItemType.GLOVES)
        {
            InventoryTextManager.SetInventoryTextGloves(ref uiQueue, lastDropStats, drop.StatIncrease);
        }
        else if (drop.ItemType == ItemType.CHEST)
        {
            InventoryTextManager.SetInventoryTextChest(ref uiQueue, lastDropStats, drop.StatIncrease);
        }
        else if (drop.ItemType == ItemType.BOOTS)
        {
            InventoryTextManager.SetInventoryTextBoots(ref uiQueue, lastDropStats, drop.StatIncrease);
        }
        else
        {
            InventoryTextManager.SetInventoryText(ref uiQueue, lastDropStats, drop.StatIncrease);
        }        

        uiQueue.Enqueue(() => equipButton.image.enabled = true);
        uiQueue.Enqueue(() => equipButtonText.enabled = true);

        uiQueue.Enqueue(() => delButton.image.enabled = true);
        uiQueue.Enqueue(() => delButtonText.enabled = true);
    }
}
