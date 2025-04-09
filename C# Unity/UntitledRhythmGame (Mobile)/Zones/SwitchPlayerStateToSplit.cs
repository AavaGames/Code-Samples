using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPlayerStateToSplit : MonoBehaviour
{
    private PlayerManager playerManager;
    public bool active = true;
    private void Start() {
        playerManager = GameObject.FindWithTag("LevelManager").GetComponent<PlayerManager>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && active)
        {
            playerManager.TransitionToSplit();
            active = false;
        }
    }
}
