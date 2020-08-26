using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : Rider
{
    InputManager inputManager;
    RaycastCollider raycastCollider;
    PlayerWallDeath wallDeath;
    PlayerDustSpawner dustSpawner;

    public enum PlayerState { Control, EnteringRoom, Cutscene, CutsceneButCanJump }
    public PlayerState currentPlayerState = PlayerState.Control;
    [HideInInspector] public bool isJumpSwitchActive = true;

    [Header("")]
    //Use these to clamp player velocities but when outside forces act they can break them
    public float maxYNegativeVelocity = -24f;
    public float maxYPositiveVelocity = 60f;
    public float maxWallYNegativeVelocity = -9.5f;

    [Tooltip("The angle of ground which is still considered ground (1 is straight).")]
    public float minGroundNormalY = 0.65f;

    private const float Gravity = -75.537f;
    public float gravityModifier = 1f;

    [Header("")]
    public float maxMoveSpeed = 14f;
    public float maxJumpPower = 29.1f;
    [Tooltip("Percentage cut when jump button is released while jumping.")]
    public float releasedJumpCut = 0.4f;
    public float maxWallJumpXPower = 22f;
    public float maxWallJumpYPower = 25.5f;

    private float moveSpeed = 0f;
    private float jumpPower = 0f;
    private float wallJumpXPower = 0f;
    private float wallJumpYPower = 0f;

    public int currentJumps = 0;
    public int maxJumps = 1;
    public int extraJumps = 0;

    private bool onGround = false;
    public bool OnGround { get { return onGround; } }

    private bool nearGround = false;
    public float nearGroundRayLength = 0.75f;

    private int onWall = 0;
    public int OnWall { get { return onWall; } }

    private bool falling = false;
    private bool jumping = false;
    public bool Jumping { get { return jumping; } }
    private bool groundJumping = false;
    public bool GroundJumping { get { return groundJumping; } }
    private bool edgeJumping = false;
    public bool EdgeJumping { get { return edgeJumping; } }
    private bool wallJumping = false;
    public bool WallJumping { get { return wallJumping; } }
    private bool wallEdgeJumping = false;
    public bool WallEdgeJumping { get { return wallEdgeJumping; } }
    private bool fastFalling = false;
    public bool FastFalling { get { return fastFalling; } }
    private bool holdingDown = false;
    public bool HoldingDown { get { return holdingDown; } }

    [Tooltip("Not affected by inertia.")]
    private float constantXVelocity = 0f;
    [Tooltip("Affected by inertia.")]
    private float wallJumpXVelocity = 0f;
    [Tooltip("Affected by inertia.")]
    private float outsideXVelocity = 0f;

    private const float XInertia = 50f;
    [Tooltip("A Percentage")]
    private const float MovementAffectOnInertia = 0.31f;
    [Tooltip("A Percentage")]
    private const float WallJumpFightingMovementAffectOnInertia = 0.10f;

    [Tooltip("Keeps track of velocity that can be cut by letting go of space.")]
    private float variableJumpYVelocity = 0f;

    public Vector2 velocity;

    private Vector2 groundNormal = new Vector2(0, 1.0f);

    private SpriteRenderer mainSprite;
    private Animator mainAnimator;
    private SpriteRenderer hairSprite;
    private Animator hairAnimator;

    private bool pressedJumpBuffer = false;
    private float pressedJumpBufferTimer = 0f;
    [Header("Jump Buffers")]
    public float pressedJumpBufferMaxTimer = 0.1f;

    private bool jumpLockout = false;
    private float jumpLockoutTimer = 0f;
    [Tooltip("Activated whenever you jump to remove the chance of jumping twice. Must be longer than jump buffers")]
    public float jumpLockoutMaxTimer = 0.15f;

    private bool edgeJumpBuffer = false;
    public bool EdgeJumpBuffer { get { return edgeJumpBuffer; } }
    private float edgeJumpBufferTimer = 0f;
    public float edgeJumpBufferMaxTimer = 0.083f;

    private bool wallJumpBuffer = false;
    public bool WallJumpBuffer { get { return wallJumpBuffer; } }
    private float wallJumpBufferTimer = 0f;
    public float wallJumpBufferMaxTimer = 0.13f;

    private bool wallGrabPause = false;
    private float wallGrabPauseTimer = 0f;
    public float wallGrabPauseMaxTimer = 0.055f;

    private float wallGrabPauseMaxNegativeYVelocity = -15f;

    private float wallJumpDirection = 0f;
    public float WallJumpDirection { get { return wallJumpDirection; } }

    [HideInInspector] public Transform prevCheckpoint;
    private GameObject playerManager;

    [HideInInspector] public bool onMutedGround = false;
    [Header("")]
    public float mutedGroundModifier = 0.5f;
    [HideInInspector] public bool jsPaused = false;

    private const float EnteringRoomLockoutTime = 0.5f;

    private bool animFalling = false;
    private float animFallingTimer = 0f;
    private float animMaxFallingTimer = 0.1f;

#region Cutscene Variables;
    //Cutscene
    private bool cutsceneMovingToXPos = false;
    private float cutsceneXDestination = 0f;
    private float cutsceneMoveDirection = 0f;
    private bool cutsceneFacingLeftOnArrival = false;
    private float cutsceneMoveSpeed = 0f;

    private bool cutsceneTethered = false;
    private float cutsceneTetherXPos = 0f;
    private float cutsceneTetherDistance = 0f;
    
    private float cutsceneTetherForce = 0f;
    [Header("Cutscene Variables")]
    public float cutsceneTetherForcePerDistance = 6f;

#endregion

    private new void OnEnable()
    {
        base.OnEnable();
        boxCollider = transform.Find("WallCollider").GetComponent<BoxCollider2D>();

        inputManager = GameObject.FindWithTag("GameManager").GetComponent<InputManager>();

        mainSprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        mainAnimator = transform.Find("Sprite").GetComponent<Animator>();
        hairSprite = mainSprite.transform.GetChild(0).GetComponent<SpriteRenderer>();
        hairAnimator = mainAnimator.transform.GetChild(0).GetComponent<Animator>();

        raycastCollider = GetComponent<RaycastCollider>();
        raycastCollider.boxCollider = boxCollider;
        raycastCollider.inWallCollider = transform.Find("InWallCollider").GetComponent<BoxCollider2D>();
        raycastCollider.spriteRenderer = mainSprite;
        raycastCollider.Setup();
        raycastCollider.onSqueeze += WallDeath;

        wallDeath = GetComponentInChildren<PlayerWallDeath>();
        dustSpawner = GetComponentInChildren<PlayerDustSpawner>();

        playerManager = GameObject.FindWithTag("PlayerManager");
        isJumpSwitchActive = playerManager.GetComponent<PlayerManager>().isJumpSwitchActive;

        if (currentPlayerState == PlayerState.EnteringRoom)
        {
            StartCoroutine(CountdownTillControl());
        }
    }

    private new void OnDisable()
    {
        base.OnDisable();

    }

    IEnumerator CountdownTillControl()
    {
        yield return new WaitForSeconds(EnteringRoomLockoutTime);
        currentPlayerState = PlayerState.Control;
    }

    void Update()
    {
        constantXVelocity = 0f;

        //For outside sources (dust, animators) to see how the player jumps
        jumping = false;
        groundJumping = false;
        edgeJumping = false;
        wallJumping = false;
        wallEdgeJumping = false;

        onGround = raycastCollider.onGround;
        nearGround = raycastCollider.nearGround;
        onWall = raycastCollider.onWall;
        SetMoveJumpPower();

        float gravity = gravityModifier * Gravity * Time.deltaTime;

        variableJumpYVelocity += gravity;
        variableJumpYVelocity = Mathf.Clamp(variableJumpYVelocity, 0, velocity.y);

        if (currentPlayerState == PlayerState.Control || currentPlayerState == PlayerState.CutsceneButCanJump)
        {
            InputVelocity();
        }
        else if (currentPlayerState == PlayerState.Cutscene || currentPlayerState == PlayerState.CutsceneButCanJump)
        {
            if (cutsceneMovingToXPos)
            {
                CutsceneMovingToXPosition();
            } 
            else
            {
                CutsceneMove();
            }
        }

        if (currentPlayerState == PlayerState.CutsceneButCanJump)
        {
            constantXVelocity = 0f;
        }

        velocity.x = 0f;

        if (outsideXVelocity != 0)
        {
            ComputeXInteria(ref outsideXVelocity);
        }
        if (wallJumpXVelocity != 0)
        {
            ComputeXInteria(ref wallJumpXVelocity, WallJumpFightingMovementAffectOnInertia);
        }

        float xVelocities = outsideXVelocity + wallJumpXVelocity;

        //if (Mathf.Abs(xVelocities) > Mathf.Abs(constantXVelocity))
        if (((xVelocities > 0) && (xVelocities > constantXVelocity)) || ((xVelocities < 0) && (xVelocities < constantXVelocity)))
        {
            //Debug.Log("XVelocities in control");
            velocity.x = xVelocities;
        }
        else
        {
            //Debug.Log("Constant in control");
            velocity.x = constantXVelocity;
        }

        velocity.y += gravity;

        WallGrabPause(gravity);

        if (onWall != 0 && !HoldingDown)
        {
            velocity.y = Mathf.Clamp(velocity.y, maxWallYNegativeVelocity, maxYPositiveVelocity);
        }
        else
        {
            velocity.y = Mathf.Clamp(velocity.y, maxYNegativeVelocity, maxYPositiveVelocity);
        }

        Riding(null);
        velocity = raycastCollider.Move(velocity);

        //probably wrong spot for this
        wallDeath.CollisionCheck();

        falling = velocity.y < 0 ? true : false;
        fastFalling = velocity.y < maxYNegativeVelocity + 0.1f ? true : false;
        SendAnimation();
    }

    private void FixedUpdate()
    {
        if (cutsceneTethered)
        {
            CutsceneTetheredToX();
        }

        onMutedGround = false;
    }

    public void Velocity(float x, float y)
    {
        velocity = new Vector2(x, y);
    }

    public Vector2 Velocity()
    {
        return velocity;
    }

    private void SetMoveJumpPower()
    {
        moveSpeed = maxMoveSpeed;
        jumpPower = maxJumpPower;
        wallJumpXPower = maxWallJumpXPower;
        wallJumpYPower = maxWallJumpYPower;

        if (onMutedGround)
        {
            moveSpeed *= mutedGroundModifier;
            jumpPower *= (mutedGroundModifier * 1.5f);
            wallJumpXPower *= mutedGroundModifier;
            wallJumpYPower *= (mutedGroundModifier * 1.5f);
        }
    }

    private void InputVelocity()
    {
        float move = 0f;
        holdingDown = false;

        if (InputManager.horizontalInput > 0)
        {
            move = 1;
        }
        else if (InputManager.horizontalInput < 0)
        {
            move = -1;
        }

        if (Input.GetAxis("Vertical") < 0)
        {
            Debug.Log("Holding Down");
            holdingDown = true;
        }
        //else if (InputManager.horizontalInput > 0)
        //{
        //    //look Up
        //}

        #region Jump Code

        if (inputManager.inputActions.Player.JumpPressed.triggered)
        {
            pressedJumpBuffer = true;
            pressedJumpBufferTimer = 0f;
        }
        else if (InputManager.jumpInput == 0)
        {
            //Once you let go of jump cut your velocity only for certain jumps
            if (variableJumpYVelocity > 0)
            {
                velocity.y -= variableJumpYVelocity;
                variableJumpYVelocity *= releasedJumpCut;
                velocity.y += variableJumpYVelocity;

                variableJumpYVelocity = 0f;
            }
        }

        if (currentJumps < 0)
        {
            currentJumps = 0;
        }

        if (onGround)
        {
            edgeJumpBuffer = true;
            edgeJumpBufferTimer = 0f;

            currentJumps = maxJumps;
            extraJumps = 0;
        }
        else if (onWall != 0)
        {
            if (!nearGround)
            {
                wallJumpBuffer = true;
                wallJumpBufferTimer = 0f;
                wallJumpDirection = -onWall;

                currentJumps = maxJumps;
            }
            else
            {
                wallJumpBuffer = false;
                wallJumpBufferTimer = 0f;

                currentJumps = 0;
            }
        }

        //Buffer for pressing jumping before landing
        if (pressedJumpBuffer && !nearGround)
        {
            pressedJumpBufferTimer += Time.deltaTime;
            if (pressedJumpBufferTimer >= pressedJumpBufferMaxTimer)
            {
                pressedJumpBuffer = false;
                pressedJumpBufferTimer = 0f;
            }
        }

        //Lockout once you jump to remove chances of "double jumping" 
        //and JS'ing twice within a small amount of frames
        if (jumpLockout)
        {
            jumpLockoutTimer += Time.deltaTime;
            if (jumpLockoutTimer >= jumpLockoutMaxTimer)
            {
                jumpLockout = false;
                jumpLockoutTimer = 0f;
            }
        }

        //Buffer for jumping when leaving the ground
        if (edgeJumpBuffer)
        {
            edgeJumpBufferTimer += Time.deltaTime;
            //if elapsed timer, lose jumps
            if (edgeJumpBufferTimer >= edgeJumpBufferMaxTimer)
            {
                edgeJumpBuffer = false;
                edgeJumpBufferTimer = 0f;
                currentJumps = 0;
            }
        }

        //Buffer for jumping when leaving a wall
        if (wallJumpBuffer)
        {
            wallJumpBufferTimer += Time.deltaTime;
            //if elapsed timer, lose jumps
            if (wallJumpBufferTimer >= wallJumpBufferMaxTimer || holdingDown)
            {
                wallJumpBuffer = false;
                wallJumpBufferTimer = 0f;
                currentJumps = 0;
            }
        }

        if (wallJumpBuffer && pressedJumpBuffer && currentJumps > 0 && !jumpLockout)
        {
            WallJump();
        }
        else if (pressedJumpBuffer && currentJumps > 0 && !jumpLockout)
        {
            Jump();
        }
        else if (pressedJumpBuffer && extraJumps > 0)
        {
            ExtraJump();
        }

#endregion

        constantXVelocity = move * moveSpeed;
    }

    private void WallJump()
    {
        jumping = true;

        jumpLockout = true;
        jumpLockoutTimer = 0f;

        if (onWall != 0)
        {
            wallJumping = true;
        }
        else
        {
            wallEdgeJumping = true;
        }

        currentJumps--;
        velocity.y = wallJumpYPower;
        variableJumpYVelocity = wallJumpYPower;

        wallJumpXVelocity = wallJumpXPower * wallJumpDirection;

        ResetBuffers();

        dustSpawner.SpawnWallJumpDust();

        if (isJumpSwitchActive)
        {
            playerManager.GetComponent<JumpSwitchTileset>().JumpSwitch();
        }
    }

    private void Jump()
    {
        jumping = true;

        jumpLockout = true;
        jumpLockoutTimer = 0f;

        if (onGround)
        {
            groundJumping = true;
        }
        else
        {
            edgeJumping = true;
        }

        currentJumps--;
        velocity.y = jumpPower;
        variableJumpYVelocity = jumpPower;
        ResetBuffers();

        dustSpawner.SpawnJumpDust();

        if (isJumpSwitchActive)
        {
            playerManager.GetComponent<JumpSwitchTileset>().JumpSwitch();
        }
    }

    private void ExtraJump()
    {
        jumping = true;
        GetComponentInChildren<ExtraJumpFlair>().PlayerJumped();
        extraJumps--;

        //for the rare case that I bypass a regular jump through the lockout 
        //and go into an extrajump reseting buffers and never removing the current jumps
        currentJumps = 0;

        velocity.y = jumpPower;
        variableJumpYVelocity = jumpPower;

        ResetBuffers();

        if (isJumpSwitchActive)
        {
            playerManager.GetComponent<JumpSwitchTileset>().JumpSwitch();
        }
    }

    private void ResetBuffers()
    {
        pressedJumpBuffer = false;
        pressedJumpBufferTimer = 0f;
        edgeJumpBuffer = false;
        edgeJumpBufferTimer = 0f;
        wallJumpBuffer = false;
        wallJumpBufferTimer = 0f;

        onGround = false;
        nearGround = false;
        onWall = 0;
    }

    private void ComputeXInteria(ref float forceVelocity, float fightingPower = MovementAffectOnInertia, float supportivePower = MovementAffectOnInertia)
    {
        // constantXVelocity is the velocity for walking left and right
        float inertia = XInertia * Time.deltaTime;

        fightingPower = 1 + fightingPower;
        supportivePower = 1 - supportivePower;
        float power = 1;

        //If Velocity is greater than 0 - Moving Right
        if (forceVelocity > 0f)
        {
            if (constantXVelocity != 0)
            {
                //Holding Right - Extend/supportivePower
                //Holding Left - Shorten/fightingPower
                power = constantXVelocity > 0 ? supportivePower : fightingPower;
            }

            forceVelocity -= inertia * power;

            //If it overshoots 0 set to 0
            if (forceVelocity < 0f)
            {
                forceVelocity = 0f;
            }
        }
        else //Velocity is less than 0 - Moving Left
        {
            if (constantXVelocity != 0)
            {
                //Holding Right - Extend/supportivePower
                //Holding Left - Shorten/fightingPower
                power = constantXVelocity > 0 ? fightingPower : supportivePower;
            }

            forceVelocity += inertia * power;

            //If it overshoots 0 set to 0
            if (forceVelocity > 0f)
            {
                forceVelocity = 0f;
            }
        }
    }

    private void FlipSprite(float xVel)
    {
        bool flipSprite;

        if (onWall != 0)
        {
            flipSprite = (mainSprite.flipX ? (onWall > 0) : (onWall < 0));

            if (holdingDown && velocity.y < 0)
            {
                flipSprite = (mainSprite.flipX ? (onWall < 0) : (onWall > 0));
            }
        }
        else
        {
            flipSprite = (mainSprite.flipX ? (xVel > 0.01f) : (xVel < -0.01f));
        }

        if (flipSprite)
        {
            mainSprite.flipX = !mainSprite.flipX;
            hairSprite.flipX = !hairSprite.flipX;
        }
    }

    private void SendAnimation()
    {
        //Insert total X velocity
        FlipSprite(constantXVelocity + wallJumpXVelocity);

        if (velocity.y < 0)
        {
            animFallingTimer += Time.deltaTime;
            if (animFallingTimer >= animMaxFallingTimer)
            {
                animFalling = true;
            }
        }
        else
        {
            animFalling = false;
            animFallingTimer = 0f;
        }

        if (holdingDown)
        {
            mainAnimator.SetBool("onWall", false);
            hairAnimator.SetBool("onWall", false);
        }
        else
        {
            mainAnimator.SetBool("onWall", (onWall != 0));
            hairAnimator.SetBool("onWall", (onWall != 0));
        }

        mainAnimator.SetBool("onGround", onGround);
        mainAnimator.SetBool("falling", animFalling);
        mainAnimator.SetFloat("velocityX", Mathf.Abs(velocity.x));
        mainAnimator.SetFloat("velocityY", Mathf.Abs(velocity.y));
        mainAnimator.SetFloat("runningSpeed", Mathf.Abs(constantXVelocity / maxMoveSpeed));

        hairAnimator.SetBool("onGround", onGround);
        hairAnimator.SetBool("falling", animFalling);
        hairAnimator.SetFloat("velocityX", Mathf.Abs(velocity.x));
        hairAnimator.SetFloat("velocityY", Mathf.Abs(velocity.y));
        hairAnimator.SetFloat("runningSpeed", Mathf.Abs(constantXVelocity / maxMoveSpeed));
    }

    private void WallGrabPause(float gravity)
    {
        if (onGround || onWall == 0)
        {
            if (velocity.y > wallGrabPauseMaxNegativeYVelocity)
            {
                wallGrabPause = true;
                wallGrabPauseTimer = 0f;
            }
            else
            {
                wallGrabPause = false;
            }
        }
        else if (onWall != 0)
        {
            //While sliding up a wall get ready to stick at the peak
            if (velocity.y > 0)
            {
                wallGrabPause = true;
                wallGrabPauseTimer = 0f;
            }

            if (HoldingDown)
            {
                wallGrabPause = false;
            }

            if (wallGrabPause && velocity.y < 0)
            {
                wallGrabPauseTimer += Time.deltaTime;
                if (wallGrabPauseTimer >= wallGrabPauseMaxTimer)
                {
                    wallGrabPause = false;
                }
                else
                {
                    velocity.y = gravity;
                }
            }
        }
    }

    public void Death(PlayerDeath.DeathType deathType)
    {
        playerManager.GetComponent<PlayerManager>().ChangeToDeath(transform.position, mainSprite.flipX, prevCheckpoint, deathType);
    }

    private void WallDeath()
    {
        Death(PlayerDeath.DeathType.Normal);
    }

    #region Outside Velocity

    public void OutsideVelocity(Vector2 outsideVelocity, bool cancleWallJump = true)
    {
        jumpLockout = true;
        jumpLockoutTimer = 0f;

        variableJumpYVelocity = 0f;

        if (cancleWallJump)
        {
            wallJumpXVelocity = 0f;
        }

        outsideXVelocity = outsideVelocity.x;
        velocity.y += outsideVelocity.y;
    }

    public override void EndRide(Vector2 _velocity)
    {
        OutsideVelocity(_velocity, false);
        savedVelocity = Vector2.zero;
        Debug.Log("Ride ended, adding " + _velocity);
    }

    #endregion

    #region Cutscene Functions
    private void CutsceneMove()
    {
        constantXVelocity = cutsceneMoveDirection * moveSpeed;
    }

    public void CutsceneSetMoveDirection(float direction)
    {
        cutsceneMoveDirection = direction;
    }

    public void CutsceneSetFacingDirection(bool faceLeft = true)
    {
        mainSprite.flipX = faceLeft;
        hairSprite.flipX = faceLeft;
    }

    public void CutsceneMoveToXPosition(float xPos, bool facingLeftOnArrival = false, float speed = 0)
    {
        if (Mathf.Abs(transform.position.x - xPos) < 0.5f)
        {
            return;
        }

        cutsceneMovingToXPos = true;
        cutsceneXDestination = xPos;
        cutsceneFacingLeftOnArrival = facingLeftOnArrival;

        cutsceneMoveSpeed = speed == 0 ? moveSpeed : speed;

        //Destination left of player
        if (xPos - transform.position.x < 0)
        {
            cutsceneMoveDirection = -1;
        }
        //right of
        else
        {
            cutsceneMoveDirection = 1;
        }
    }

    private void CutsceneMovingToXPosition()
    {
        constantXVelocity = cutsceneMoveDirection * cutsceneMoveSpeed;
        bool leftOfDestination = (cutsceneXDestination - transform.position.x < 0);
        //If dest was left and now right OR dest was right, end
        if ((cutsceneMoveDirection == -1 && !leftOfDestination) || (cutsceneMoveDirection == 1 && leftOfDestination))
        {
            constantXVelocity = 0;
            mainSprite.flipX = cutsceneFacingLeftOnArrival;
            hairSprite.flipX = cutsceneFacingLeftOnArrival;

            cutsceneMovingToXPos = false;
            cutsceneMoveDirection = 0;
        }
    }

    public void CutsceneJump()
    {
        if (currentPlayerState == PlayerState.Cutscene)
            Jump();
    }

    public void CutsceneWallJump()
    {
        if (currentPlayerState == PlayerState.Cutscene)
            WallJump();
    }

    public void CutsceneTetherToX(float xPos, float distance)
    {
        cutsceneTethered = true;
        cutsceneTetherXPos = xPos;
        cutsceneTetherDistance = distance;
        cutsceneTetherForce = distance * cutsceneTetherForcePerDistance;
    }

    public void CutsceneUnTether()
    {
        cutsceneTethered = false;
        cutsceneTetherXPos = 0f;
        cutsceneTetherDistance = 0f;
    }

    private void CutsceneTetheredToX()
    {
        //Apply force in direction depending if the player past the distance set to xPos

        float farthestRightX = cutsceneTetherXPos + cutsceneTetherDistance;
        float farthestLeftX = cutsceneTetherXPos - cutsceneTetherDistance;

        bool pastBoundary = false;
        float force = 0;

        //Check whether the player has gone past barriers
        if (transform.position.x > farthestRightX)
        {
            pastBoundary = true;
            force = -cutsceneTetherForce;
        }
        else if (transform.position.x < farthestLeftX)
        {
            pastBoundary = true;
            force = cutsceneTetherForce;
        }

        if (pastBoundary)
        {
            //yanks you back into place
            OutsideVelocity(new Vector2(force, 0));
        }
    }


#endregion
}
