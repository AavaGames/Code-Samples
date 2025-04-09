using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBeat : MonoBehaviour
{
    public float zoomAmount = 1.215f;
    [Tooltip("Zoom in and zoom out time.")]
    public float zoomTime = 0.25f;
    Camera cam;
    private float originalCamSize;
    private bool zoomIn = false;
    private bool zoomOut = false;
    private float zoomInGoal = 0;
    public bool testBeat = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalCamSize = cam.orthographicSize;
    }

    private void Update() {
        if(testBeat)
        {
            Beat();
            testBeat = false;
        }
        
        if (zoomIn)
        {
            float addZoom = (zoomAmount / zoomTime) * Time.deltaTime;

            cam.orthographicSize -= addZoom;

            if(cam.orthographicSize < zoomInGoal)
            {
                cam.orthographicSize = zoomInGoal;

                zoomIn = false;

                zoomOut = true;
            }
        }   
        else if (zoomOut)
        {
            float addZoom = (zoomAmount / zoomTime) * Time.deltaTime;

            cam.orthographicSize += addZoom;
            
            if(cam.orthographicSize > originalCamSize)
            {
                cam.orthographicSize = originalCamSize;
                zoomOut = false;
            }
        } 
    }

    public void Beat()
    {
        cam.orthographicSize = originalCamSize;
        zoomInGoal = cam.orthographicSize - zoomAmount;

        zoomIn = true;
    }
}
