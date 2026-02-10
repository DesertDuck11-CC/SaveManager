using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SaveData
{
    public List<string> keys = new();
    public List<string> values = new();
}

public static class SaveManager
{
    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static Dictionary<string, object> dataList = new Dictionary<string, object>();

    public static void Save<T>(string key, T data)
    {
        if (data == null)
        {
            Debug.LogError("ERROR: Data is set to null");
            return;
        }

        if (dataList.ContainsKey(key))
        {
            Debug.LogWarning("Warning: Key has already been used");
        }

        dataList[key] = data;

        WriteToFile();
    }

    public static T Load<T>(string key)
    {
        if(dataList.Count == 0)
        {
            ReadFile();
        }

        if (dataList.TryGetValue(key, out var json))
        {
            return JsonUtility.FromJson<T>((string)json);
        }
        
        Debug.LogError("ERROR: Variable not found");            
        return default;
    }   

    private static void WriteToFile()
    {
        SaveData saveData = new SaveData();

        foreach (var pair in dataList)
        {
            saveData.keys.Add(pair.Key);
            saveData.values.Add(JsonUtility.ToJson(pair.Value));
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
    }

    private static void ReadFile()
    {
        if (!File.Exists(saveFilePath))
        {
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        dataList.Clear();

        for (int i = 0; i < saveData.keys.Count; i++)
        {
            dataList.Add(saveData.keys[i], saveData.values[i]);
        }
    }

    public static void SetFilePath(string filePath)
    {
        saveFilePath = filePath;
    }
}
