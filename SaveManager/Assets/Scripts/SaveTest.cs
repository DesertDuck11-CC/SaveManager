using UnityEngine;

public class SaveTest : MonoBehaviour
{
    public string playerName;
    public float health;
    public Vector3 position = Vector3.one;

    private void Start()
    {
        SaveData saveData = new SaveData();

        saveData.playerName = playerName;
        saveData.health = health;
        saveData.position = position;

        Debug.Log(saveData.playerName);
        Debug.Log(saveData.health);
        Debug.Log(saveData.position);

        SaveManager.Save("SaveData", saveData);

        SaveManager.dataList.Clear();

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
