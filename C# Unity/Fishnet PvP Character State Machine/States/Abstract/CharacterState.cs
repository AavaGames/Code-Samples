using Assets.App.Scripts.Extensions;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States.Abstract
{
    public abstract class CharacterState : MonoBehaviour
    {
        protected const float GROUND_HUG_VELOCITY = -2.0f;

        protected CharacterPawn _pawn;
        protected virtual bool MovementState => false;

        // Used for HasStoppedMoving
        protected Vector3 _lastPosition;
        protected float _stoppedMovingTime = 0.01f;
        protected float _stoppedMovingTimer = 0f;

        /// <summary>
        /// Used in place of Awake. Called by StateMachine to stay synchronized
        /// </summary>
        public virtual void Initialize(CharacterPawn pawn)
        {
            _pawn = pawn;
            // Make sure we're always at local Vector3.zero
            transform.ResetLocal();
        }

        /// <summary>
        /// Setup state
        /// </summary>
        public virtual void EnterState(CharacterStateMachine.State previousState)
        {
            //Debug.Log("Entered state " + _pawn.CurrentState);
        }

        /// <summary>
        /// Cleanup state
        /// </summary>
        public virtual void ExitState(CharacterStateMachine.State nextState) { }

        public virtual void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            // if not in a move state then smooth AnimSpeedBlend back to 0 for less snapping
            if (!MovementState)
            {
                UpdateAnimSpeedBlend(0f, delta);
            }
        }

        protected void UpdateAnimSpeedBlend(float targetSpeed, float delta)
        {
            if (_pawn.AnimSpeedBlend == targetSpeed)
                return;

            _pawn.AnimSpeedBlend = FloatExtension.Damp(_pawn.AnimSpeedBlend, targetSpeed,
                _pawn.AnimSpeedDampSmoothing, delta);

            if (FloatExtension.Approximately(_pawn.AnimSpeedBlend, targetSpeed, 0.01f))
                _pawn.AnimSpeedBlend = targetSpeed;

            _pawn.Animator.SetFloat(_pawn.AnimIDSpeed, _pawn.AnimSpeedBlend);
        }

        /// <summary>
        /// Used for certain states that transition outside of the state system 
        /// (e.g waiting for skill animation to finish)
        /// </summary>
        public virtual void EndState() { }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            if (!_pawn.StateMachine.ShowGizmos) return;

            CharacterController characterController = _pawn.CharacterController;

            Vector3 bottomSpherePosition = transform.position;
            bottomSpherePosition.y -= _pawn.Character.Data.GroundedOffset;

            Vector3 topSpherePosition = bottomSpherePosition;
            topSpherePosition.y += characterController.height - (characterController.radius * 2);

            Color c = Color.magenta;
            c.a = 0.5f;
            Gizmos.color = c;
            Gizmos.DrawSphere(bottomSpherePosition, _pawn.CharacterController.radius);
            Gizmos.DrawSphere(topSpherePosition, _pawn.CharacterController.radius);

            //spherePosition = transform.position;
            //spherePosition.y -= _pawn.Character.Data.GroundedOffset;
            //Gizmos.DrawSphere(spherePosition, _pawn.CharacterController.radius);
        }


        /// <summary>
        /// A defensive function to check if we've stopped moving for a time
        /// used in places to make sure we don't get soft locked 
        /// like Hit state if certain conditions fail to get met.
        /// </summary>
        protected bool HasStoppedMoving(float delta)
        {
            // Vector3 comparison uses approximation
            if (_lastPosition == transform.position)
            {
                _lastPosition = transform.position;

                _stoppedMovingTimer += delta;
                if (_stoppedMovingTimer >= _stoppedMovingTime)
                {
                    _stoppedMovingTimer = 0f;
                    Debug.LogWarning("Soft lock detected in " + _pawn.CurrentState + " state. Flagging HasStoppedMoving true.");
                    return true;
                }
            }
            else
            {
                _lastPosition = transform.position;
                _stoppedMovingTimer = 0f;
            }

            return false;
        }

        /// <summary>
        /// Checks Grounded and keeps track of fall timer (only timer needs delta).
        /// </summary>
        /// <param name="delta"></param>
        protected void GroundedCheck(float delta)
        {
            CharacterController characterController = _pawn.CharacterController;

            Vector3 bottomSpherePosition = transform.position;
            bottomSpherePosition.y -= _pawn.Character.Data.GroundedOffset;

            Vector3 topSpherePosition = bottomSpherePosition;
            topSpherePosition.y += characterController.height - (characterController.radius * 2);

            PhysicsScene physics = gameObject.scene.GetPhysicsScene();
            // results buffer does not increase past given array size
            // See: https://docs.unity3d.com/ScriptReference/Physics.OverlapSphereNonAlloc.html

            // check if any collision with ground
            // using capsule instead of sphere because moving a sphere down slightly
            // creates a gap between the actual collider and the sphere cast
            // which rarely could result in a false negative (soft lock for hit state)
            SetGrounded(physics.OverlapCapsule(bottomSpherePosition, topSpherePosition,
                characterController.radius, new Collider[1],
                _pawn.Character.Data.GroundLayers, QueryTriggerInteraction.Ignore) > 0);

            if (_pawn.Grounded)
            {
                // stop our velocity dropping infinitely when grounded
                // but move down to make sure we're hugging the ground
                if (_pawn.VerticalVelocity < 0.0f)
                    _pawn.VerticalVelocity = GROUND_HUG_VELOCITY;
            }
        }

        protected void SetGrounded(bool grounded)
        {
            _pawn.Grounded = grounded;
            _pawn.Animator.SetBool(_pawn.AnimIDGrounded, grounded);
        }

        /// <summary>
        /// Checks whether we hit the ceiling to reset our vertical velocity to start falling
        /// </summary>
        protected void CeilingCheck()
        {
            Vector3 spherePosition = transform.position;
            spherePosition.y += _pawn.CharacterController.height;
            spherePosition.y += _pawn.Character.Data.GroundedOffset;

            PhysicsScene physics = gameObject.scene.GetPhysicsScene();
            bool ceiling = physics.OverlapSphere(spherePosition, _pawn.CharacterController.radius, new Collider[1],
                _pawn.Character.Data.GroundLayers, QueryTriggerInteraction.Ignore) > 0;

            if (ceiling && _pawn.VerticalVelocity > 0.0f)
            {
                _pawn.VerticalVelocity = 0.0f;
            }
        }



        protected void AddGravity(float delta)
        {
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_pawn.VerticalVelocity > _pawn.Character.Data.TerminalVelocity)
            {
                _pawn.VerticalVelocity += _pawn.Character.Data.Gravity * delta;
            }
        }

        /// <summary>
        /// Used to tick down the timer to 0
        /// If something else is needed with the timer than that class will manage the variable
        /// TimerFinished() is called when timer reaches 0.0f
        /// </summary>
        protected void UpdateTimer(float delta)
        {
            if (_pawn.StateTimer > 0.0f)
                _pawn.StateTimer -= delta;

            if (_pawn.StateTimer <= 0.0f)
                TimerFinished();
        }

        /// <summary>
        /// Called when timer reached 0.0f
        /// </summary>
        protected virtual void TimerFinished() { }
    }
}