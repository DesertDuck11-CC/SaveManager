This is the repository for my Save Manager tool

This tool is a package built for Unity that allows the user to save and load data in binary format of many different variable types using keys through user-made save files

Feature List:
- Save file creation
- Ability to set save file path 
- Save data
- Load data
- Auto-save
- Multiple debug functions
- Ability to have an on-screen console

Example:

<pre>
    <code>
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

        SaveManager.ToggleSaveWarning();

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
    </code>
</pre>
