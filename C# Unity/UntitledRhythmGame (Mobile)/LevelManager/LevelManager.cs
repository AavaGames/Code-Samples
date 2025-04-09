using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    // Game Over, Restart and Level Complete vars
    private MusicManager MM;
    private SFXManager SFXM;
    private UIManager UIM;
    private string currentLevel;
    private LevelEditor lvlEditor;
    SceneController sceneController;
    AudioSource bgMusic;

    // Start is called before the first frame update
    void Start()
    {
        bgMusic = GameObject.Find("MusicForMainMenu&LevelSelect").GetComponent<AudioSource>();

        sceneController = (SceneController)FindObjectOfType(typeof(SceneController));
        // Level Complete, Game Over and Restart
        if (UIM == null)
        {
            UIM = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        }
        if (MM == null)
        {
            MM = GameObject.FindWithTag("MusicSource").GetComponent<MusicManager>();
        }
        if (SFXM == null)
        {
            SFXM = GameObject.FindWithTag("SFXManager").GetComponent<SFXManager>();
        }
        if (lvlEditor == null)
        {
            lvlEditor = GetComponent<LevelEditor>();
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        if (DetectZone.isInZone == true) DetectZone.isInZone = false;
        Camera.main.GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
        currentLevel = SceneManager.GetActiveScene().name;
        bgMusic.Stop();
        SceneManager.LoadScene(currentLevel);
    }

    public void BackToSongSelect()
    {
        Time.timeScale = 1f;
        sceneController.FadeOutToScene("LevelSelect");
    }

    public void LevelComplete()
    {
        UIM.levelComplete.SetActive(true);
        lvlEditor.HighScore();
        if (DetectZone.isInZone == true) DetectZone.isInZone = false;
        Camera.main.GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
        Time.timeScale = 0f;
        MM.GetComponent<AudioSource>().Pause();
        SFXM.GetComponent<AudioSource>().Pause();
        GameObject.FindWithTag("Score").GetComponent<TextMeshProUGUI>().SetText("" + PlayerStats.score);
    }

    public void GameOver()
    {
        UIM.gameOver.SetActive(true);
        if (DetectZone.isInZone == true) DetectZone.isInZone = false;
        Camera.main.GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
        Time.timeScale = 0f;
        MM.GetComponent<AudioSource>().Pause();
        SFXM.GetComponent<AudioSource>().Pause();
    }
}