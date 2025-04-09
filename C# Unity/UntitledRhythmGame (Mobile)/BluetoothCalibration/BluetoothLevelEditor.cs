using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using UnityEngine.SceneManagement;

public class BluetoothLevelEditor : MonoBehaviour
{
    public AudioSource musicSource;
    public Transform levelFolder;
    public BluetoothBeatLineSpawner spawner;
    private Vector3 levelFolderPosition = Vector3.zero;

    public TextMeshProUGUI delayTMP;

    public bool scrolling = true;
    [Tooltip("1 BPM = -1x (x = distance) per minute")]
    public float bpm = 110f;
    public float distancePerBeat = 6f;

    public float bluetoothDelay = 0;
    private float previousBluetoothDelay = 0f;

    AudioSource bgMusic;
    // Start is called before the first frame update
    void Start()
    {
        bgMusic = GameObject.Find("MusicForMainMenu&LevelSelect").GetComponent<AudioSource>();

        if (musicSource == null)
        {
            musicSource = GameObject.FindWithTag("MusicSource").GetComponent<AudioSource>();
        }
        if (levelFolder == null)
        {
            levelFolder = GameObject.FindWithTag("LevelFolder").transform;
        }
        levelFolder.GetComponent<BluetoothLevelScroller>().levelEditor = this;

        SavedSettings.ToggleDelayActive(true);
        bluetoothDelay = SavedSettings.delay;
        previousBluetoothDelay = bluetoothDelay;
        delayTMP.text = bluetoothDelay.ToString("0.00");//"" + bluetoothDelay;

        StartCoroutine(DelayedSyncMusicAndScroll());
    }

    public void ResumeMusic()
    {
        bgMusic.Play();
    }
    // Update is called once per frame
    void Update()
    {
        bluetoothDelay = SavedSettings.delay;

        if (previousBluetoothDelay != bluetoothDelay)
        {
            previousBluetoothDelay = bluetoothDelay;

            SavedSettings.delay = bluetoothDelay;
            delayTMP.text = "" + bluetoothDelay;

            SyncMusicAndScroll();
        }
    }

    private void SyncMusicAndScroll()
    {
        //Reset everything
        scrolling = false;
        musicSource.Stop();
        levelFolder.localPosition = new Vector3(0, 0, 0);
        spawner.PurgeBeatLines();
        //Start again
        StartCoroutine(ScrollingDelay());
        musicSource.Play();
    }

    private IEnumerator DelayedSyncMusicAndScroll()
    {
        yield return new WaitForSeconds(0.5f);

        //Reset everything
        scrolling = false;
        musicSource.Stop();
        levelFolder.localPosition = new Vector3(0, 0, 0);
        spawner.PurgeBeatLines();
        //Start again
        StartCoroutine(ScrollingDelay());
        musicSource.Play();
    }

    private IEnumerator ScrollingDelay()
    {
        yield return new WaitForSeconds(SavedSettings.delay);
        scrolling = true;
    }

    public void BackToSongSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
