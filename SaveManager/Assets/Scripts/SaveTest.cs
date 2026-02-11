using UnityEngine;

public class SaveTest : MonoBehaviour
{
    public string playerName;
    public float health;
    public Vector3 position = Vector3.one;

    private void Start()
    {
        // Create some data to save
        SaveData saveData = new SaveData();

        saveData.playerName = playerName;
        saveData.health = health;
        saveData.position = position;

        Debug.Log(saveData.playerName);
        Debug.Log(saveData.health);
        Debug.Log(saveData.position);

        // Save the data
        SaveManager.Save("SaveData", saveData);
        SaveManager.Save("name", name);

        // Show Overwrite logic/warning
        SaveManager.Save("SaveData", saveData);

        // Clear the data for testing purposes
        SaveManager.dataList.Clear();

        // Create new empty data then give it information based on the saved data
        SaveData newData = SaveManager.Load<SaveData>("SaveData");

        Debug.Log(newData.playerName);
        Debug.Log(newData.health);
        Debug.Log(newData.position);
    }

    [System.Serializable]
    private struct SaveData
    {
        public string playerName; 
        public float health;
        public Vector3 position;
    }
}
