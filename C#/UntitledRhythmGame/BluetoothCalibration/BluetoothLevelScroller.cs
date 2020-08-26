using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothLevelScroller : MonoBehaviour
{
    public BluetoothLevelEditor levelEditor;

    private void OnEnable()
    {
        levelEditor = GameObject.FindWithTag("LevelManager").GetComponent<BluetoothLevelEditor>();
    }

    void FixedUpdate()
    {
        if (levelEditor.scrolling)
        {
            Scroll(levelEditor.bpm, levelEditor.distancePerBeat);
        }
    }

    private void Scroll(float bpm, float distancePerBeat)
    {
        //Invert bpm to negative to push level down, make bpm into bps
        float speed = ((bpm * distancePerBeat) * -1) / 60;
        transform.localPosition += new Vector3(0, speed * Time.deltaTime, 0);
    }
}
