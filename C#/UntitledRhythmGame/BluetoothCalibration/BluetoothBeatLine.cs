using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothBeatLine : MonoBehaviour
{
    private bool triggered = false;
    public GameObject perfect;
    void Update()
    {
        if (!triggered)
        {
            if (transform.position.y < -9f)
            {
                triggered = true;

                perfect.SetActive(false);
                perfect.SetActive(true);
            }
        }
        else
        {
            if (transform.position.y < -25f)
            {
                transform.parent.GetComponent<BluetoothBeatLineSpawner>().beatLineCount--;
                Destroy(gameObject);
            }
        }
    }
}
