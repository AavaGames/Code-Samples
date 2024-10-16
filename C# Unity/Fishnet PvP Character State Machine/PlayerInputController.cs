using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.App.Scripts.Players
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputController : NetworkBehaviour
    {
        public PlayerInput Input { get; private set; }

        public InputActionAsset Actions => Input.actions;

        public InputActionMap BaseMap { get; private set; }
        public InputActionMap ModifierMap { get; private set; }

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }

        public bool IsCurrentDeviceMouse
        {
            get
            {
                if (Input == null) return false;
                return Input.currentControlScheme == "Keyboard Mouse";
            }
        }

        private void Awake()
        {
            Input = GetComponent<PlayerInput>();
            Input.enabled = false;
        }


        private Action<InputAction.CallbackContext> _onModifierStarted;
        private Action<InputAction.CallbackContext> _onModifierCanceled;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                Input.enabled = true;

                // Only gameplay action map is active by default

                BaseMap = Input.actions.FindActionMap("Gameplay_Base");
                ModifierMap = Input.actions.FindActionMap("Gameplay_Modifier");

                BaseMap.Enable();
                ModifierMap.Disable();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Input.actions.FindActionMap("Dev").Enable();
#endif

                _onModifierStarted = ctx =>
                {
                    ModifierMap.Enable();
                    BaseMap.Disable();
                };
                _onModifierCanceled += ctx =>
                {
                    ModifierMap.Disable();
                    BaseMap.Enable();
                };

                InputAction modifier = Input.actions["Modifier"];
                modifier.started += _onModifierStarted;
                modifier.canceled += _onModifierCanceled;
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner)
            {
                Input.enabled = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Input.actions.FindActionMap("Dev").Disable();
#endif

                InputAction modifier = Input.actions["Modifier"];
                modifier.started -= _onModifierStarted;
                modifier.canceled -= _onModifierCanceled;
            }
        }

        private void LateUpdate()
        {
            if (Input.enabled)
            {
                if (Input.inputIsActive &&
                    CursorController.CaptureMode == CursorController.CursorActiveCaptureState.Menu)
                {
                    DeactivateInput();
                }
                else if (!Input.inputIsActive &&
                    CursorController.CaptureMode == CursorController.CursorActiveCaptureState.Gameplay)
                {
                    ActivateInput();
                }
            }
        }

        public void ActivateInput()
        {
            Input.ActivateInput();
        }

        public void DeactivateInput()
        {
            Input.DeactivateInput();
        }

        public InputAction GetAction(string actionName)
        {
            return Input.actions[actionName];
        }

        #region Inputs

        public void OnMove(InputValue inputValue)
        {
            Move = inputValue.Get<Vector2>();
        }

        public void OnLook(InputValue inputValue)
        {
            Look = inputValue.Get<Vector2>();
        }


        // Examples
        public bool MovementAbilityPressed()
        {
            return Input.actions["MovementAbility"].WasPerformedThisFrame();
        }

        #endregion

        public virtual bool IsMoving()
        {
            bool hasHorizontalInput = !Mathf.Approximately(Move.x, 0f);
            bool hasVerticalInput = !Mathf.Approximately(Move.y, 0f);
            return hasHorizontalInput || hasVerticalInput;
        }

        public bool MouseToWorldHitPoint(out RaycastHit hit, float maxCheckDistance = Mathf.Infinity)
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            return gameObject.scene.GetPhysicsScene().Raycast(ray.origin, ray.direction, out hit, maxCheckDistance);
        }

    }
}
