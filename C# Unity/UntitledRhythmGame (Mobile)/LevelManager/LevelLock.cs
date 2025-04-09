using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelLock : MonoBehaviour
{
    bool isLocked = true;
    public string levelName;
    public Image crossed;
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        //Check if HARD level has been completed, then set isLocked to false
        if (PlayerPrefs.GetInt(levelName + "_HighScore") > 0)
        {
            isLocked = false;
        }

        //IF level is locked, disable button interaction
        if (isLocked)
        {
            button.interactable = false;
            crossed.enabled = true;
        }
        else
        {
            button.interactable = true;
            crossed.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
