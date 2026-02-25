using System.IO;

public class SaveFile
{
    private string fileName;
    private string filePath;

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

        this.filePath = Path.Combine(filePath, fileName);
    }

    public string getName()
    {
        return fileName;
    }

    public string getFilePath()
    {
        return filePath;
    }
}
