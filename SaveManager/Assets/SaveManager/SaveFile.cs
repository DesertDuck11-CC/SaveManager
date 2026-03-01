using System.Collections.Generic;
using System.IO;

public class SaveFile
{
    private string fileName;
    private string filePath;

    private Dictionary<string, object> dataList = new Dictionary<string, object>();

    public SaveFile(string fileName, string filePath)
    {
        switch (SaveManager.getSaveType())
        {
            case SaveType.JSON:
                this.fileName = fileName + ".json";
                break;
            case SaveType.BINARY:
                this.fileName = fileName + ".dat";
                break;
            default:
                break;
        }

        this.filePath = Path.Combine(filePath, this.fileName);
    }

    public string getName()
    {
        return fileName;
    }

    public string getFilePath()
    {
        return filePath;
    }

    public Dictionary<string, object> getDataList()
    {
        return dataList;
    }
}
