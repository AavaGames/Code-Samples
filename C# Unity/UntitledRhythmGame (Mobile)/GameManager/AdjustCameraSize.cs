using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustCameraSize : MonoBehaviour
{
    public float aspectRatio = 0;

    void Start()
    {
        aspectRatio = Camera.main.aspect;

        //9:18.5
        if (aspectRatio <= 0.49)
        {
            Camera.main.orthographicSize = 16.432f;
            transform.position += new Vector3(0, 1.5f, -20);
        }
        //5:9
        else if (aspectRatio < 0.56)
        {
            Camera.main.orthographicSize = 14.405f;
        }
        //9:16
        else if (aspectRatio >= 0.56)
        {
            Camera.main.orthographicSize = 14.23f;
        }
        
    }
}
