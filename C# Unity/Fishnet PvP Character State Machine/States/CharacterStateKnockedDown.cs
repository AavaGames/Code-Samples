using Assets.App.Scripts.Characters.States.Abstract;
using FishNet;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateKnockedDown : CharacterState
    {

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            SetGrounded(true);

            _pawn.StateTimer = _pawn.Character.Data.KnockedDownDuration;

            _pawn.Animator.SetBool(_pawn.AnimIDKnockedDown, true);
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);

            _pawn.Animator.SetBool(_pawn.AnimIDKnockedDown, false);

            if (InstanceFinder.IsServerStarted) // only call on server
                _pawn.SetDowned(false);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);

            AddGravity(delta);
            GroundedCheck(delta);

            // Move to make sure we're hugging the ground
            Vector3 verticalMovement = new Vector3(0.0f, _pawn.VerticalVelocity, 0.0f);
            _pawn.CharacterController.Move(verticalMovement * delta);

            // stay in this state if we're dead
            if (!_pawn.Character.IsDead)
                UpdateTimer(delta);
        }

        protected override void TimerFinished()
        {
            base.TimerFinished();

            _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Standing);
        }
    }
}