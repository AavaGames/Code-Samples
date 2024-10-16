using Assets.App.Scripts.Attributes;
using Assets.App.Scripts.Characters.Pawn;
using Assets.App.Scripts.Combat;
using Assets.App.Scripts.Extensions;
using Assets.App.Scripts.Structs;
using Assets.App.Scripts.UI;
using Assets.App.Scripts.Utilities;
using Cinemachine;
using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.App.Scripts.Characters
{
    public class CharacterCamera : NetworkBehaviour, ICharacter
    {
        public Camera MainCamera { get; private set; }
        private CharacterPawn _pawn;
        private CharacterTargeter _targeter;

        private Transform _cameraParent;
        [Required]
        [Dependencies]
        public GameObject CameraFollow;
        [Required]
        [Dependencies]
        public GameObject CameraTargetFollow;
        [Required]
        [Dependencies]
        public GameObject CameraTargetLookAtPrefab;
        [Required]
        [Dependencies]
        public GameObject FollowCameraPrefab;
        [Required]
        [Dependencies]
        public GameObject TargetCameraPrefab;

        [Header("Cameras")]
        public int CameraPriority = 10;

        public FloatRange CameraClamp = new FloatRange(-30f, 70f);
        [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;
        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        private CinemachineBrain _cinemachineBrain;
        private CinemachineVirtualCamera _followCamera;
        private CinemachineVirtualCamera[] _targetingCameras;
        private GameObject[] _targetFollows;
        private SimplePositionFollow[] _targetLookAt;

        private int _targetingCameraIndex = 0;
        private const int TARGETING_CAMERAS = 2;

        private CinemachineVirtualCamera _activeCamera;
        public CinemachineVirtualCamera ActiveCamera => _activeCamera;

        [SerializeField]
        [HorizontalGroup("Camera Goal", 0.5f)]
        [LabelText("Pitch")]
        private float _cameraGoalPitch = 0;
        [SerializeField]
        [HorizontalGroup("Camera Goal", 0.5f)]
        [LabelText("Yaw")]
        private float _cameraGoalYaw = 0;


        [SerializeField]
        private bool _targetFollowLockout = false;
        [SerializeField]
        private float _targetFollowLockoutTime = 0.1f;
        private Coroutine _targetFollowLockoutCoroutine;

        /// <summary>
        /// The threshold the camera must move before it changes from Target cam to Follow cam
        /// NOTE: this is not actually how it works, this is the threshold 
        /// that must be constantly passed to stay follow cam
        /// </summary>
        private const float LOOK_THRESHOLD = 0.01f;
        // TODO: create initial look threshold and (timer when letting go?)

        private Action<InputAction.CallbackContext> _resetCameraAction;

        [Tooltip("Called after camera has updated. bool isTargeting")]
        public Action<bool> OnCameraPostUpdate;

        private void Awake()
        {
            _pawn = GetComponent<CharacterPawn>();
            _targeter = GetComponent<CharacterTargeter>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                MainCamera = Camera.main;
                _cinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();

                // Setup Camera, LookAt & Follow objects

                // Set parent to Character, which does not move
                _cameraParent = new GameObject("Camera").transform;
                _cameraParent.transform.SetParent(_pawn.transform.parent, false);

                CameraFollow.transform.SetParent(_cameraParent, false);
                // Camera names are important for cinemachine blender settings
                _followCamera = Instantiate(FollowCameraPrefab, _cameraParent).GetComponent<CinemachineVirtualCamera>();
                _followCamera.name = FollowCameraPrefab.name;
                _followCamera.Follow = CameraFollow.transform;

                _targetingCameras = new CinemachineVirtualCamera[TARGETING_CAMERAS];
                _targetFollows = new GameObject[TARGETING_CAMERAS];
                _targetLookAt = new SimplePositionFollow[2];

                CameraTargetFollow.transform.SetParent(_cameraParent, false);
                _targetFollows[0] = CameraTargetFollow;
                _targetFollows[1] = Instantiate(CameraTargetFollow, _cameraParent, false);
                _targetFollows[1].name = CameraTargetFollow.name;

                for (int i = 0; i < TARGETING_CAMERAS; i++)
                {
                    _targetLookAt[i] = Instantiate(CameraTargetLookAtPrefab, _cameraParent)
                        .GetComponent<SimplePositionFollow>();
                    _targetLookAt[i].name = CameraTargetLookAtPrefab.name;
                    _targetLookAt[i].CheckNullFollowTarget = true;

                    _targetingCameras[i] = Instantiate(TargetCameraPrefab, _cameraParent).GetComponent<CinemachineVirtualCamera>();
                    // NOTE: name must be the same for cinemachine blend settings to affect it
                    _targetingCameras[i].name = TargetCameraPrefab.name;
                    _targetingCameras[i].Follow = _targetFollows[i].transform;
                    _targetingCameras[i].LookAt = _targetLookAt[i].transform;
                }

                ChangeCamera(_followCamera);

                ResetCameraRotation();

                _resetCameraAction = ctx =>
                {
                    if (!_targeter.IsTargeting)
                        ResetCameraRotation();
                };

                // Setup input
                _pawn.Character.Controller.Input.Actions["ResetCamera"].performed += _resetCameraAction;

                _targeter.OnStartTargeting += Targeter_OnStartTargeting;
                _targeter.OnStopTargeting += Targeter_OnStopTargeting;

                _targeter.OnNewTarget += Targeter_OnNewTarget;
                _targeter.OnLeavingTarget += Targeter_OnLeavingTarget;
            }
        }

        public void Targeter_OnStartTargeting()
        {
            _targetFollowLockout = false;
        }

        private void Targeter_OnStopTargeting()
        {
            ChangeCamera(_followCamera);
        }

        private void Targeter_OnNewTarget(Target target)
        {
            // switch cameras for smoothing
            _targetingCameraIndex = IntExtension.WrapIndex(_targetingCameraIndex + 1, _targetingCameras.Length);
            _targetLookAt[_targetingCameraIndex].SetFollowTarget(_targeter.CurrentTarget.transform);

            // remove lockout so we snap to the new target without waiting
            _targetFollowLockout = false;
        }

        private void Targeter_OnLeavingTarget(Target target, bool isTargeting)
        {

        }

        private IEnumerator TargetFollowLockout()
        {
            _targetFollowLockout = true;
            yield return new WaitForSeconds(_targetFollowLockoutTime);
            _targetFollowLockout = false;
            _targetFollowLockoutCoroutine = null;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner)
            {
                _pawn.Character.Controller.Input.Actions["ResetCamera"].performed -= _resetCameraAction;
            }
        }

        public void Death()
        {

        }

        public void Respawn()
        {

        }

        public void Activate()
        {
            if (IsOwner || IsServerInitialized)
            {
                _followCamera.gameObject.SetActive(true);
                foreach (var obj in _targetingCameras)
                {
                    obj.gameObject.SetActive(true);
                }
            }
        }

        public void Deactivate()
        {
            if (IsOwner || IsServerInitialized)
            {
                _followCamera.gameObject.SetActive(false);
                foreach (var obj in _targetingCameras)
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }

        private void LateUpdate()
        {
            if (IsOwner)
            {
                CameraRotation();
            }
        }

        private void ChangeCamera(CinemachineVirtualCamera cam)
        {
            if (_activeCamera != cam)
            {
                cam.Priority = CameraPriority;
                cam.MoveToTopOfPrioritySubqueue();
                _activeCamera = cam;
            }
        }

        private void CameraRotation()
        {
            if (!LockCameraPosition)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    // this needs a rework to split free look to target free look
                    // target free look needs an initial impulse to pull away from target

                    if (_pawn.Character.Controller.Input.Look.sqrMagnitude >= LOOK_THRESHOLD)
                    {
                        // change to timer rather than coroutine
                        if (_targetFollowLockoutCoroutine != null)
                            StopCoroutine(_targetFollowLockoutCoroutine);
                        _targetFollowLockoutCoroutine = StartCoroutine(TargetFollowLockout());

                        float pitchInput = _pawn.Character.Controller.Input.Look.y * SettingsMenu.VerticalSensitivity;
                        if (SettingsMenu.PitchInverted)
                            pitchInput *= -1;

                        float yawInput = _pawn.Character.Controller.Input.Look.x * SettingsMenu.HorizontalSensitivity;
                        if (SettingsMenu.YawInverted)
                            yawInput *= -1;

                        float pitch = _cameraGoalPitch + pitchInput;
                        float yaw = _cameraGoalYaw + yawInput;
                        SetCameraRotation(pitch, yaw);

                        ChangeCamera(_followCamera);
                    }
                }

                if (_targeter.IsTargeting && !_targetFollowLockout)
                {
                    for (int i = 0; i < _targetFollows.Length; i++)
                    {
                        _targetFollows[i].transform.LookAt(_targetLookAt[i].transform.position);
                    }

                    if (!_cinemachineBrain.IsBlending && _activeCamera != _followCamera)
                    {
                        Vector3 rotation = _targetFollows[_targetingCameraIndex].transform.eulerAngles;

                        var pitch = rotation.x;
                        var yaw = rotation.y;
                        SetCameraRotation(pitch, yaw);
                    }

                    ChangeCamera(_targetingCameras[_targetingCameraIndex]);
                }
            }

            CameraFollow.transform.rotation = Quaternion.Euler(
                _cameraGoalPitch + CameraAngleOverride,
                _cameraGoalYaw, 0.0f);

            OnCameraPostUpdate?.Invoke(_targeter.IsTargeting);
        }

        private void SetCameraRotation(float pitch, float yaw)
        {
            _cameraGoalPitch = FloatExtension.ClampAngle(pitch, CameraClamp.Minimum, CameraClamp.Maximum);
            _cameraGoalYaw = yaw; //FloatExtension.ClampAngle(yaw, float.MinValue, float.MaxValue);
        }

        // TODO: change default to false once its implemented
        /// <summary>
        /// Faces the camera to Characters facing direction
        /// </summary>
        /// <param name="snap">Whether to instantly snap the rotation and position</param>
        [Client(RequireOwnership = true)]
        public void ResetCameraRotation(bool snap = true)
        {
            if (snap)
            {
                _cameraGoalPitch = _pawn.transform.rotation.eulerAngles.x;
                _cameraGoalYaw = _pawn.transform.rotation.eulerAngles.y;
            }
            else
            {
                // TODO disable input to camera and tween towards local rotation
            }
        }
    }
}