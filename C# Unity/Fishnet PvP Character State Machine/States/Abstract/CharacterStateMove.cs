using UnityEngine;

namespace Assets.App.Scripts.Characters.States.Abstract
{
    public abstract class CharacterStateMove : CharacterState
    {
        [Tooltip("The MoveSpeed of the state")]
        protected float _moveSpeed;
        protected override bool MovementState => true;


        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            _moveSpeed = _pawn.Character.Data.MoveSpeed;
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);

            _pawn.Animator.SetFloat(_pawn.AnimIDMotionSpeed, 1.0f);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            Vector3 moveDirection = new Vector3(md.Move.x, 0.0f, md.Move.y).normalized;

            // Rotation
            if (moveDirection != Vector3.zero)
            {
                _pawn.GoalRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + md.CameraEulers.y;
            }
            else if (md.Targeting)
            {
                Vector3 directionToTarget = md.TargetPosition - _pawn.transform.position;
                Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget, Vector3.up);
                _pawn.GoalRotation = rotationToTarget.eulerAngles.y;
            }

            Quaternion goalQuaternion = Quaternion.Euler(0.0f, _pawn.GoalRotation, 0.0f);

            // rotate to face target or input direction relative to camera position
            _pawn.transform.rotation =
                Quaternion.Slerp(_pawn.transform.rotation, goalQuaternion,
                _pawn.Character.Data.RotationSmoothSpeed * delta);

            Vector3 horizontalMovement = Vector3.zero;

            // NOTE: Has desync jitter issues at the moment
            if (_pawn.Character.Data.StopMovementTillFacingDirection)
            {
                if (Quaternion.Angle(goalQuaternion, transform.rotation) <= _pawn.Character.Data.FacingDirectionAnglePadding)
                {
                    horizontalMovement = goalQuaternion * Vector3.forward;
                }
            }
            else
            {
                horizontalMovement = goalQuaternion * Vector3.forward;
            }

            //Movement
            float targetSpeed = _moveSpeed * _pawn.Character.Stats.Buffs.MoveSpeed;
            // note: Vector3's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            if (moveDirection == Vector3.zero || horizontalMovement == Vector3.zero)
            {
                targetSpeed = 0.0f;
            }

            UpdateAnimSpeedBlend(targetSpeed, delta);
            _pawn.Animator.SetFloat(_pawn.AnimIDMotionSpeed, 1.0f * _pawn.Character.Stats.Buffs.MoveSpeed);

            Vector3 verticalVelocity = new Vector3(0.0f, _pawn.VerticalVelocity, 0.0f) * delta;

            // Re-add here when physics is incorporated
            //Vector3 horizontalVelocity = transform.forward * _pawn.HorizontalVelocity * delta;

            _pawn.CharacterController.Move(horizontalMovement * (targetSpeed * delta) + verticalVelocity);
        }
    }
}