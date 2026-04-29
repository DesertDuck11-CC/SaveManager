using UnityEngine;
using TMPro;
using System.Collections;

public class SaveTest : MonoBehaviour
{
    public enum Test
    {
        None,
        One,
        Two
    }

    public TMP_Text consoleText;

    public string playerName;
    public float health;
    public Vector3 position = Vector3.one;

    public Test test;

    public int[,] testArray = new int[3,3];

    private void Start()
    {
        SaveManager.consoleText = consoleText;

        //SaveManager.ToggleSaveWarning();

        StartCoroutine(TestSaveManager());

        return;

        testArray[0,0] = 0;
        testArray[1,1] = 1;
        testArray[2,2] = 2;

        // Create some data to save
        SaveData saveData = new SaveData(playerName, health, position);

        SaveFile file = SaveManager.CreateFile("save");
        SaveManager.SetFile(file);

        // Save the data to 1st file
        SaveManager.Save("SaveData", saveData);
        SaveManager.Save("health", 20.0f);
        SaveManager.Save("Array", testArray);

        // Show Overwriting Data
        SaveManager.Save("SaveData", saveData);

        // Create new empty data then give it information based on the saved data
        SaveData newData = SaveManager.Load<SaveData>("SaveData");
        int newHealth = (int)SaveManager.Load<float>("health");
        int[,] newArray = SaveManager.Load<int[,]>("Array");

        // Log all new variable data
        Debug.Log(newData.playerName);
        Debug.Log(newData.health);
        Debug.Log(newData.position);
        Debug.Log(newHealth);

        Debug.Log(testArray[0, 0]);
        Debug.Log(testArray[1, 1]);
        Debug.Log(testArray[2, 2]);

        // Multiple save files
        SaveFile file2 = SaveManager.CreateFile("save2");
        SaveManager.SetFile(file2);

        // Save data to 2nd file
        SaveManager.Save("Test", test);

        // Show that same key can be used in multiple files
        SaveManager.Save("SaveData", saveData);

        // File 2 new data
        Test newTest = SaveManager.Load<Test>("Test");
        SaveData newData2 = SaveManager.Load<SaveData>("SaveData");

        SaveData newData3 = SaveManager.Load<SaveData>("SaveData", file);

        // Log all new variable data
        Debug.Log(newTest);
        Debug.Log(newData2.playerName);
        Debug.Log(newData2.health);
        Debug.Log(newData2.position);
        Debug.Log(newData3.playerName);
        Debug.Log(newData3.health);
        Debug.Log(newData3.position);

        // Show Debug functions
        SaveManager.PrintData("Test");
        SaveManager.PrintKeys();
        Debug.Log(SaveManager.CheckKey("health"));

        Debug.Log(SaveManager.autoSaveDelay);
        SaveManager.autoSaveDelay = 10;
        Debug.Log(SaveManager.autoSaveDelay);
        SaveManager.ToggleAutoSave();
    }

    private IEnumerator TestSaveManager()
    {
        testArray[0, 0] = 0;
        testArray[1, 1] = 1;
        testArray[2, 2] = 2;

        // Create some data to save
        SaveData saveData = new SaveData(playerName, health, position);

        SaveFile file = SaveManager.CreateFile("save");
        yield return new WaitForSeconds(0.2f);
        SaveManager.SetFile(file);
        yield return new WaitForSeconds(0.2f);

        // Save the data to 1st file
        SaveManager.Save("SaveData", saveData);
        yield return new WaitForSeconds(0.2f);
        SaveManager.Save("health", 20.0f);
        yield return new WaitForSeconds(0.2f);
        SaveManager.Save("name", playerName);
        yield return new WaitForSeconds(0.2f);
        SaveManager.Save("Array", testArray);
        yield return new WaitForSeconds(0.2f);

        // Show Overwriting Data
        SaveManager.Save("SaveData", saveData);
        yield return new WaitForSeconds(0.2f);

        // Create new empty data then give it information based on the saved data
        SaveData newData = SaveManager.Load<SaveData>("SaveData");
        yield return new WaitForSeconds(0.2f);
        int newHealth = (int)SaveManager.Load<float>("health");
        yield return new WaitForSeconds(0.2f);
        string newName = SaveManager.Load<string>("name");
        yield return new WaitForSeconds(0.2f);
        int[,] newArray = SaveManager.Load<int[,]>("Array");
        yield return new WaitForSeconds(0.2f);

        // Multiple save files
        SaveFile file2 = SaveManager.CreateFile("save2");
        yield return new WaitForSeconds(0.2f);
        SaveManager.SetFile(file2);
        yield return new WaitForSeconds(0.2f);

        // Save data to 2nd file
        SaveManager.Save("Test", test);
        yield return new WaitForSeconds(0.2f);

        // Show that same key can be used in multiple files
        SaveManager.Save("SaveData", saveData);
        yield return new WaitForSeconds(0.2f);

        // File 2 new data
        Test newTest = SaveManager.Load<Test>("Test");
        yield return new WaitForSeconds(0.2f);
        SaveData newData2 = SaveManager.Load<SaveData>("SaveData");
        yield return new WaitForSeconds(0.2f);

        SaveData newData3 = SaveManager.Load<SaveData>("SaveData", file);
        yield return new WaitForSeconds(0.2f);

        // Log all new variable data

        // Show Debug functions
        SaveManager.PrintData("Test");
        yield return new WaitForSeconds(0.2f);
        SaveManager.PrintKeys();
        yield return new WaitForSeconds(0.2f);

        SaveManager.autoSaveDelay = 10;
        SaveManager.ToggleAutoSave();
        yield return new WaitForSeconds(0.2f);
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
