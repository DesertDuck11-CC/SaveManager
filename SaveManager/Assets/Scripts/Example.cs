using UnityEngine;

public class Example : MonoBehaviour
{
    [SerializeField] SaveData saveData;

    private void Awake()
    {
        SaveManager.Load<SaveData>("saveData");
    }

    private void OnDestroy()
    {
        SaveManager.Save("saveData", saveData);
    }

    [System.Serializable]
    private struct SaveData
    {
        public string playerName;
        public float health;
        public Vector3 position;
    }
}
