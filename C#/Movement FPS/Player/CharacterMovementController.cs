using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementController : Actor
{
    private InputManager inputManager;

    [Header("Limiters")]
    private bool groundSlamMoveLockout = false;

    [Header("Movement")]
    public float runSpeed = 20f;
    public bool walking { get { return _walking; } }
    [SerializeField] private bool _walking = false;
    public float walkSpeed = 10f;
    public bool sliding { get { return _sliding; } }
    [SerializeField] private bool _sliding = false;
    public float slideSpeed = 25f;
    public float airMoveForce = 75f;
    public float jumpHeight = 10f;
    public Vector3 drag = new Vector3(100, 0, 100);
    public Vector3 airDrag = new Vector3(1, 0, 1);
    
    public int maxJumps = 1;

    [Header("Walls")]
    public float wallGrabVelocity = 0f;
    public float wallPushPower = 10f;
    private bool wallGrab = false;
    public float wallGrabSlowPower = 20f;

    [Header("Buffers")]
    public float jumpBufferTime = 0.15f;
    [SerializeField] private bool _jumpBuffer = false;
    private float _jumpBufferTimer = 0f;

    //Also known as coyote time
    public float wallEdgeJumpBufferTime = 0.15f;
    [SerializeField] private bool _wallEdgeJumpBuffer = false;
    private float _wallEdgeJumpBufferTimer = 0f;

    public float edgeJumpBufferTime = 0.15f;
    [SerializeField] private bool _edgeJumpBuffer = false;
    private float _edgeJumpBufferTimer = 0f;
    
    //Private
    [SerializeField] private int _currentJumps = 0;

    new void Start()
    {
        base.Start();
        inputManager = GameObject.FindObjectOfType<InputManager>();
    }

    private void OnEnable()
    {
        inputManager.inputActions.Movement.Slide.started += _ => SlideStart();
        inputManager.inputActions.Movement.Slide.canceled += _ => SlideEnd();
    }

    private void OnDisable()
    {
        inputManager.inputActions.Movement.Slide.started -= _ => SlideStart();
        inputManager.inputActions.Movement.Slide.canceled -= _ => SlideEnd();
    }

    void Update()
    {
        #region Resets

        _sliding = false;
        _walking = false;

        #endregion

        #region Checks

        CollisionChecks();

        if (wallGrab)
            _onWall = true;

        #endregion

        #region Player Input
        if (_actorStates.currentActorState == ActorStates.ActiveState.Active)
        {
            if (inputManager.inputActions.Movement.Jump.triggered)
            {
                _jumpBuffer = true;
                _jumpBufferTimer = 0f;
            }

            if (inputManager.inputActions.Movement.Walk.triggered)
            {
                _walking = true;
            }
            
            //Stick to wall
            if (inputManager.inputActions.Movement.Slide.ReadValue<float>() != 0)
            {
                if (_onGround)
                    _sliding = true;

                if (_onWall && !_onGround && !walking)
                    wallGrab = true;
            }

            Vector2 movement = inputManager.inputActions.Movement.Movement.ReadValue<Vector2>();
            _moveDirection = _actorStates.canMove && !groundSlamMoveLockout ? movement.x * transform.right + movement.y * transform.forward : Vector3.zero;
            if (wallGrab)
                _moveDirection = Vector3.zero;

            float movingSpeed = runSpeed;
            
            if (_onGround || _onWall)
            {
                _currentJumps = maxJumps;
            }

            if (_onGround)
            {
                if (_sliding)
                    movingSpeed = slideSpeed;
                else if (_walking)
                    movingSpeed = walkSpeed;

                _edgeJumpBuffer = true;
                _edgeJumpBufferTimer = 0f;

                _innerVelocity.x = _moveDirection.x * movingSpeed;
                _innerVelocity.z = _moveDirection.z * movingSpeed;

                groundSlamMoveLockout = false;
            }
            //Air Movement
            else
            {
                _innerVelocity.x = Mathf.MoveTowards(_innerVelocity.x, _moveDirection.x * movingSpeed, airMoveForce * Time.deltaTime);
                _innerVelocity.z = Mathf.MoveTowards(_innerVelocity.z, _moveDirection.z * movingSpeed, airMoveForce * Time.deltaTime);
            }

            if (_onWall)
            {
                _wallEdgeJumpBuffer = true;
                _wallEdgeJumpBufferTimer = 0f;
            }

        }
        #endregion

        if (_edgeJumpBuffer)
        {
            _edgeJumpBufferTimer += Time.deltaTime;
            if (_edgeJumpBufferTimer >= edgeJumpBufferTime)
                _edgeJumpBuffer = false;
        }
        if (_wallEdgeJumpBuffer)
        {
            _wallEdgeJumpBufferTimer += Time.deltaTime;
            if (_wallEdgeJumpBufferTimer >= wallEdgeJumpBufferTime)
                _wallEdgeJumpBuffer = false;
        }

        if (_jumpBuffer)
        {
            _jumpBufferTimer += Time.deltaTime;
            if (_currentJumps > 0 && _jumpBufferTimer <= jumpBufferTime)
            {
                if (_onGround || _edgeJumpBuffer)
                {
                    ClearJumpBuffers();
                    Jump();
                }
                else if (_onWall || _wallEdgeJumpBuffer)
                {
                    ClearJumpBuffers();
                    WallJump();
                }
            }
            
        }

        _jumping = _innerVelocity.y > 0;
        _falling = _innerVelocity.y < 0 && !_onGround;

        _innerVelocity.y += FPSPhysics.Gravity * Time.deltaTime;
        float velocityCap = _onWall ? wallGrabVelocity : terminalVelocity;
        _innerVelocity.y = Mathf.Clamp(_innerVelocity.y, -velocityCap, terminalVelocity);

        if (_onGround)
        {
            _innerVelocity.x /= 1 + drag.x * Time.deltaTime;
            _innerVelocity.y /= 1 + drag.y * Time.deltaTime;
            _innerVelocity.z /= 1 + drag.z * Time.deltaTime;
        }
        else
        {
            _innerVelocity.x /= 1 + airDrag.x * Time.deltaTime;
            _innerVelocity.y /= 1 + airDrag.y * Time.deltaTime;
            _innerVelocity.z /= 1 + airDrag.z * Time.deltaTime;
        }

        if (wallGrab)
        {
            _innerVelocity = Vector3.MoveTowards(_innerVelocity, Vector3.zero, wallGrabSlowPower * Time.deltaTime);
        }

        if (_onGround && _innerVelocity.y < 0)
            _innerVelocity.y = 0f;

        Move(_innerVelocity * Time.deltaTime);

        Gravity = FPSPhysics.Gravity;
    }

    private void SlideStart()
    {
        if (!_onGround && !_onWall)
        {
            // Ground Pound
            groundSlamMoveLockout = true;
            _innerVelocity.y = -terminalVelocity;
        }  
    }

    private void SlideEnd()
    {
        wallGrab = false;
    }

    private void ClearJumpBuffers()
    {
        _jumpBuffer = false;
        _edgeJumpBuffer = false;
        _wallEdgeJumpBuffer = false;
    }

    private void Jump()
    {
        _innerVelocity.y += Mathf.Sqrt(jumpHeight * -2f * FPSPhysics.Gravity);
        _jumping = true;
    }

    private void WallJump()
    {
        //push off wall
        wallGrab = false;
        _innerVelocity += _wallNormal * wallPushPower;
        _innerVelocity.y += Mathf.Sqrt(jumpHeight * -2f * FPSPhysics.Gravity);
        _jumping = true;
    }
}
