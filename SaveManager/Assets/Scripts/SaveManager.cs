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
    private static SaveType saveType = SaveType.BINARY;

    private static string saveFilePath = Application.persistentDataPath; 

    public static Dictionary<string, object> dataList = new Dictionary<string, object>();

    public static List<SaveFile> files = new List<SaveFile>();
    public static SaveFile activeFile = null;

    #region File Write On Quit

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        Application.quitting += OnQuit;
    }

    private static void OnQuit()
    {
        WriteToFile();
    }

    #endregion

    #region Data Functions

    public static void Save<T>(string key, T data)
    {
        if(activeFile == null)
        {
            Debug.LogError($"ERROR: There is no active file");
            return;
        }

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
    }

    public static T Load<T>(string key)
    {
        if (activeFile == null)
        {
            Debug.LogError($"ERROR: There is no active file");
            return default;
        }

        if (dataList.Count == 0)
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

    #endregion

    #region File Functions

    public static SaveFile CreateFile(string fileName)
    {
        SaveFile file = new SaveFile(fileName, saveFilePath);

        files.Add(file);

        return file;
    }

    public static void SetFile(SaveFile file)
    {
        if (!files.Contains(file))
        {
            files.Add(file);
        }

        

        activeFile = file;
    }

    private static void WriteToFile()
    {
        if (!File.Exists(activeFile.getFilePath()))
        {
            Debug.Log("ERROR: No file set to write to");
            return;
        }

        using (FileStream fileStream = new(activeFile.getFilePath(), FileMode.Create))
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
        if (!File.Exists(activeFile.getFilePath()))
        {
            Debug.Log("ERROR: No file set to read from");
            return;
        }

        dataList.Clear();

        using (FileStream fileStream = new(activeFile.getFilePath(), FileMode.Open))
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

    #endregion

    #region Binary Serialization

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

    #endregion

    #region Save Manager Settings Functions

    // Save Manager Settings Functions
    public static void SetFilePath(string filePath)
    {
        saveFilePath = filePath;
    }

    public static void SetSaveType(SaveType type)
    {
        saveType = type;
    }

    #endregion

    #region Debug Tool Function

    // Debug Tool Functions
    public static void PrintData(string key)
    {
        if(dataList.Count == 0)
        {
            ReadFile();
        }

        if(dataList.ContainsKey(key))
        {
            Debug.Log(dataList[key]);
        }
        else
        {
            Debug.LogError($"ERROR: {key} does not exist");
        }
    }

    public static void PrintKeys()
    {
        foreach(var pair in dataList)
        {
            Debug.Log(pair.Key);
        }
    }

    public static bool CheckKey(string key)
    {
        return dataList.ContainsKey(key);
    }

    public static SaveType getSaveType()
    {
        return saveType;
    }

    #endregion
}
