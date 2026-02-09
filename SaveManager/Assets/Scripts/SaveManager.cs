using UnityEngine;
using System.Collections.Generic;

public static class SaveManager
{
    private static string saveFilePath;

    public static void SaveData(SaveData saveData, string fileName)
    {
        string json = JsonUtility.ToJson(saveData);
    }

    public static void PrintData(SaveData saveData)
    {
        foreach (KeyValuePair<string, object> pair in saveData.GetData())
        {
            Debug.Log("Key: " + pair.Key + "\nValue: " + pair.Value);
        }
    }

    public static T GetVar<T>(SaveData saveData, string dataName)
    {
        if(saveData.GetData().TryGetValue(dataName, out var value))
        {
            return (T)value;
        }
        else
        {
            Debug.LogError("ERROR: variable not found");
            return default;
        }            
    }
}
