using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public static TMP_Text consoleText;

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

        if(autoSave)
        {
            ToggleAutoSave();
        }
    }

    #endregion

    #region Data Functions

    public static void Save<T>(string key, T data, SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == activeFile && fileInUse == null)
        {
            Debug.LogError($"ERROR: There is no active file");
            if (consoleText != null) consoleText.text += $"ERROR: There is no active file\n";
            return;
        }

        if (data == null)
        {
            Debug.LogError($"ERROR: {data} is set to null");
            if (consoleText != null) consoleText.text += $"ERROR: {data} is set to null\n";
            return;
        }

        if (saveWarning)
        {
            if (fileInUse.getDataList().ContainsKey(key))
            {
                Debug.LogWarning($"Warning: Key: {key} has already been used. Overwriting data");

                if (consoleText != null) consoleText.text += $"Warning: Key: {key} has already been used. Overwriting data\n";
            }
        }

        if (consoleText != null) consoleText.text += $"Saving {data.GetType().Name}: {data} to '{key}' in {fileInUse.getName()}\n";

        fileInUse.getDataList()[key] = data;
    }

    public static T Load<T>(string key, SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == activeFile && fileInUse == null)
        {
            Debug.LogError($"ERROR: There is no active file");

            if (consoleText != null) consoleText.text += $"ERROR: There is no active file\n";

            return default;
        }

        if (fileInUse.getDataList().Count == 0)
        {
            ReadFile(fileInUse);
        }

        if (fileInUse.getDataList().TryGetValue(key, out var value))
        {
            if (value is T typedValue)
            {
                if (consoleText != null) consoleText.text += $"Loaded {typeof(T).Name}: {typedValue} from '{key}' in {fileInUse.getName()}\n";

                return typedValue;
            }
        }

        Debug.LogError($"ERROR: Variable not found for {key}");

        if (consoleText != null) consoleText.text += $"ERROR: Variable not found for {key}\n";

        return default;
    }

    #endregion

    #region File Functions

    public static SaveFile CreateFile(string fileName)
    {
        SaveFile file = new SaveFile(fileName, saveFilePath);

        files.Add(file);

        if(consoleText != null) consoleText.text += $"New File Created: {fileName}\n";

        return file;
    }

    public static void SetFile(SaveFile file)
    {
        if (!files.Contains(file))
        {
            files.Add(file);
        }

        if (consoleText != null) consoleText.text += $"File Set to {file.getName()}\n";

        activeFile = file;

        ReadFile(file);
    }

    private static void WriteToFile(SaveFile file)
    {
        if (file == null)
        {
            Debug.Log("ERROR: File does not exist");
            if (consoleText != null) consoleText.text += "ERROR: File does not exist\n";
            return;
        }

        using FileStream fileStream = new(file.getFilePath(), FileMode.Create);
        using BinaryWriter writer = new(fileStream);

        var data = file.getDataList();

        writer.Write(data.Count);

        foreach (var pair in data)
        {
            writer.Write(pair.Key);

            BinarySerializer.WriteObject(writer, pair.Value);
        }
    }

    private static void ReadFile(SaveFile file)
    {
        if (!File.Exists(file.getFilePath()))
        {
            Debug.Log("ERROR: No file set to read from");
            if (consoleText != null) consoleText.text += "ERROR: No file set to read from\n";
            return;
        }

        file.getDataList().Clear();

        using FileStream fileStream = new(file.getFilePath(), FileMode.Open);
        using BinaryReader reader = new(fileStream);

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            object value = BinarySerializer.ReadObject(reader);

            file.getDataList()[key] = value;
        }
    }

    #endregion

    #region Save Manager Settings Functions

    // Save Manager Settings Functions
    public static void SetFilePath(string filePath)
    {
        saveFilePath = filePath;

        if (consoleText != null) consoleText.text += $"File path set to {filePath}\n";
    }

    public static void SetSaveType(SaveType type)
    {
        saveType = type;

        if (consoleText != null) consoleText.text += $"Save type set to {type}\n";
    }

    static bool saveWarning = false;
    public static void ToggleSaveWarning()
    {
        saveWarning = !saveWarning;

        if (consoleText != null) consoleText.text += $"Save warning set to {saveWarning}\n";
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
                if (consoleText != null) consoleText.text += "Auto-Save: Saved Files\n";
            }
        }
        catch (TaskCanceledException) { }

        autoSaveRunning = false;
    }

    public static void ToggleAutoSave()
    {
        autoSave = !autoSave;

        Debug.Log(autoSave ? "Auto Save Turned On!" : "Auto Save Turned Off!");
        if (consoleText != null) consoleText.text += autoSave ? "Auto Save Turned On!\n" : "Auto Save Turned Off!\n";

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
            if (consoleText != null) consoleText.text += $"ERROR: {fileInUse} does not exist\n";
        }

        if (fileInUse.getDataList().Count == 0)
        {
            ReadFile(fileInUse);
        }

        if (fileInUse.getDataList().ContainsKey(key))
        {
            Debug.Log($"Key: {key} Data: {fileInUse.getDataList()[key]}");
            if (consoleText != null) consoleText.text += $"Key: {key} Data: {fileInUse.getDataList()[key]}\n";
        }
        else
        {
            Debug.LogError($"ERROR: {key} does not exist");
            if (consoleText != null) consoleText.text += $"ERROR: {key} does not exist\n";
        }
    }

    public static void PrintKeys(SaveFile file = null)
    {
        SaveFile fileInUse = file != null ? file : activeFile;

        if (fileInUse == null)
        {
            Debug.LogError($"ERROR: {fileInUse} does not exist");
            if (consoleText != null) consoleText.text += $"ERROR: {fileInUse} does not exist\n";
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
            if (consoleText != null) consoleText.text += $"ERROR: {fileInUse} does not exist\n";
        }

        return fileInUse.getDataList().ContainsKey(key);
    }

    public static SaveType getSaveType()
    {
        return saveType;
    }

    #endregion
}