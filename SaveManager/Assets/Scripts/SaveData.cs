using System.Collections.Generic;

public class SaveData
{
    //private List<object> dataList = new List<object>();

    private Dictionary<string, object> dataList = new Dictionary<string, object>();

    public void AddData<T>(T data, string dataName)
    {
        if(!dataList.ContainsKey(dataName))
        {
            dataList.Add(dataName, data);
        }
    }

    public Dictionary<string, object> GetData()
    {
        return dataList;
    }
}
