using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public static int score = 0;                                   //Store player's points
    public static float maxHealth = 100;
    public static float health = 0;
    public Text healthDisplay;
    public Transform healthBar;
    public TextMeshProUGUI scoreTMP;
    public float timeUntilScore = 0.1f;
    private float scoreTimer = 0;
    public int scoreOverTime = 1;
    public static int highScore = 0;
    string levelName;
    void Start()
    {
        levelName = SceneManager.GetActiveScene().name;
        //Debug.Log(levelName);
        highScore = PlayerPrefs.GetInt(levelName + "_HighScore"); //Get saved highscore of currently open scene
        //Debug.Log("" + highScore);
        health = maxHealth;

        score = 0;
    }

    void Update()
    {
        healthDisplay.text = health.ToString();//Shows player health on screen

        healthBar.transform.localScale = new Vector3(health / maxHealth, 1, 1);
        

        if (health <= 0)
        {
            //use to intitiate gameover scene

            healthBar.transform.localScale = new Vector3(0, 1, 1);

           GetComponent<LevelManager>().GameOver();
           // gameOver.SetActive(true);
           //Scenemanager.LoadScene(SceneManager.GetActiveScene().builIndex);
        }

        scoreTimer += Time.deltaTime;
        if (scoreTimer > timeUntilScore)
        {
            score += scoreOverTime; 
            scoreTimer = 0;

        }

        scoreTMP.text = score.ToString();
    }

    public static void TakingDamage(float dmg)
    {
        health -= dmg;
    }

    public static void AddHealth(float addedHealth)
    {
        health += addedHealth;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public static void AddPoints(int amount)
    {
        score += amount;
    }

    void DebugClearPrefs()
    {
        //Only for testing. Clears all values saved in prefs
        PlayerPrefs.DeleteAll();
    }
}

