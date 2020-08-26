using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ProfileSaveData
{
    public string Name = "Lillion";
    public int TotalDeathCount;
    public float TotalPlayTime;
    public DateTime LastDatePlayed;
    public bool FreshSave = true;
    public CurrentSessionSettings CurrentSession;
    public AccessibilitySettings Accessibility;
    public TempleFlagsSettings TempleFlags;

    public ProfileSaveData()
    {
        CurrentSession = new CurrentSessionSettings();
        Accessibility = new AccessibilitySettings();
        TempleFlags = new TempleFlagsSettings();
    }
}

public class CurrentSessionSettings
{
    public string CurrentScene = "000_PathZero_Temple";
    public string Checkpoint = "Checkpoint";

    public CurrentSessionSettings() { }
}

public class AccessibilitySettings
{
    public float GameSpeed = 1f;
    public AccessibilitySettings() { }
}

public class TempleFlagsSettings
{
    public bool SawCoatVanish = false;
    public bool ReachedIntermission = false;
    public float MeetingLillithEnding = 0;
    public bool ArtifactOne = false;
    public bool OnPathTwo = false;
    public bool ArtifactTwo = false;
    public bool MetThem = false;

    public TempleFlagsSettings() { }
}

