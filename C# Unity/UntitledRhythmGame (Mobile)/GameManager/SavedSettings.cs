using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedSettings : MonoBehaviour
{
    public static float masterVol;
    public static float musicVol;
    public static float sfxVol;

    public static int delayActive = 0;
    public static float delay = 0.5f;

    private void Start()
    {
        delayActive = PlayerPrefs.GetInt("BluetoothDelayActive");
        delay = PlayerPrefs.GetFloat("BluetoothDelay");
    }

    //DELAY ----------
    public static void IncreaseDelay()
    {
        ToggleDelayActive(true);
        delay = Mathf.Round((delay + 0.01f) * 100f) / 100f;

        if (delay > 1.491f)
        {
            delay = 1.50f;
        }

        PlayerPrefs.SetFloat("BluetoothDelay", delay);
    }

    public static void DecreaseDelay()
    {
        delay = Mathf.Round((delay - 0.01f) * 100f) / 100f;

        if (delay < 0.0099)
        {
            ToggleDelayActive(false);
            delay = 0f;
        }
        PlayerPrefs.SetFloat("BluetoothDelay", delay);
    }

    public void CallIncreaseDelay()
    {
        IncreaseDelay();
    }

    public void CallDecreaseDelay()
    {
        DecreaseDelay();
    }

    public void ResetDelay()
    {
        delay = 0.00f;
        PlayerPrefs.SetFloat("BluetoothDelay", delay);
    }

    public static void ToggleDelayActive(bool activate)
    {
        //If true = 1 else = 0
        delayActive = activate == true ? 1 : 0;
        PlayerPrefs.SetInt("BluetoothDelayActive", delayActive);
    }

    public void ButtonToggleDelayActive()
    {
        delayActive = delayActive == 0 ? 1 : 0;
        PlayerPrefs.SetInt("BluetoothDelayActive", delayActive);
    }

    //SOUND ----------
    public static void SaveSoundSettings(string sliderToSave)
    {
        if (sliderToSave == "MasterVolume")
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVol);
        }
        if (sliderToSave == "MusicVolume")
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVol);
        }
        if (sliderToSave == "SfxVolume")
        {
            PlayerPrefs.SetFloat("SfxVolume", sfxVol);
        }
        PlayerPrefs.Save();
    }
}
 