using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TriggerType { ACTIVATE, DEACTIVATE }
    public TriggerType type;
    public GameObject TMPFolder;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            if (type == TriggerType.ACTIVATE)
            {
                TMPFolder.SetActive(true);
            }
            else
            {
                TMPFolder.SetActive(false);
            }
        }
    }
}
