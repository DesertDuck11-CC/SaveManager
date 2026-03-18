using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public enum SaveType
{
    JSON,
    BINARY
}

public static class SaveManager
{
    private static SaveType saveType = SaveType.BINARY;

    private static string saveFilePath = Application.persistentDataPath;

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
        foreach (var file in files)
        {
            WriteToFile(file);
        }
    }

    #endregion

    #region Data Functions

    public static void Save<T>(string key, T data, SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if(fileInUse == activeFile && fileInUse == null)
        {
            Debug.LogError($"ERROR: There is no active file");
            return;
        }

        if (data == null)
        {
            Debug.LogError($"ERROR: {data} is set to null");
            return;
        }

        if (fileInUse.getDataList().ContainsKey(key))
        {
            Debug.LogWarning($"Warning: Key: {key} has already been used. Overwriting data");
        }

        fileInUse.getDataList()[key] = data;
    }

    public static T Load<T>(string key, SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == activeFile && fileInUse == null)
        {
            Debug.LogError($"ERROR: There is no active file");
            return default;
        }

        if (fileInUse.getDataList().Count == 0)
        {
            ReadFile(fileInUse);
        }

        if (fileInUse.getDataList().TryGetValue(key, out var value))
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

    private static void WriteToFile(SaveFile file)
    {
        if (file == null)
        {
            Debug.Log("ERROR: File does not exist");
            return;
        }

        using (FileStream fileStream = new(file.getFilePath(), FileMode.Create))
        using (BinaryWriter writer = new(fileStream))
        {
            writer.Write(file.getDataList().Count);

            foreach (var pair in file.getDataList())
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

    private static void ReadFile(SaveFile file)
    {
        if (!File.Exists(file.getFilePath()))
        {
            Debug.Log("ERROR: No file set to read from");
            return;
        }

        file.getDataList().Clear();

        using (FileStream fileStream = new(file.getFilePath(), FileMode.Open))
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

                file.getDataList().Add(key, obj);
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

    #region Auto Save

    private static bool autoSave = false;
    private static bool autoSaveRunning = false;

    /// <summary>
    /// Delay between auto-saves in seconds
    /// </summary>
    public static float autoSaveDelay = 60.0f;

    private static CancellationTokenSource autoSaveCTS;

    private static async Task AutoSave(CancellationToken token)
    {
        autoSaveRunning = true;

        try
        {
            while (!autoSaveCTS.IsCancellationRequested)
            {
                await Task.Delay((int)(autoSaveDelay * 1000.0f), token);

                foreach (var file in files)
                {
                    WriteToFile(file);
                }

                Debug.Log("Auto-Save: Saved Files");
            }
        }
        catch (TaskCanceledException) { }

        autoSaveRunning = false;
    }

    public static void ToggleAutoSave()
    {
        autoSave = !autoSave;

        Debug.Log(autoSave ? "Auto Save Turned On!" : "Auto Save Turned Off!");

        if (autoSave)
        {
            if (autoSaveRunning) return;

            autoSaveCTS = new CancellationTokenSource();
            _ = AutoSave(autoSaveCTS.Token);
        }
        else
        {
            autoSaveCTS?.Cancel();
        }
    }

    #endregion

    #endregion

    #region Debug Tool Function

    // Debug Tool Functions
    public static void PrintData(string key, SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == null)
        {
            Debug.LogError($"ERROR: {fileInUse} does not exist");
        }

        if (fileInUse.getDataList().Count == 0)
        {
            ReadFile(fileInUse);
        }

        if(fileInUse.getDataList().ContainsKey(key))
        {
            Debug.Log($"Key: {key} Data: {fileInUse.getDataList()[key]}");
        }
        else
        {
            Debug.LogError($"ERROR: {key} does not exist");
        }
    }

    public static void PrintKeys(SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == null)
        {
            Debug.LogError($"ERROR: {fileInUse} does not exist");
        }

        foreach (var pair in fileInUse.getDataList())
        {
            Debug.Log(pair.Key);
        }
    }

    public static bool CheckKey(string key, SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == null)
        {
            Debug.LogError($"ERROR: {fileInUse} does not exist");
        }

        return fileInUse.getDataList().ContainsKey(key);
    }

    public static SaveType getSaveType()
    {
        return saveType;
    }

    #endregion
}
