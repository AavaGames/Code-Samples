using Assets.App.Scripts.Characters.States.Abstract;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateLandLock : CharacterState
    {
        public override void EnterState(CharacterStateMachine.State previousState)
        {
            // Exit immediately if there is no LandingLockout
            if (_pawn.Character.Data.LandingLockout <= 0.0f)
            {
                _pawn.StateMachine.SkipState(CharacterStateMachine.State.Idle);
                return;
            }

            base.EnterState(previousState);

            SetGrounded(true);

            _pawn.Animator.SetBool(_pawn.AnimIDLanded, true);

            // animation is about 0.75s long
            _pawn.Animator.SetFloat(_pawn.AnimIDMotionSpeed, 1 / (_pawn.Character.Data.LandingLockout + 0.5f));

            _pawn.StateTimer = _pawn.Character.Data.LandingLockout;
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
            _pawn.Animator.SetBool(_pawn.AnimIDLanded, false);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);

            AddGravity(delta);
            GroundedCheck(delta);

            // Move to make sure we're hugging the ground
            Vector3 verticalMovement = new Vector3(0.0f, _pawn.VerticalVelocity, 0.0f);
            _pawn.CharacterController.Move(verticalMovement * delta);

            UpdateTimer(delta);
        }

        protected override void TimerFinished()
        {
            base.TimerFinished();

            _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Idle);
        }
    }
}