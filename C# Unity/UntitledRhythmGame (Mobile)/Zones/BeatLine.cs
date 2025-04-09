using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatLine : MonoBehaviour
{
    public enum ZoneType { BEATSIDE, BEATBONUS }
    [Tooltip("What type of zone is this?")]
    public ZoneType objectType;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponentInParent<BeatPoints>().player = other.gameObject;

            if (other.gameObject.GetComponentInParent<PlayerController>().beatTriggerActive)
            {
                if (objectType == ZoneType.BEATBONUS)
                {
                    GetComponentInParent<BeatPoints>().beatBonus = true;
                }
                else if (objectType == ZoneType.BEATSIDE)
                {
                    GetComponentInParent<BeatPoints>().beatSide = true;
                }

                GetComponentInParent<BeatPoints>().triggered = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            GetComponentInParent<BeatPoints>().player = other.gameObject;
            GetComponentInParent<BeatPoints>().playerExit = true;
            GetComponentInParent<BeatPoints>().triggered = true;
        }
    }
}

