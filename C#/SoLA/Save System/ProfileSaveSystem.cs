using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

public static class ProfileSaveSystem
{
    private static string FolderPath()
    {
        string path = Application.dataPath + "/Saves";
        return path;
    }
    private static string Path()
    {
        string path = FolderPath() + "/Profile_" + SaveData.settingsData.Profile + ".aava";
        return path;
    }
     
    public static void Save(ProfileSaveData data)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ProfileSaveData));

        FileStream stream = new FileStream(Path(), FileMode.Create);

        serializer.Serialize(stream, data);

        stream.Close();
    }

    public static ProfileSaveData Load()
    {
        if (!File.Exists(FolderPath()))
        {
            Directory.CreateDirectory(FolderPath());
        }
        if (!File.Exists(Path()))
        {
            Save(new ProfileSaveData());
        }
        XmlSerializer formatter = new XmlSerializer(typeof(ProfileSaveData));

        FileStream stream = new FileStream(Path(), FileMode.Open);

        ProfileSaveData data = (ProfileSaveData)formatter.Deserialize(stream);

        stream.Close();
        return data;
    }

#region Manual Profile Input
    public static ProfileSaveData Load(int profile)
    {
        string path = FolderPath() + "/Profile_" + profile + ".aava";

        if (!File.Exists(FolderPath()))
        {
            Directory.CreateDirectory(FolderPath());
        }
        if (!File.Exists(path))
        {
            Save(new ProfileSaveData());
        }
        XmlSerializer formatter = new XmlSerializer(typeof(ProfileSaveData));

        FileStream stream = new FileStream(path, FileMode.Open);

        ProfileSaveData data = (ProfileSaveData)formatter.Deserialize(stream);

        stream.Close();

        return data;
    }

    public static bool CheckIfProfileExists(int profile)
    {
        string path = FolderPath() + "/Profile_" + profile + ".aava";

        if (File.Exists(path))
        {
            return true;
        }
        else
            return false;
    }
#endregion
    public static void DeleteProfile(int profile)
    {
        string path = FolderPath() + "/Profile_" + profile + ".aava";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted Profile " + profile);
        }
    }
}
