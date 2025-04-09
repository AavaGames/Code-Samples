using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerDuet;
    public GameObject playerMagenta;
    public GameObject playerCyan;
    public GameObject duetBGFolder;
    public GameObject splitBGFolder;
    private GameObject cam;
    public enum PlayerState { DUET, SPLIT }
    public PlayerState currentPlayerState = PlayerState.DUET;
    private PlayerState previousPlayerState;
    private Vector3 duetStartPos = new Vector3(0, -9, 0);
    private Vector3 magentaStartPos = new Vector3(-4, -9, 0);
    private Vector3 cyanStartPos = new Vector3(4, -9, 0);
    private float duetCombinationSpeed = 20f;
    private float splitCombinationSpeed = 15f;
    private Vector3 magentaVelocity = Vector3.zero;
    private Vector3 cyanVelocity = Vector3.zero;
    private bool transitioningToDuet = false;
    private bool transitioningToSplit = false;
    private bool duetToCenter = false;
    private bool splitToPositions = false;

    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera");
        previousPlayerState = currentPlayerState;
    }

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.P))
        // {
        //     if (currentPlayerState == PlayerState.DUET)
        //     {
        //         TransitionToSplit();
        //     }
        //     else if (currentPlayerState == PlayerState.SPLIT)
        //     {
        //         TransitionToDuet();
        //     }
        // }

        if(previousPlayerState != currentPlayerState)
        {
            previousPlayerState = currentPlayerState;
            if (currentPlayerState == PlayerState.DUET)
            {
                //Take control of the split player pull them fast to the center
                playerMagenta.GetComponent<PlayerController>().hasControl = false;
                playerCyan.GetComponent<PlayerController>().hasControl = false;

                transitioningToDuet = true;
            }
            else if (currentPlayerState == PlayerState.SPLIT)
            {
                playerDuet.GetComponent<PlayerController>().hasControl = false;
                
                duetToCenter = true;
                transitioningToSplit = true;                
            }
        }

        if(transitioningToDuet)
        {
            float fractionOfJourney = splitCombinationSpeed * Time.deltaTime;

            playerMagenta.transform.position = Vector3.Lerp(playerMagenta.transform.position, duetStartPos, fractionOfJourney);

            playerCyan.transform.position = Vector3.Lerp(playerCyan.transform.position, duetStartPos, fractionOfJourney);
            
            if(Vector3.Distance(playerMagenta.transform.position, duetStartPos) < 0.1f &&
                Vector3.Distance(playerCyan.transform.position, duetStartPos) < 0.1f)
            {
                //Beat the camera
                cam.GetComponent<CameraBeat>().Beat();

                //Swap to Duet
                playerMagenta.SetActive(false);
                playerMagenta.transform.position = duetStartPos;

                playerCyan.SetActive(false);
                playerCyan.transform.position = duetStartPos;

                duetBGFolder.SetActive(true);
                splitBGFolder.SetActive(false);
                
                playerDuet.SetActive(true);

                //Play animation?


                //Give control back to player
                playerDuet.GetComponent<PlayerController>().hasControl = true;

                transitioningToDuet = false;
            }
        }

        if(transitioningToSplit)
        {
            if(duetToCenter)
            {
                float step = duetCombinationSpeed * Time.deltaTime;
                
                playerDuet.transform.position = Vector3.MoveTowards(playerDuet.transform.position, duetStartPos, step);

                if(Vector3.Distance(playerDuet.transform.position, duetStartPos) < 0.01f)
                {
                    cam.GetComponent<CameraBeat>().Beat();

                    //Play Animation
                    playerDuet.SetActive(false);
                    playerDuet.transform.position = duetStartPos;

                    //At the end of animation trigger event to switch to duet
                    playerMagenta.transform.position = duetStartPos;
                    playerMagenta.SetActive(true);

                    playerCyan.transform.position = duetStartPos;
                    playerCyan.SetActive(true);

                    splitBGFolder.SetActive(true);
                    duetBGFolder.SetActive(false);
                    
                    duetToCenter = false;
                    splitToPositions = true;
                }
            }

            if (splitToPositions)
            {
                float step = duetCombinationSpeed * Time.deltaTime;

                playerMagenta.transform.position = Vector3.MoveTowards(playerMagenta.transform.position, magentaStartPos, step);

                playerCyan.transform.position = Vector3.MoveTowards(playerCyan.transform.position, cyanStartPos, step);

                if(Vector3.Distance(playerMagenta.transform.position, magentaStartPos) < 0.01f &&
                    Vector3.Distance(playerCyan.transform.position, cyanStartPos) < 0.01f)
                {
                    //Give control back to player
                    playerMagenta.GetComponent<PlayerController>().hasControl = true;
                    playerCyan.GetComponent<PlayerController>().hasControl = true;
                    
                    splitToPositions = false;
                    transitioningToSplit = false;
                }
            }

        }
    }

    public void TransitionToSplit()
    {
        currentPlayerState = PlayerState.SPLIT;
    }

    public void TransitionToDuet()
    {
        currentPlayerState = PlayerState.DUET;
    }

    public void PlayerHasControl()
    {
        playerDuet.GetComponent<PlayerController>().hasControl = true;
        playerMagenta.GetComponent<PlayerController>().hasControl = true;
        playerCyan.GetComponent<PlayerController>().hasControl = true;
    }

    public void PlayerNoControl()
    {
        playerDuet.GetComponent<PlayerController>().hasControl = false;
        playerMagenta.GetComponent<PlayerController>().hasControl = false;
        playerCyan.GetComponent<PlayerController>().hasControl = false;
    }
}
