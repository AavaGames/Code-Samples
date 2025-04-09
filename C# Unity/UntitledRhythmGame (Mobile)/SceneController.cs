using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class SceneController : MonoBehaviour
{
    public static SceneController i;
    ScreenFade fade;
    [SerializeField]
    private TextMeshProUGUI progressText;
    [SerializeField]

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInChildren<Canvas>(true);

        //Prevents gameObject from being destroyed between scenes
        if (i == null)
        {
            i = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);//DestroyImmediate(this); // or gameObject
    }

    void Update()
    {
        if (Time.timeScale == 0f && (SceneManager.GetActiveScene().name == "LevelSelect")) Time.timeScale = 1f;
    }

    //Takes in the name of the level to load.
    public void FadeOutToScene(string lvlName)
    {
        //Update the progress percentage text, reset to 0
        UpdateProgressUI(0);
        //Begin the process of actually loading in the scene
        StartCoroutine(BeginLoad(lvlName));

        //Debug.Log("Loading triggered");
    }

/*
void OnEnable()
{
    //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
    SceneManager.sceneLoaded += OnLevelFinishedLoading;
}
void OnDisable()
{
    //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change
    // as soon as this script is disabled. 
    // Remember to always have an unsubscription for every delegate you subscribe to!
    SceneManager.sceneLoaded -= OnLevelFinishedLoading;
}

void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
{
    if (GameObject.Find("ScreenFade") != null)
    {
        fade = GameObject.Find("ScreenFade").GetComponent<ScreenFade>();
    }

    //Debug.Log("Level Loaded");
    //Debug.Log(scene.name);
    //Debug.Log(mode);
}*/

    private IEnumerator BeginLoad(string sceneName)
    {
        canvas.gameObject.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        //Debug.Log(operation);
        
        while (!operation.isDone)
        {
            UpdateProgressUI(operation.progress);
            yield return null;
        }

        UpdateProgressUI(operation.progress);
        operation = null;
        canvas.gameObject.SetActive(false);
    }

    private void UpdateProgressUI(float progress)
    {
        progressText.text = (int)(progress * 100f) + "%";
        //Debug.Log(progressText.text);
    }

}
