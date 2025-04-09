using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatPoints : MonoBehaviour
{
    public bool active = true;
    public bool triggered = false;

    public int sideScore = 10000;
    public int bonusScore = 20000;

    public bool beatSide = false;
    public bool beatBonus = false;
    private BoxCollider2D left;
    private BoxCollider2D right;
    private BoxCollider2D bonus;
    
    public GameObject player;
    public bool playerExit = false;

    private void Start()
    {
        bonus = transform.GetChild(0).GetComponent<BoxCollider2D>();
        left = transform.GetChild(1).GetComponent<BoxCollider2D>();
        right = transform.GetChild(2).GetComponent<BoxCollider2D>();
    }

    void LateUpdate()
    {
        if (active)
        { 
            if (triggered)
            {
                if (beatBonus)
                {
                    PlayerStats.score += bonusScore;
                    player.GetComponentInParent<PlayerController>().beatRating.TriggerPerfect();
                    active = false;
                }
                else if (beatSide)
                {
                    PlayerStats.score += sideScore;
                    player.GetComponentInParent<PlayerController>().beatRating.TriggerGood();
                    active = false;
                }
                else if (playerExit)
                {
                    player.GetComponentInParent<PlayerController>().beatRating.TriggerMiss();
                    active = false;
                }
            }
        }
    }
}
