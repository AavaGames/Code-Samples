using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CalculateLevelEndPosition : MonoBehaviour
{
    [SerializeField]
    private LevelEditor levelManager;
    private Vector3 endPosition = Vector3.zero;
    [Tooltip("DO NOT PRESS IN PLAY MODE")]
    //public bool setPosition = false;
    //public AudioSource musicSource;
    private AudioClip musicClip;

    private void OnEnable() 
    {
        levelManager = GameObject.FindWithTag("LevelManager").GetComponent<LevelEditor>();
    }

    void Update()
    {
        if (levelManager.setLevelEndPosition)
        {
            levelManager.setLevelEndPosition = false;

            transform.parent.transform.localPosition = Vector3.zero;
            
            musicClip = levelManager.musicSource.clip;
            float songLength = musicClip.length;
            float bpm = levelManager.bpm;
            float distancePerBeat = levelManager.distancePerBeat;

            endPosition.y = (bpm * distancePerBeat) * (songLength / 60);
            transform.position = endPosition;
        }
    }
}
