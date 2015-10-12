using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveLoad
{
    public static void SaveData<T>(string key, T data)
    {
        var formatter = new BinaryFormatter();
        var stream = new MemoryStream();

        formatter.Serialize(stream, data);

        PlayerPrefs.SetString(key, System.Convert.ToBase64String(stream.GetBuffer()));
    }

    public static T LoadData<T>(string key, T defaultValue)
    {
        var stringData = PlayerPrefs.GetString(key, null);

        if (!string.IsNullOrEmpty(stringData))
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(System.Convert.FromBase64String(stringData));

            T data = (T)formatter.Deserialize(stream);
            return data;
        }

        return defaultValue;
    }
}
