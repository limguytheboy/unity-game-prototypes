using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SavePlayer(PlayerNetwork player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(player);

        formatter.Serialize(stream, data);
        stream.Close();
    }
    
    public static PlayerData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.dat";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path,FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        else
        {
            return null;
        }
    }

    public static void SaveDefaultGameData()
    {
        GameData defaultData = new GameData(); // Use constructor to set default values
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/defaultGameData.dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, defaultData);
        stream.Close();

        Debug.Log("Default game data saved.");
    }

    public static GameData LoadDefaultGameData()
    {
        string path = Application.persistentDataPath + "/defaultGameData.dat";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameData defaultData = formatter.Deserialize(stream) as GameData;
            stream.Close();
            return defaultData;
        }
        else
        {
            return null;
        }
    }
}
