using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public enum SaveType
{
    JSON,
    BINARY
}

public static class SaveManager
{
    private static SaveType _type;

    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "save.txt");  

    public static Dictionary<string, object> dataList = new Dictionary<string, object>();

    public static void Save<T>(string key, T data)
    {
        if (data == null)
        {
            Debug.LogError($"ERROR: {data} is set to null");
            return;
        }

        if (dataList.ContainsKey(key))
        {
            Debug.LogWarning($"Warning: Key: {key} has already been used. Overwriting data");
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

        if (dataList.TryGetValue(key, out var value))
        {
            if(value is T typedValue)
            {
                return typedValue;
            }       
        }
        
        Debug.LogError($"ERROR: Variable not found for {key}");
        return default;
    }   

    private static void WriteToFile()
    {
        using (FileStream fileStream = new(saveFilePath, FileMode.Create))
        using (BinaryWriter writer = new(fileStream))
        {
            writer.Write(dataList.Count);

            foreach (var pair in dataList)
            {
                writer.Write(pair.Key);

                Type type = pair.Value.GetType();
                writer.Write(type.AssemblyQualifiedName);

                byte[] bytes = SerializeToBytes(pair.Value);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }            
    }

    private static void ReadFile()
    {
        if (!File.Exists(saveFilePath))
        {
            return;
        }

        dataList.Clear();

        using (FileStream fileStream = new(saveFilePath, FileMode.Open))
        using (BinaryReader reader = new(fileStream))
        {
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string typeName = reader.ReadString();

                int length = reader.ReadInt32();
                byte[] bytes = reader.ReadBytes(length);

                Type type = Type.GetType(typeName);
                object obj = DeserializeFromBytes(bytes, type);

                dataList.Add(key, obj);
            }
        }       
    }

    private static byte[] SerializeToBytes(object obj)
    {
        if(obj.GetType().IsPrimitive || obj is string)
        {
            return System.Text.Encoding.UTF8.GetBytes(obj.ToString());
        }
        string json = JsonUtility.ToJson(obj);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    private static object DeserializeFromBytes(byte[] data, Type type)
    {
        string str = System.Text.Encoding.UTF8.GetString(data);
        if (type.IsPrimitive || type == typeof(string))
        {
            return Convert.ChangeType(str, type);
        }
        return JsonUtility.FromJson(str, type);
    }

    public static void SetFilePath(string filePath)
    {
        saveFilePath = filePath;
    }

    public static void SetSaveType(SaveType type)
    {
        _type = type;
    }
}
