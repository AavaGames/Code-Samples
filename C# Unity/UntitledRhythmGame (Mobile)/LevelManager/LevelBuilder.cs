using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    // Level Builder Stuff
    /*----------------------------------------------------------/
    /                                                           /
    */          public bool levelBuild = false;                 /*
    /                                                           /
    /----------------------------------------------------------*/

    public GameObject duetPlayer;
    public GameObject leftPlayer;
    public GameObject rightPlayer;
    public enum LevelBuild { DangerZone, MoveZone, StopZone, BeatLine, BeatLineSplit, SwitchToDuet, SwitchToSplit, Collectable }
    public LevelBuild ArrowUpDuet;
    public LevelBuild ArrowUpSplit;
    public LevelBuild ArrowDownDuet;
    public LevelBuild ArrowDownSplit;
    public LevelBuild ArrowLeftDuet;
    public LevelBuild ArrowLeftSplit;
    public LevelBuild ArrowRightDuet;
    public LevelBuild ArrowRightSplit;

    private float keyTimerL;
    private float keyTimerR;
    private float keyTimerU;
    private float keyTimerD;

    private Transform levelFolderBuild;

    private GameObject DangerZonePrefab;
    private GameObject MoveZonePrefab;
    private GameObject StopZonePrefab;
    private GameObject BeatLinePrefab;
    private GameObject BeatLineSplitPrefab;
    private GameObject SwitchToDuetPrefab;
    private GameObject SwitchToSplitPrefab;
    private GameObject CollectablePrefab;

    private GameObject UpPrefabDuet;
    private GameObject UpPrefabSplit;

    private GameObject DownPrefabDuet;
    private GameObject DownPrefabSplit;

    private GameObject LeftPrefabDuet;
    private GameObject LeftPrefabSplit;

    private GameObject RightPrefabDuet;
    private GameObject RightPrefabSplit;

    private Transform LevelCreatorFolder;

    private GameObject tmpObj;

    public float keyDelay;
    [Tooltip("")]
    public bool flopOn;
    private bool flop = false;
    private Transform DuetPos;
    private Transform CyanPos;
    private Transform MagPos;

    public bool DuetPosSet = false;
    public bool SplitPosSet = false;

    /*----------------------------------------------------------*/

    void Start()
    {
        // lvl builder stuff

        LevelCreatorFolder = GameObject.Find("_LevelCreator").transform;

        //------------------------------------------------------------------

        levelFolderBuild = GameObject.Find("_LevelFolder").transform;
        DangerZonePrefab = GameObject.Find("DangerZone");
        MoveZonePrefab = GameObject.Find("MoveZone");
        StopZonePrefab = GameObject.Find("StopZone");
        BeatLinePrefab = GameObject.Find("BeatLine");
        BeatLineSplitPrefab = GameObject.Find("BeatLineSplit");
        SwitchToDuetPrefab = GameObject.Find("SwitchToDuet");
        SwitchToSplitPrefab = GameObject.Find("SwitchToSplit");
        CollectablePrefab = GameObject.Find("Collectable");

        //----------------------------- DANGER ZONE -----------------------------
        if (ArrowUpDuet == LevelBuild.DangerZone) UpPrefabDuet = DangerZonePrefab;
        if (ArrowUpSplit == LevelBuild.DangerZone) UpPrefabSplit = DangerZonePrefab;
        if (ArrowDownDuet == LevelBuild.DangerZone) DownPrefabDuet = DangerZonePrefab;
        if (ArrowDownSplit == LevelBuild.DangerZone) DownPrefabSplit = DangerZonePrefab;
        if (ArrowLeftDuet == LevelBuild.DangerZone) LeftPrefabDuet = DangerZonePrefab;
        if (ArrowLeftSplit == LevelBuild.DangerZone) LeftPrefabSplit = DangerZonePrefab;
        if (ArrowRightDuet == LevelBuild.DangerZone) RightPrefabDuet = DangerZonePrefab;
        if (ArrowRightSplit == LevelBuild.DangerZone) RightPrefabSplit = DangerZonePrefab;
        //----------------------------- MOVE ZONE -----------------------------
        if (ArrowUpDuet == LevelBuild.MoveZone) UpPrefabDuet = MoveZonePrefab;
        if (ArrowUpSplit == LevelBuild.MoveZone) UpPrefabSplit = MoveZonePrefab;
        if (ArrowDownDuet == LevelBuild.MoveZone) DownPrefabDuet = MoveZonePrefab;
        if (ArrowDownSplit == LevelBuild.MoveZone) DownPrefabSplit = MoveZonePrefab;
        if (ArrowLeftDuet == LevelBuild.MoveZone) LeftPrefabDuet = MoveZonePrefab;
        if (ArrowLeftSplit == LevelBuild.MoveZone) LeftPrefabSplit = MoveZonePrefab;
        if (ArrowRightDuet == LevelBuild.MoveZone) RightPrefabDuet = MoveZonePrefab;
        if (ArrowRightSplit == LevelBuild.MoveZone) RightPrefabSplit = MoveZonePrefab;
        //----------------------------- STOP ZONE -----------------------------
        if (ArrowUpDuet == LevelBuild.StopZone) UpPrefabDuet = StopZonePrefab;
        if (ArrowUpSplit == LevelBuild.StopZone) UpPrefabSplit = StopZonePrefab;
        if (ArrowDownDuet == LevelBuild.StopZone) DownPrefabDuet = StopZonePrefab;
        if (ArrowDownSplit == LevelBuild.StopZone) DownPrefabSplit = StopZonePrefab;
        if (ArrowLeftDuet == LevelBuild.StopZone) LeftPrefabDuet = StopZonePrefab;
        if (ArrowLeftSplit == LevelBuild.StopZone) LeftPrefabSplit = StopZonePrefab;
        if (ArrowRightDuet == LevelBuild.StopZone) RightPrefabDuet = StopZonePrefab;
        if (ArrowRightSplit == LevelBuild.StopZone) RightPrefabSplit = StopZonePrefab;
        //----------------------------- BEAT LINE -----------------------------
        if (ArrowUpDuet == LevelBuild.BeatLine) UpPrefabDuet = BeatLinePrefab;
        if (ArrowUpSplit == LevelBuild.BeatLine) UpPrefabSplit = BeatLinePrefab;
        if (ArrowDownDuet == LevelBuild.BeatLine) DownPrefabDuet = BeatLinePrefab;
        if (ArrowDownSplit == LevelBuild.BeatLine) DownPrefabSplit = BeatLinePrefab;
        if (ArrowLeftDuet == LevelBuild.BeatLine) LeftPrefabDuet = BeatLinePrefab;
        if (ArrowLeftSplit == LevelBuild.BeatLine) LeftPrefabSplit = BeatLinePrefab;
        if (ArrowRightDuet == LevelBuild.BeatLine) RightPrefabDuet = BeatLinePrefab;
        if (ArrowRightSplit == LevelBuild.BeatLine) RightPrefabSplit = BeatLinePrefab;
        //----------------------------- BEAT LINE SPLIT -----------------------
        if (ArrowUpDuet == LevelBuild.BeatLineSplit) UpPrefabDuet = BeatLineSplitPrefab;
        if (ArrowUpSplit == LevelBuild.BeatLineSplit) UpPrefabSplit = BeatLineSplitPrefab;
        if (ArrowDownDuet == LevelBuild.BeatLineSplit) DownPrefabDuet = BeatLineSplitPrefab;
        if (ArrowDownSplit == LevelBuild.BeatLineSplit) DownPrefabSplit = BeatLineSplitPrefab;
        if (ArrowLeftDuet == LevelBuild.BeatLineSplit) LeftPrefabDuet = BeatLineSplitPrefab;
        if (ArrowLeftSplit == LevelBuild.BeatLineSplit) LeftPrefabSplit = BeatLineSplitPrefab;
        if (ArrowRightDuet == LevelBuild.BeatLineSplit) RightPrefabDuet = BeatLineSplitPrefab;
        if (ArrowRightSplit == LevelBuild.BeatLineSplit) RightPrefabSplit = BeatLineSplitPrefab;
        //----------------------------- SWITCH DUET ---------------------------
        if (ArrowUpDuet == LevelBuild.SwitchToDuet) UpPrefabDuet = SwitchToDuetPrefab;
        if (ArrowUpSplit == LevelBuild.SwitchToDuet) UpPrefabSplit = SwitchToDuetPrefab;
        if (ArrowDownDuet == LevelBuild.SwitchToDuet) DownPrefabDuet = SwitchToDuetPrefab;
        if (ArrowDownSplit == LevelBuild.SwitchToDuet) DownPrefabSplit = SwitchToDuetPrefab;
        if (ArrowLeftDuet == LevelBuild.SwitchToDuet) LeftPrefabDuet = SwitchToDuetPrefab;
        if (ArrowLeftSplit == LevelBuild.SwitchToDuet) LeftPrefabSplit = SwitchToDuetPrefab;
        if (ArrowRightDuet == LevelBuild.SwitchToDuet) RightPrefabDuet = SwitchToDuetPrefab;
        if (ArrowRightSplit == LevelBuild.SwitchToDuet) RightPrefabSplit = SwitchToDuetPrefab;
        //----------------------------- SWITCH SPLIT --------------------------
        if (ArrowUpDuet == LevelBuild.SwitchToSplit) UpPrefabDuet = SwitchToSplitPrefab;
        if (ArrowUpSplit == LevelBuild.SwitchToSplit) UpPrefabSplit = SwitchToSplitPrefab;
        if (ArrowDownDuet == LevelBuild.SwitchToSplit) DownPrefabDuet = SwitchToSplitPrefab;
        if (ArrowDownSplit == LevelBuild.SwitchToSplit) DownPrefabSplit = SwitchToSplitPrefab;
        if (ArrowLeftDuet == LevelBuild.SwitchToSplit) LeftPrefabDuet = SwitchToSplitPrefab;
        if (ArrowLeftSplit == LevelBuild.SwitchToSplit) LeftPrefabSplit = SwitchToSplitPrefab;
        if (ArrowRightDuet == LevelBuild.SwitchToSplit) RightPrefabDuet = SwitchToSplitPrefab;
        if (ArrowRightSplit == LevelBuild.SwitchToSplit) RightPrefabSplit = SwitchToSplitPrefab;
        //----------------------------- COLLECTABLE ---------------------------
        if (ArrowUpDuet == LevelBuild.Collectable) UpPrefabDuet = CollectablePrefab;
        if (ArrowUpSplit == LevelBuild.Collectable) UpPrefabSplit = CollectablePrefab;
        if (ArrowDownDuet == LevelBuild.Collectable) DownPrefabDuet = CollectablePrefab;
        if (ArrowDownSplit == LevelBuild.Collectable) DownPrefabSplit = CollectablePrefab;
        if (ArrowLeftDuet == LevelBuild.Collectable) LeftPrefabDuet = CollectablePrefab;
        if (ArrowLeftSplit == LevelBuild.Collectable) LeftPrefabSplit = CollectablePrefab;
        if (ArrowRightDuet == LevelBuild.Collectable) RightPrefabDuet = CollectablePrefab;
        if (ArrowRightSplit == LevelBuild.Collectable) RightPrefabSplit = CollectablePrefab;
        //---------------------------------------------------------------------
    }

    void Update()
    {
        // Level Builder Stuff
        if (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.DUET && DuetPosSet == false)
        {
            DuetPos = duetPlayer.transform;
            DuetPosSet = true;
        }
        else if (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.SPLIT && SplitPosSet == false)
        {
            CyanPos = leftPlayer.transform;
            MagPos = rightPlayer.transform;
            SplitPosSet = true;
        }

        // call build if you have the bool checked under 
        // LevelManager > LevelEditor > Level Build
        if (levelBuild == true) LevelBuildActivate();
    }

    void LevelBuildActivate()
    {
        keyTimerU += Time.deltaTime;
        keyTimerD += Time.deltaTime;
        keyTimerL += Time.deltaTime;
        keyTimerR += Time.deltaTime;

        /*------------------
         * Up Arrow Start
         -------------------*/

        if (Input.GetKey(KeyCode.UpArrow) && keyTimerU >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.DUET))
        {
            //Debug.Log("Up Duet Aligned: " + levelProgress);

            // [ If you are in DUET (One Ball) Mode ]
            keyTimerU = 0f;
            tmpObj = GameObject.Instantiate(UpPrefabDuet, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(DuetPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(4, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(-5, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }
        else if (Input.GetKey(KeyCode.UpArrow) && keyTimerU >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.SPLIT))
        {
            //Debug.Log("Up Split Aligned: " + levelProgress);

            // [ If you are in SPLIT (Two Ball) Mode ]
            keyTimerU = 0f;
            tmpObj = GameObject.Instantiate(UpPrefabSplit, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(0, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(MagPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f); flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(CyanPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }

        /*------------------
         * Up Arrow End
         -------------------*/

        /*------------------
         * Down Arrow Start
         -------------------*/
        if (Input.GetKey(KeyCode.DownArrow) && keyTimerD >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.DUET))
        {
            //Debug.Log("Down Duet Aligned: " + levelProgress);

            // [ If you are in DUET (One Ball) Mode ]
            keyTimerD = 0f;
            tmpObj = GameObject.Instantiate(DownPrefabDuet, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(DuetPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(4, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(-5, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow) && keyTimerD >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.SPLIT))
        {
            //Debug.Log("Down Split Aligned: " + levelProgress);

            // [ If you are in SPLIT (Two Ball) Mode ]
            keyTimerD = 0f;
            tmpObj = GameObject.Instantiate(DownPrefabSplit, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(0, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(MagPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(CyanPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }

        /*------------------
         * Down Arrow End
         -------------------*/

        /*------------------
         * Left Arrow Start
         -------------------*/

        if (Input.GetKey(KeyCode.LeftArrow) && keyTimerL >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.DUET))
        {
            //Debug.Log("Left Duet Aligned: " + levelProgress);

            // [ If you are in DUET (One Ball) Mode ]
            keyTimerL = 0f;
            tmpObj = GameObject.Instantiate(LeftPrefabDuet, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(DuetPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(4, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(-5, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && keyTimerL >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.SPLIT))
        {
            //Debug.Log("Left Split Aligned: " + levelProgress);

            // [ If you are in SPLIT (Two Ball) Mode ]
            keyTimerL = 0f;
            tmpObj = GameObject.Instantiate(LeftPrefabSplit, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(MagPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(-2.5f, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(-6.5f, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }

        /*------------------
         * Left Arrow End
         -------------------*/

        /*------------------
         * Right Arrow Start
         -------------------*/

        if (Input.GetKey(KeyCode.RightArrow) && keyTimerR >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.DUET))
        {
            //Debug.Log("Right Duet Aligned: " + levelProgress);

            // [ If you are in DUET (One Ball) Mode ]
            keyTimerR = 0f;
            tmpObj = GameObject.Instantiate(RightPrefabDuet, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(DuetPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(4, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(-5, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow) && keyTimerR >= keyDelay && (GetComponent<PlayerManager>().currentPlayerState == PlayerManager.PlayerState.SPLIT))
        {
            //Debug.Log("Right Split Aligned: " + levelProgress);

            // [ If you are in SPLIT (Two Ball) Mode ]
            keyTimerR = 0f;
            tmpObj = GameObject.Instantiate(RightPrefabDuet, LevelCreatorFolder);
            if (flopOn == false) tmpObj.transform.position = new Vector3(CyanPos.position.x, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
            else if (flopOn == true)
            {
                if (flop == true)
                {
                    tmpObj.transform.position = new Vector3(1.5f, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = false;
                }
                else if (flop == false)
                {
                    tmpObj.transform.position = new Vector3(5.5f, (tmpObj.transform.position.y - levelFolderBuild.transform.position.y - 8f), 0f);
                    flop = true;
                }
            }
        }

        /*------------------
         * Right Arrow End
         -------------------*/

    }
}
