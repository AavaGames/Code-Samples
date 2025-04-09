using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour
{
    //Welcome to the level editor
    //To configure the level correctly go down the list in the inspector and make sure everything is in its place
    //After the musicSource, levelFolder, endPoint, BPM, Distance Per Beat is all set
    //Press the Set Level End Position
    //The level is now configured to scroll as fast as the BPM * Distance Per Beat and last as long as the songs duration.
    //Do not place anything past the level end position.

    //You can use the Set Level Progress bar to scroll through the level and test sections if needed.

    public AudioSource musicSource;
    public Transform levelFolder;
    public Transform levelEndPoint;
    private Vector3 levelFolderPosition = Vector3.zero;

    string levelName;

    private LevelManager lvlManager;
    
    //Level Folder
    public bool scrolling = true;
    [Tooltip("1 BPM = -1x (x = distance) per minute")]
    public float bpm = 110f;
    public float distancePerBeat = 6f;

    //Set Level End Position
    [Tooltip("DO NOT PRESS IN PLAY MODE")]
    public bool setLevelEndPosition = false;

    [SerializeField]
    private float levelLength = 0;
    [SerializeField]
    private float musicLength = 0;

    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float levelProgress = 0;

    [Range(0.0f, 1.0f)]
    public float setLevelProgress;
    private float previousSetLevelProgress;
    public bool setProgress = false;

    private bool levelCompleted = false;

    private void OnEnable()
    {
        levelName = SceneManager.GetActiveScene().name; //Get active scene name

        if (musicSource == null)
        {
            musicSource = GameObject.FindWithTag("MusicSource").GetComponent<AudioSource>();
        }
        if (levelFolder == null)
        {
            levelFolder = GameObject.FindWithTag("LevelFolder").transform;
            levelFolder.GetComponent<LevelScroller>().levelManager = this;
        }
        if (levelEndPoint == null)
        {
            levelEndPoint = levelFolder.transform.GetChild(0).transform;
        }
        if (lvlManager == null)
        {
            lvlManager = GetComponent<LevelManager>();
        }
    }

    private void Update()
    {
        //Level Complete check
        if (Application.isPlaying && !levelCompleted)
        {
            if (levelProgress >= 1)
            {
                lvlManager.LevelComplete();
                levelCompleted = true;
            }
        }

        //Only trigger when progress variable has changed
        if (previousSetLevelProgress != setLevelProgress || setProgress)
        {
            levelCompleted = false;

            setProgress = false;
            previousSetLevelProgress = setLevelProgress;

            //For BT Delay
            scrolling = false;

            if (musicSource == null)
            {
                musicSource = GameObject.FindGameObjectWithTag("MusicSource").GetComponent<AudioSource>();
            }

            AudioClip musicClip = musicSource.clip;

            //Find length of level (y coord) and music (time length)
            levelLength = levelEndPoint.localPosition.y;
            musicLength = musicClip.length;

            //Set music progress
            musicSource.time = musicLength * setLevelProgress;
            //Set level progress
            levelFolderPosition.y = levelEndPoint.localPosition.y * setLevelProgress * -1;
            levelFolder.localPosition = levelFolderPosition;

            //For BT Delay
            if (!LevelStartCountdown.levelStarting)
            {
                StartScrolling();
            }
        }

        levelProgress = (levelFolder.localPosition.y * -1) / levelEndPoint.localPosition.y;
    }

    public void StartScrolling()
    {
        if (SavedSettings.delayActive == 1)
        {
            StartCoroutine(ScrollingDelay());
        }
        else
        {
            scrolling = true;
        }
    }

    private IEnumerator ScrollingDelay()
    {
        yield return new WaitForSeconds(SavedSettings.delay);
        scrolling = true;
    }

    public void HighScore()
    {
        //bool for best score / level completed

        //IF player's score is higher than the saved high score for this scene,
        if (PlayerStats.score > PlayerStats.highScore)
        {
            //THEN set the new high score for this scene
            PlayerPrefs.SetInt(levelName + "_HighScore", PlayerStats.score);
            PlayerPrefs.Save();
            PlayerStats.highScore = PlayerPrefs.GetInt(levelName + "_HighScore");
            Debug.Log("! New High Score = " + PlayerStats.highScore);
        }

        else if (PlayerStats.score <= PlayerStats.highScore)
        {
            GameObject.FindWithTag("HighScore").SetActive(false);
        }
    }
}
