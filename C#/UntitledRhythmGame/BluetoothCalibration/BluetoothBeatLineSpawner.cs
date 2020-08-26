using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothBeatLineSpawner : MonoBehaviour
{
    public GameObject beatLine;
    public GameObject perfect;
    private GameObject tempObject;
    private float yPosition = 0f;
    private float yAdded = 10f;
    public int beatLineCount = 0;

    private void Update() {

        if(beatLineCount < 6)
        {
            SpawnBeatLine();
        }
    }

    private void SpawnBeatLine()
    {
        yPosition += yAdded;

        tempObject = Instantiate(beatLine, new Vector3(0, 20, 0), Quaternion.identity);
        tempObject.transform.parent = gameObject.transform;
        tempObject.transform.localPosition = new Vector3(0, yPosition, 0);
        tempObject.GetComponent<BluetoothBeatLine>().perfect = perfect;

        beatLineCount++;
    }

    public void PurgeBeatLines()
    {
        foreach(Transform theChild in transform)
        {
            Destroy(theChild.gameObject);
            beatLineCount = 0;
            yPosition = 0f;
        }
    }
}
