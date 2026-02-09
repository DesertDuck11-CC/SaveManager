using UnityEngine;

public class SaveTest : MonoBehaviour
{
    SaveData playerData = new SaveData();

    [SerializeField] int playerHealth = 0;
    [SerializeField] string playerName = "";

    int newHealth;

    void Start()
    {
        playerData.AddData(playerHealth, nameof(playerHealth));
        playerData.AddData(playerName, nameof(playerName));
        
        SaveManager.SaveData(playerData, "PlayerSave");

        playerHealth = 12;

        SaveManager.PrintData(playerData);

        //newHealth = SaveManager.GetVar<int>(playerData, nameof(playerHealth));

        newHealth = SaveManager.GetVar<int>(playerData, nameof(playerHealth));
    }
}
