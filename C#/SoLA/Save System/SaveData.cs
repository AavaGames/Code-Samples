using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class SaveData : MonoBehaviour
{
    public static ProfileSaveData profileData;
    public static SettingsSaveData settingsData;

    private static Dictionary<string, dynamic> profileDataDictionary = new Dictionary<string, dynamic>();

    private void Awake()
    {
        //Load Settings for Profile #
        LoadSettingsData();

        LoadProfileData();
    }

    public static void SaveProfileData()
    {
        DictionaryToSaveData();

        ProfileSaveSystem.Save(profileData);
    }
    private static void LoadProfileData()
    {
        profileData = ProfileSaveSystem.Load();

        SaveDataToDictionary();
    }

    public static void SaveSettingsData()
    {
        SettingsSaveSystem.Save(settingsData);
    }
    private void LoadSettingsData()
    {
        settingsData = SettingsSaveSystem.Load();
    }

    public static void GetProfile(int profile)
    {
        //Save before switching
        SaveProfileData();

        //Switch profile and save
        settingsData.Profile = profile;
        SaveSettingsData();

        //Load new profile
        LoadProfileData();
    }

#region Dictionary

    /*
    When adding new variable to SaveData
        1. Add to ProfileSaveData
        2. Add to SaveDataToDictionary with nomenclature of the SaveData path
        3. Add to DictionaryToSaveData
    */

    public static dynamic GetDictionary(string key)
    {
        return profileDataDictionary[key];
    }

    public static void SetDictionary(string key, dynamic value)
    {
        profileDataDictionary[key] = value;
    }

    public static void AddToValueInDictionary(string key, dynamic toAdd)
    {
        profileDataDictionary[key] += toAdd;
    }

    public static void SubtractValueInDictionary(string key, dynamic toSubtract)
    {
        profileDataDictionary[key] -= toSubtract;
    }

    private static void SaveDataToDictionary()
    {
        profileDataDictionary.Clear();

        //General
        profileDataDictionary.Add("Name", profileData.Name);
        profileDataDictionary.Add("TotalDeathCount", profileData.TotalDeathCount);
        profileDataDictionary.Add("TotalPlayTime", profileData.TotalPlayTime);
        profileDataDictionary.Add("LastDatePlayed", profileData.LastDatePlayed);
        profileDataDictionary.Add("FreshSave", profileData.FreshSave);

        //CurrentSession
        profileDataDictionary.Add("CurrentSession_CurrentScene", profileData.CurrentSession.CurrentScene);
        profileDataDictionary.Add("CurrentSession_Checkpoint", profileData.CurrentSession.Checkpoint);

        //Accessibility
        profileDataDictionary.Add("Accessibility_GameSpeed", profileData.Accessibility.GameSpeed);

        //Temple Flags
        profileDataDictionary.Add("TempleFlags_SawCoatVanish", profileData.TempleFlags.SawCoatVanish);
        profileDataDictionary.Add("TempleFlags_ReachedIntermission", profileData.TempleFlags.ReachedIntermission);
        profileDataDictionary.Add("TempleFlags_MeetingLillithEnding", profileData.TempleFlags.MeetingLillithEnding);
        profileDataDictionary.Add("TempleFlags_ArtifactOne", profileData.TempleFlags.ArtifactOne);
        profileDataDictionary.Add("TempleFlags_OnPathTwo", profileData.TempleFlags.OnPathTwo);
        profileDataDictionary.Add("TempleFlags_ArtifactTwo", profileData.TempleFlags.ArtifactTwo);
        profileDataDictionary.Add("TempleFlags_MetThem", profileData.TempleFlags.MetThem);
    }

    private static void DictionaryToSaveData()
    {
        //General
        profileData.Name = profileDataDictionary["Name"];
        profileData.TotalDeathCount = profileDataDictionary["TotalDeathCount"];
        profileData.TotalPlayTime = profileDataDictionary["TotalPlayTime"];
        profileData.FreshSave = profileDataDictionary["FreshSave"];

        //CurrentSession
        profileData.CurrentSession.CurrentScene = profileDataDictionary["CurrentSession_CurrentScene"];
        profileData.CurrentSession.Checkpoint = profileDataDictionary["CurrentSession_Checkpoint"];

        //Accessibility
        profileData.Accessibility.GameSpeed = profileDataDictionary["Accessibility_GameSpeed"];

        //Temple Flags
        profileData.TempleFlags.SawCoatVanish = profileDataDictionary["TempleFlags_SawCoatVanish"];
        profileData.TempleFlags.ReachedIntermission = profileDataDictionary["TempleFlags_ReachedIntermission"];
        profileData.TempleFlags.MeetingLillithEnding = profileDataDictionary["TempleFlags_MeetingLillithEnding"];
        profileData.TempleFlags.ArtifactOne = profileDataDictionary["TempleFlags_ArtifactOne"];
        profileData.TempleFlags.OnPathTwo = profileDataDictionary["TempleFlags_OnPathTwo"];
        profileData.TempleFlags.ArtifactTwo = profileDataDictionary["TempleFlags_ArtifactTwo"];
        profileData.TempleFlags.MetThem = profileDataDictionary["TempleFlags_MetThem"];
    }

#endregion
}
