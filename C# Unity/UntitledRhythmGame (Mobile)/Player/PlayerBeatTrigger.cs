using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBeatTrigger : MonoBehaviour
{
    //private void OnEnable()
    //{
    //    GetComponent<AudioSource>().Play();
    //    StartCoroutine(WaitOneFrame());
    //}

    //IEnumerator WaitOneFrame()
    //{
    //    yield return new WaitForEndOfFrame();
    //    StartCoroutine(Disable());
    //}

    //IEnumerator Disable()
    //{
    //    yield return new WaitForEndOfFrame();
    //    gameObject.SetActive(false);
    //}

    private void OnEnable()
    {
        GetComponent<AudioSource>().Play();
        StartCoroutine(Disable());
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(0.05f);
        gameObject.SetActive(false);
    }
}
