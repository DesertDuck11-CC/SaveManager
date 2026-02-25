using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTest : MonoBehaviour
{
    public enum Test
    {
        None,
        One,
        Two
    }

    public string playerName;
    public float health;
    public Vector3 position = Vector3.one;

    public Test test;

    private void Start()
    {
        // Create some data to save
        SaveData saveData = new SaveData(playerName, health, position);

        SaveFile file = SaveManager.CreateFile("save");
        SaveManager.SetFile(file);

        // Save the data
        SaveManager.Save("SaveData", saveData);
        SaveManager.Save("health", 20.0f);

        SaveFile file2 = SaveManager.CreateFile("save2");
        SaveManager.SetFile(file2);

        SaveManager.Save("Test", test);

        // Show Overwrite logic/warning
        SaveManager.Save("SaveData", saveData);

        // Create new empty data then give it information based on the saved data
        SaveData newData = SaveManager.Load<SaveData>("SaveData");
        int newHealth = (int)SaveManager.Load<float>("health");
        Test newTest = SaveManager.Load<Test>("Test");

        // Log all new variable data
        Debug.Log(newData.playerName);
        Debug.Log(newData.health);
        Debug.Log(newData.position);

        Debug.Log(newHealth);

        Debug.Log(newTest);
    }

    [System.Serializable]
    private struct SaveData
    {
        public string playerName; 
        public float health;
        public Vector3 position;

        public SaveData(string playerName, float health, Vector3 position)
        {
            this.playerName = playerName;
            this.health = health;
            this.position = position;
        }
    }
}
