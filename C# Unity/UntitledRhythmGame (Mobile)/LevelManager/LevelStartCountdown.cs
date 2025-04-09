using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelStartCountdown : MonoBehaviour
{
    //Once Scene is loaded
    //Countdown
    //Get Ready
    //Go
    public static bool levelStarting = true;
    public TextMeshProUGUI LevelStartCDTMP;
    private AudioSource musicSource;
    private float CDSpeed = 1f;

    private void Start()
    {
        Time.timeScale = 1f;
        musicSource = GameObject.FindWithTag("MusicSource").GetComponent<AudioSource>();
        GetComponent<PlayerManager>().PlayerNoControl();
        levelStarting = true;
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        GetComponent<LevelEditor>().scrolling = false;
        yield return new WaitForSeconds(CDSpeed);
        
        levelStarting = false;

        LevelStartCDTMP.text = "Go";

        musicSource.Play();
        GetComponent<LevelEditor>().StartScrolling();
        GetComponent<PlayerStats>().enabled = true;
        GetComponent<PlayerManager>().PlayerHasControl();

        yield return new WaitForSeconds(CDSpeed / 2);
        LevelStartCDTMP.gameObject.SetActive(false);
    }
}
