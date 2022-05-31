using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]

public class CameraController : MonoBehaviour
{
    private ActorStates _actorStates;
    private CharacterMovementController _moveController;
    [Header("")]
    public CharacterWeaponController weaponController;
    private InputManager inputManager;

    private bool _startDelay = true;
    public bool active = true;

    public float cameraSpeed = 100f;
    
    [Header("Rotation")]
    //camera public values
    public float XMinRotation = -60f;
    public float XMaxRotation = 60f;
    [Range(0.01f, 10.0f)]
    public float Xsensitivity = 1f;
    [Range(0.01f, 10.0f)]
    public float Ysensitivity = 1f;
    public const float MOUSE_DELTA_MULTIPLIER = 0.1f;
    private Camera cam;
    private float rotAroundX, rotAroundY, rotAroundZ;

    [Header("Movement Tilt")]
    public float movementTiltAmount = 3f;
    public float tiltSpeed = 6f;

    [Header("")]
    public float slidingCameraLowerDistance = 1f;

    private Vector3 _startLocalPosition;
    private Vector3 _currentPosition;

    // Use this for initialization
    void Start()
    { 
        _actorStates = GetComponentInParent<ActorStates>();
        _moveController = GetComponentInParent<CharacterMovementController>();
        inputManager = GameObject.FindObjectOfType<InputManager>();
        cam = GetComponent<Camera>();
        rotAroundX = transform.eulerAngles.x;
        rotAroundY = transform.eulerAngles.y;
        rotAroundZ = 0;
        _startLocalPosition = transform.localPosition;
        _currentPosition = transform.position;
        StartCoroutine(StartDelay());
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _startDelay = false;
    }

    private void LateUpdate()
    {
        if (active && CursorController.cursorLocked)
        {
            if (!_startDelay)
            {
                float x = inputManager.inputActions.Movement.MouseX.ReadValue<float>();
                float y = inputManager.inputActions.Movement.MouseY.ReadValue<float>();
                //Debug.Log("x = " + x + " | y = " + y);
                rotAroundX += y * MOUSE_DELTA_MULTIPLIER * Xsensitivity;
                rotAroundY += x * MOUSE_DELTA_MULTIPLIER * Ysensitivity;

                // Clamp rotation values
                rotAroundX = Mathf.Clamp(rotAroundX, XMinRotation, XMaxRotation);

                // Tilt - Half if moving forward/back
                float tiltGoal = 0;
                if (_actorStates.canMove)
                {
                    tiltGoal = Input.GetAxis("Horizontal") * movementTiltAmount * (Input.GetAxis("Vertical") == 0f ? 1f : 0.5f);
                }
                rotAroundZ = Mathf.MoveTowards(rotAroundZ, tiltGoal, tiltSpeed * Time.deltaTime);

                CameraRotation();
            }

            Vector3 goal = transform.parent.position + _startLocalPosition;
            if (_moveController.sliding)
            {
                goal.y -= slidingCameraLowerDistance;
            }

            _currentPosition = Vector3.MoveTowards(_currentPosition, goal, cameraSpeed * Time.deltaTime);

            transform.position = goal;

            weaponController.Move();
        }
    }

    private void CameraRotation()
    {
        transform.parent.rotation = Quaternion.Euler(0, rotAroundY, 0);
        cam.transform.rotation = Quaternion.Euler(-rotAroundX, rotAroundY, rotAroundZ);
    }
}
