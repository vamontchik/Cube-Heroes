// NOTE:
// modified from https://stackoverflow.com/a/40966346 for an easy-to-use serialization class !!!

using System;
using System.IO;
using System.Text;
using UnityEngine;

public class DataSaver
{
    public static readonly string ALLY_DATA_FILENAME = "ally_data.json";
    public static readonly string DATA_FOLDER_NAME = "data";

    public static void SaveItemData(Item item)
    {
        string path = Path.Combine(Path.Combine(Application.persistentDataPath, DATA_FOLDER_NAME), ALLY_DATA_FILENAME);
 
        string dirName = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirName)) 
        { 
            Directory.CreateDirectory(dirName);
        }

        byte[] itemAsJson = Encoding.ASCII.GetBytes(JsonUtility.ToJson(item)); // TODO: allows for utf8 encoding?

        try
        {
            File.WriteAllBytes(path, itemAsJson);
            Debug.Log(string.Format("{0}{1}", "Saved Data to: ", path.Replace("/", "\\")));
        }
        catch (Exception e)
        {
            Debug.LogWarning(string.Format("{0}{1}", "Failed To PlayerInfo Data to: ", path.Replace("/", "\\")));
            Debug.LogWarning(string.Format("{0}{1}", "Error: ", e.Message));
        }
    }

    public static Item LoadItemData()
    {
        string path = Path.Combine(Path.Combine(Application.persistentDataPath, DATA_FOLDER_NAME), ALLY_DATA_FILENAME);

        string dirName = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirName))
        {
            Debug.LogWarning(string.Format("{0}{1}", "Directory does not exist: ", path));
            return null;
        }

        if (!File.Exists(path))
        {
            Debug.Log(string.Format("{0}{1}", "File does not exist: ", path));
            return null;
        }

        byte[] jsonBytes;
        try
        {
            jsonBytes = File.ReadAllBytes(path);
            Debug.Log(string.Format("{0}{1}", "Loaded Data from: ", path.Replace("/", "\\")));
        }
        catch (Exception e)
        {
            Debug.LogWarning(string.Format("{0}{1}", "Failed To Load Data from: ", path.Replace("/", "\\")));
            Debug.LogWarning(string.Format("{0}{1}", "Error: ", e.Message));
            return null;
        }

        string jsonData = Encoding.ASCII.GetString(jsonBytes); // TODO: allows for utf8 encoding?
        return JsonUtility.FromJson<Item>(jsonData);
    }

    //    public static bool DeleteData(string dataFile)
    //    {
    //        bool success = false;

    //        string tempPath = Path.Combine(Application.persistentDataPath, DATA_FOLDER_NAME);
    //        tempPath = Path.Combine(tempPath, dataFile);

    //        if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
    //        {
    //            Debug.LogWarning(string.Format("{0}{1}", "Directory does not exist: ", tempPath));
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
    //            Debug.LogWarning(string.Format("{0}{1}", "Failed To Delete Data: ", e.Message));
    //        }

    //        return success;
    //    }
}