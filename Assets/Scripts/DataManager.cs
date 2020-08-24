// NOTE:
// modified from https://stackoverflow.com/a/40966346 for an easy-to-use serialization class !!!

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DataManager
{
    private static readonly string ALLY_INVENTORY_DATA_PATH = "/ally_inventory_data.json";
    private static readonly string LEVEL_REWARD_ITEM_PATH = "/level_reward_item.json";
    private static readonly string LEVEL_INDEX_PATH = "/level_index.json";

    public static (EquippableSet, bool) LoadAllyInventory()
    {
        string fullPath = string.Format("{0}{1}", Application.persistentDataPath, ALLY_INVENTORY_DATA_PATH);
        return LoadData<EquippableSet>(fullPath);
    }
    public static bool SaveAllyInventory(EquippableSet set)
    {
        string fullPath = string.Format("{0}{1}", Application.persistentDataPath, ALLY_INVENTORY_DATA_PATH);
        return SaveData(set, fullPath);
    }

    public static (Item, bool) LoadLevelRewardItem()
    {
        string pathToItemReward = string.Format("{0}{1}", Application.persistentDataPath, LEVEL_REWARD_ITEM_PATH);
        return LoadData<Item>(pathToItemReward);
    }

    public static bool SaveLevelRewardItem(Item item)
    {
        string pathToItemReward = string.Format("{0}{1}", Application.persistentDataPath, LEVEL_REWARD_ITEM_PATH);
        return SaveData(item, pathToItemReward);
    }

    public static bool DeleteLevelRewardItem()
    {
        string pathToItemReward = string.Format("{0}{1}", Application.persistentDataPath, LEVEL_REWARD_ITEM_PATH);
        return DeleteData<Item>(pathToItemReward);
    }

    public static (int, bool) LoadLevelIndex()
    {
        string pathToLevelIndex = string.Format("{0}{1}", Application.persistentDataPath, LEVEL_INDEX_PATH);
        (LevelIndex wrapped, bool success) = LoadData<LevelIndex>(pathToLevelIndex);
        return (success ? wrapped.index : -1, success);
    }

    public static bool SaveLevelIndex(int index)
    {
        string pathToLevelIndex = string.Format("{0}{1}", Application.persistentDataPath, LEVEL_INDEX_PATH);
        LevelIndex wrapper = new LevelIndex() { index = index };
        return SaveData(wrapper, pathToLevelIndex);
    }


    //
    // BASE
    //

    private static bool SaveData<T>(T data, string fullPath)
    {
        byte[] itemAsJson = Encoding.ASCII.GetBytes(JsonUtility.ToJson(data)); // TODO: allows for utf8 encoding?

        try
        {
            File.WriteAllBytes(fullPath, itemAsJson);
            Debug.Log(string.Format("{0}{1}", "Saved Data to: ", fullPath.Replace("/", "\\")));
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("{0}{1}", "Failed To PlayerInfo Data to: ", fullPath.Replace("/", "\\")));
            Debug.LogError(string.Format("{0}{1}", "Error: ", e.Message));
            return false;
        }

        return true;
    }

    private static (T, bool) LoadData<T>(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning(string.Format("{0}{1}", "File does not exist: ", fullPath));
            return (default, false);
        }

        byte[] jsonBytes;
        try
        {
            jsonBytes = File.ReadAllBytes(fullPath);
            Debug.Log(string.Format("{0}{1}", "Loaded Data from: ", fullPath.Replace("/", "\\")));
        }
        catch (Exception e)
        {
            Debug.LogWarning(string.Format("{0}{1}", "Failed To Load Data from: ", fullPath.Replace("/", "\\")));
            Debug.LogWarning(string.Format("{0}{1}", "Error: ", e.Message));
            return (default, false);
        }

        T ret = JsonUtility.FromJson<T>(Encoding.ASCII.GetString(jsonBytes)); // TODO: allows for utf8 encoding?
        return (ret, true);
    }

    private static bool DeleteData<T>(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning(string.Format("File does not exist: {0}", fullPath));
            return false;
        }

        try
        {
            File.Delete(fullPath);
            Debug.Log(string.Format("Data deleted from: {0}", fullPath.Replace("/", "\\")));
        } catch (Exception e)
        {
            Debug.LogError(string.Format("Failed to delete data at {0}: {1}", fullPath, e.Message));
            return false;
        }

        return true;
    }

    //    public static bool DeleteData(string dataFile)
    //    {
    //        bool success = false;

    //        string tempPath = Path.Combine(Application.persistentDataPath, DATA_FOLDER_NAME);
    //        tempPath = Path.Combine(tempPath, dataFile);

    //        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
    //        {
    //            Debug.LogError(string.Format("{0}{1}", "Directory does not exist: ", tempPath));
    //            return false;
    //        }

    //        if (!File.Exists(tempPath))
    //        {
    //            Debug.Log(string.Format("{0}{1}", "File does not exist: ", tempPath));
    //            return false;
    //        }

    //        try
    //        {
    //            File.Delete(tempPath);
    //            Debug.Log(string.Format("{0}{1}", "Data deleted from: ", tempPath.Replace("/", "\\")));
    //            success = true;
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogError(string.Format("{0}{1}", "Failed To Delete Data: ", e.Message));
    //        }

    //        return success;
    //    }
}