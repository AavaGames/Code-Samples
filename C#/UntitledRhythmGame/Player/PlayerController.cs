using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool hasControl = true;
    public float moveSpeed = 10;
    public bool currentlyMoving = false;

    public enum PlayerType { DUET, LEFT, RIGHT }
    public PlayerType objectType = PlayerType.DUET;
    private bool hittingWall = false;

    private Vector3 dashGoal = Vector3.zero;
    public float dashCooldown = 1.5f;
    private float dashTimer = 0f;
    public bool currentlyDashing = false;
    public float dashDistance = 5f;
    public float dashSpeed = 15f;

    [Tooltip("Amount of time that the player has stopped until the bool is switched back to false.")]
    public float currentlyMovingFalloffBuffer = 0.1f;
    private float currentlyMovingFalloffBufferTimer = 0f;

    public bool beatTriggerActive = false;
    public float beatTriggerTimerMax = 0.1f;
    private float beatTriggerTimer = 0;

    private float rightContainer = 7.49f;
    private float leftContainer = -7.49f;
    private float rightCircleLeftContainer = 0.5f;
    private float leftCircleRightContainer = -0.5f;

    public BeatRating beatRating;

    private void Start() {
        beatRating.playerController = gameObject.GetComponent<PlayerController>()   ;
    }

    void FixedUpdate()
    {
        if(hasControl)
        {
#if UNITY_STANDALONE
            if (objectType == PlayerType.DUET)
            {
                ControllerDash();
            }
            if (!currentlyDashing)
            {
                ControllerMovement();
            }
            Container();
#endif
#if (UNITY_ANDROID || UNITY_IOS)
            if (MobileInputDetector.usingController)
            {
                if (objectType == PlayerType.DUET)
                {
                    ControllerDash();
                }
                if (!currentlyDashing)
                {
                    ControllerMovement();
                }
                Container();
            }
            else
            {
                if (objectType == PlayerType.DUET)
                {
                    Dash();
                }
                if (!currentlyDashing)
                {
                    Movement();
                }
                Container();
            }
#endif
        }
    }

    private void Update()
    {
#if UNITY_STANDALONE
        ControllerBeatCheck();
#endif
#if (UNITY_ANDROID || UNITY_IOS)
        if (MobileInputDetector.usingController)
        {
            ControllerBeatCheck();
        }
        else
        {
            FindLeadingTouches();
            BeatCheck();
        }
#endif

        if (beatTriggerActive)
        {
            beatTriggerTimer += Time.deltaTime;
            if(beatTriggerTimer >= beatTriggerTimerMax)
            {
                beatTriggerActive = false;
            }
        }
    }

    //Lock inside of container
    private void Container()
    {
        Vector3 setPosition = transform.position;
        hittingWall = false;

        if(objectType == PlayerType.DUET)
        {
            if(transform.position.x > rightContainer)
            {
                setPosition.x = rightContainer;
                hittingWall = true;
            }
            else if(transform.position.x < leftContainer)
            {
                setPosition.x = leftContainer;
                hittingWall = true;
            }
        }
        else if (objectType == PlayerType.LEFT)
        {
            if (transform.position.x > leftCircleRightContainer)
            {
                setPosition.x = leftCircleRightContainer;
                hittingWall = true;
            }
            else if (transform.position.x < leftContainer)
            {
                setPosition.x = leftContainer;
                hittingWall = true;
            }
        }
        else if (objectType == PlayerType.RIGHT)
        {
            if (transform.position.x > rightContainer)
            {
                setPosition.x = rightContainer;
                hittingWall = true;
            }
            else if (transform.position.x < rightCircleLeftContainer)
            {
                setPosition.x = rightCircleLeftContainer;
                hittingWall = true;
            }
        }

        transform.position = setPosition;
    }

    private void BeatTriggerOn()
    {
        beatTriggerActive = true;
        beatTriggerTimer = 0f;
    }

    private void ControllerDash()
    {
        if (dashTimer != 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                dashTimer = 0f;
            }
        }
        else
        {
            if (Input.GetAxisRaw("DuetDash") != 0 && !currentlyDashing)
            {
                if (Input.GetAxisRaw("ControllerLeftHorizontal") != 0)
                {
                    Vector3 dashDirection = new Vector3(Input.GetAxis("ControllerLeftHorizontal"), 0, 0);
                    dashGoal = transform.position + (dashDirection.normalized * dashDistance);
                    currentlyDashing = true;
                }
                else if (Input.GetAxisRaw("LeftHorizontal") != 0)
                {
                    Vector3 dashDirection = new Vector3(Input.GetAxis("LeftHorizontal"), 0, 0);
                    dashGoal = transform.position + (dashDirection.normalized * dashDistance);
                    currentlyDashing = true;
                }
                else if (Input.GetAxisRaw("ControllerRightHorizontal") != 0)
                {
                    Vector3 dashDirection = new Vector3(Input.GetAxis("ControllerRightHorizontal"), 0, 0);
                    dashGoal = transform.position + (dashDirection.normalized * dashDistance);
                    currentlyDashing = true;
                }
                else if (Input.GetAxisRaw("RightHorizontal") != 0)
                {
                    Vector3 dashDirection = new Vector3(Input.GetAxis("RightHorizontal"), 0, 0);
                    dashGoal = transform.position + (dashDirection.normalized * dashDistance);
                    currentlyDashing = true;
                }
            }

            if (currentlyDashing)
            {
                transform.position = Vector3.MoveTowards(transform.position, dashGoal, dashSpeed * Time.deltaTime);

                if(Vector3.Distance(transform.position, dashGoal) < 0.01f || hittingWall)
                {
                    dashTimer = dashCooldown;
                    currentlyDashing = false;
                }
            }
        }
        
    }
    private void ControllerMovement()
    {
        bool leftJoystick = false;
        bool leftKeyboard = false;
        bool rightJoystick = false;
        bool rightKeyboard = false;

        //Check input type
        if (Input.GetAxisRaw("ControllerLeftHorizontal") != 0 || Input.GetAxisRaw("ControllerLeftVertical") != 0)
        {
            leftJoystick = true;
        }
        else if (Input.GetAxisRaw("LeftHorizontal") != 0 || Input.GetAxisRaw("LeftVertical") != 0)
        {
            leftKeyboard = true;
        }

        if (Input.GetAxisRaw("ControllerRightHorizontal") != 0 || Input.GetAxisRaw("ControllerRightVertical") != 0)
        {
            rightJoystick = true;
        }
        else if (Input.GetAxisRaw("RightHorizontal") != 0 || Input.GetAxisRaw("RightVertical") != 0)
        {
            rightKeyboard = true;
        }

        //Check PlayerType
        if (objectType == PlayerType.DUET)
        {
            if (leftJoystick)
            {
                //Act on according to input
                transform.position += new Vector3(Input.GetAxis("ControllerLeftHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else if (rightJoystick)
            {
                transform.position += new Vector3(Input.GetAxis("ControllerRightHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else if (leftKeyboard)
            {
                transform.position += new Vector3(Input.GetAxisRaw("LeftHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else if (rightKeyboard)
            {
                transform.position += new Vector3(Input.GetAxisRaw("RightHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else
            {
                ControllerPlayerNotMoving();
            }
        }
        else if (objectType == PlayerType.LEFT)
        {
            if (leftJoystick)
            {
                transform.position += new Vector3(Input.GetAxis("ControllerLeftHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else if (leftKeyboard)
            {
                transform.position += new Vector3(Input.GetAxisRaw("LeftHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else
            {
                ControllerPlayerNotMoving();
            }
        }
        else if (objectType == PlayerType.RIGHT)
        {
            if (rightJoystick)
            {
                transform.position += new Vector3(Input.GetAxis("ControllerRightHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else if (rightKeyboard)
            {
                transform.position += new Vector3(Input.GetAxisRaw("RightHorizontal"), 0, 0) * moveSpeed * Time.deltaTime;
                ControllerPlayerMoving();
            }
            else
            {
                ControllerPlayerNotMoving();
            }
        }
    }

    private void ControllerPlayerMoving()
    {
        currentlyMoving = true;
        currentlyMovingFalloffBufferTimer = 0f;
    }

    private void ControllerPlayerNotMoving()
    {
        currentlyMovingFalloffBufferTimer += Time.deltaTime;
        if (currentlyMovingFalloffBufferTimer > currentlyMovingFalloffBuffer)
        {
            currentlyMoving = false;
        }
    }

    private void ControllerBeatCheck()
    {
        if (objectType == PlayerType.DUET)
        {
            if (Input.GetButtonDown("LeftBeat"))
            {
                BeatTriggerOn();
            }
            else if (Input.GetButtonDown("RightBeat"))
            {
                BeatTriggerOn();
            }
        }
        else if (objectType == PlayerType.LEFT)
        {
            if (Input.GetButtonDown("LeftBeat"))
            {
                BeatTriggerOn();
            }
        }
        else if (objectType == PlayerType.RIGHT)
        {
            if (Input.GetButtonDown("RightBeat"))
            {
                BeatTriggerOn();
            }
        }
    }

#if (UNITY_IOS || UNITY_ANDROID)
    
    public PlayerController oppositeObjectType;
    public Touch leadingTouch;
    public Touch? checkForLeadingTouch;

    private float leadingTouchTopContainer = -5f;
    private float leadingTouchMiddleContainer = 0f;

    private void FindLeadingTouches()
    {
        if(Input.touchCount > 0)
        {
            if (objectType == PlayerType.DUET)
            {
                //find new leading touch
                if (checkForLeadingTouch == null)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                        touchWorldPosition.z = 0f;
                        //Check whether touch is on the lower third of the screen
                        if (touchWorldPosition.y < leadingTouchTopContainer)
                        {
                            leadingTouch = Input.GetTouch(i);
                            checkForLeadingTouch = leadingTouch;
                            break;
                        }
                    }
                }

                if (checkForLeadingTouch != null)
                {
                    foreach (Touch touch in Input.touches)
                    {
                        if (leadingTouch.fingerId == touch.fingerId)
                        {
                            leadingTouch = touch;
                            break;
                        }
                    }

                    if (leadingTouch.phase == TouchPhase.Ended)
                    {
                        checkForLeadingTouch = null;
                    }
                    else
                    {
                        Vector3 leadingTouchWorldPosition = Camera.main.ScreenToWorldPoint(leadingTouch.position);
                        leadingTouchWorldPosition.z = 0f;

                        if (leadingTouchWorldPosition.y > leadingTouchTopContainer)
                        {
                            checkForLeadingTouch = null;
                        }
                    }
                }
            }
            else if (objectType == PlayerType.LEFT)
            {
                //find new leading touch
                if (checkForLeadingTouch == null)
                {
                    for(int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        bool found = false;

                        //if touch.fingerId == oppositeObjectType.leadingTouch.fingerId
                        //Dont do it
                        if (oppositeObjectType.checkForLeadingTouch == null)
                        {
                            found = true;
                        }
                        else if (touch.fingerId != oppositeObjectType.leadingTouch.fingerId)
                        {
                            found = true;
                        }

                        if (found)
                        {
                            Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                            touchWorldPosition.z = 0f;

                            if (touchWorldPosition.y < leadingTouchTopContainer && touchWorldPosition.x < leadingTouchMiddleContainer)
                            {
                                leadingTouch = Input.GetTouch(i);
                                checkForLeadingTouch = leadingTouch;
                                break;
                            }
                        }
                    }
                }

                if (checkForLeadingTouch != null)
                {
                    foreach (Touch touch in Input.touches)
                    {
                        if(leadingTouch.fingerId == touch.fingerId)
                        {
                            leadingTouch = touch;
                            break;
                        }
                    }

                    if(leadingTouch.phase == TouchPhase.Ended)
                    {
                        checkForLeadingTouch = null;
                    }
                    else
                    {
                        Vector3 leadingTouchWorldPosition = Camera.main.ScreenToWorldPoint(leadingTouch.position);
                        leadingTouchWorldPosition.z = 0f;

                        if(leadingTouchWorldPosition.y > leadingTouchTopContainer)
                        {
                            checkForLeadingTouch = null;
                        }
                    }
                }
            }
            else if (objectType == PlayerType.RIGHT)
            {
                //find new leading touch
                if (checkForLeadingTouch == null)
                {
                    for(int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        bool found = false;

                        //if touch.fingerId == oppositeObjectType.leadingTouch.fingerId
                        //Dont do it
                        if (oppositeObjectType.checkForLeadingTouch == null)
                        {
                            found = true;
                        }
                        else if (touch.fingerId != oppositeObjectType.leadingTouch.fingerId)
                        {

                            found = true;
                        }

                        if (found)
                        {
                            Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                            touchWorldPosition.z = 0f;
                            //Check whether touch is on the lower third of the screen and right side
                            if (touchWorldPosition.y < leadingTouchTopContainer && touchWorldPosition.x > leadingTouchMiddleContainer)
                            {
                                leadingTouch = Input.GetTouch(i);
                                checkForLeadingTouch = leadingTouch;
                                break;
                            }
                        }
                    }
                }

                if (checkForLeadingTouch != null)
                {
                    foreach (Touch touch in Input.touches)
                    {
                        if(leadingTouch.fingerId == touch.fingerId)
                        {
                            leadingTouch = touch;
                            break;
                        }
                    }

                    if(leadingTouch.phase == TouchPhase.Ended)
                    {
                        checkForLeadingTouch = null;
                    }
                    else
                    {
                        Vector3 leadingTouchWorldPosition = Camera.main.ScreenToWorldPoint(leadingTouch.position);
                        leadingTouchWorldPosition.z = 0f;

                        if(leadingTouchWorldPosition.y > leadingTouchTopContainer)
                        {
                            checkForLeadingTouch = null;
                        }
                    }
                }
            }
        }
    }
    
    private void Movement()
    {
        if(Input.touchCount > 0)
        {
            if (checkForLeadingTouch != null && !currentlyDashing)
            {
                PlayerMoving();
                Vector3 leadingTouchWorldPosition = Camera.main.ScreenToWorldPoint(leadingTouch.position);
                leadingTouchWorldPosition.y = -9f;
                leadingTouchWorldPosition.z = 0f;
                transform.position = Vector3.MoveTowards(transform.position, leadingTouchWorldPosition, moveSpeed * Time.deltaTime);
            }
        }
        else if (!currentlyDashing)
        {
            PlayerNotMoving();
        }
    }

    private void PlayerMoving()
    {
        currentlyMoving = true;
    }

    private void PlayerNotMoving()
    {
        currentlyMoving = false;
    }

    private void Dash()
    {
        //If (touch is not a tap && dash is off cd && far from the player >= dash distance)
        if (dashTimer != 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                dashTimer = 0f;
            }
        }
        else
        {
            if (checkForLeadingTouch != null && !currentlyDashing)
            {
                Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(leadingTouch.position);
                touchWorldPosition.y = -9f;
                touchWorldPosition.z = 0f;

                if (Vector3.Distance(transform.position, touchWorldPosition) >= dashDistance)
                {
                    Vector3 dashDirection = touchWorldPosition - transform.position;
                    dashGoal = transform.position + (dashDirection.normalized * dashDistance);
                    currentlyDashing = true;
                }
            }

            if (currentlyDashing)
            {
                PlayerMoving();

                transform.position = Vector3.MoveTowards(transform.position, dashGoal, dashSpeed * Time.deltaTime);

                if(Vector3.Distance(transform.position, dashGoal) < 0.01f || hittingWall)
                {
                    dashTimer = dashCooldown;
                    currentlyDashing = false;
                }
            }
        }        
    }

    private void BeatCheck()
    {
        //if (touch is a tap depending on where on the screen)
        if (objectType == PlayerType.DUET)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    BeatTriggerOn();
                }
            }
        }
        else if (objectType == PlayerType.LEFT)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    touchWorldPosition.z = 0f;

                    if (touchWorldPosition.x < leadingTouchMiddleContainer)
                    {
                        BeatTriggerOn();
                    }
                }
            }
        }
        else if (objectType == PlayerType.RIGHT)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    touchWorldPosition.z = 0f;

                    if (touchWorldPosition.x > leadingTouchMiddleContainer)
                    {
                        BeatTriggerOn();
                    }
                }
            }
        }
    }
#endif
}
