using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelScroller : MonoBehaviour
{
    public LevelEditor levelManager;

    private void OnEnable()
    {
        levelManager = GameObject.FindWithTag("LevelManager").GetComponent<LevelEditor>();
    }

    void FixedUpdate()
    {
        if (levelManager.scrolling)
        {
            Scroll(levelManager.bpm, levelManager.distancePerBeat);
        }
    }

    private void Scroll(float bpm, float distancePerBeat)
    {
        //Invert bpm to negative to push level down, make bpm into bps
        float speed = ((bpm * distancePerBeat) * -1) / 60;
        transform.localPosition += new Vector3(0, speed * Time.deltaTime, 0);
    }

}
