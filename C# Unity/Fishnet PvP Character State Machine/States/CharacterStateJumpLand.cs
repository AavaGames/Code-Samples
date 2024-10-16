using Assets.App.Scripts.Characters.States.Abstract;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateJumpLand : CharacterStateMove
    {

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            // Exit immediately if there is no landing duration
            if (_pawn.Character.Data.JumpLandingDuration <= 0.0f)
            {
                _pawn.StateMachine.SkipState(CharacterStateMachine.State.Idle);
                return;
            }

            base.EnterState(previousState);

            SetGrounded(true);

            _pawn.StateTimer = _pawn.Character.Data.JumpLandingDuration;
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            // Don't reset _pawn.AnimationSpeedBlend if we're moving between idle and run
            if (nextState == CharacterStateMachine.State.Idle || nextState == CharacterStateMachine.State.Run)
                return;

            base.ExitState(nextState);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);

            GroundedCheck(delta);

            if (_pawn.Grounded)
            {
                _pawn.Animator.SetBool(_pawn.AnimIDJump, false);
                _pawn.Animator.SetBool(_pawn.AnimIDFreeFall, false);
            }
            else
            {
                _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Fall);
            }


            // Gradually increase back up to normal speed
            _moveSpeed = Mathf.SmoothStep(
                _pawn.Character.Data.MoveSpeed * _pawn.Character.Data.JumpLandMoveSpeedMultiplier,
                _pawn.Character.Data.MoveSpeed,
                _pawn.Character.Data.JumpLandMoveSpeedCurve.Evaluate
                (1 - _pawn.StateTimer / _pawn.Character.Data.JumpLandingDuration)
            );

            AddGravity(delta);
            base.MoveWithData(md, delta);

            UpdateTimer(delta);
        }

        protected override void TimerFinished()
        {
            base.TimerFinished();

            _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Idle);
        }
    }
}