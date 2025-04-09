using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour
{
    private bool active = false;
    // Start is called before the first frame update
    void Awake()
    {
        //Disable 2DBoxColliders

        //Beatline or BeatlineSplit 
        if(gameObject.name.Contains("BeatLine"))
        {
            foreach(Transform child in transform)
            {
                child.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }

        active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active && transform.position.y < 40)
        {
            //Activate 2DBoxColliders
            if(gameObject.name.Contains("BeatLine"))
            {
                foreach(Transform child in transform)
                {
                    child.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                }
            }
            else
            {
                gameObject.GetComponent<BoxCollider2D>().enabled = true;
            }

            active = true;
        }
        else if (active && transform.position.y < -40)
        {
            Destroy(gameObject);
        }
    }
}
