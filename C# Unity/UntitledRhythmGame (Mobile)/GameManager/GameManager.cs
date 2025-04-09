using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{    
    public static GameManager instance = null;

    private void Awake() {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        //make this a singleton
        DontDestroyOnLoad(gameObject);

#if (UNITY_IOS || UNITY_ANDROID)
        Application.targetFrameRate = 60;
#endif
    }

    public void DestroyGameManager()
    {
        instance = null;
        Destroy(gameObject);
    }
}
