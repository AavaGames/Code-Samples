using Assets.App.Scripts.Characters.States.Abstract;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateJump : CharacterStateAir
    {

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _pawn.VerticalVelocity = Mathf.Sqrt(
                (_pawn.Character.Data.JumpHeight * _pawn.Character.Stats.Buffs.JumpHeight)
                * -2f * _pawn.Character.Data.Gravity);

            SetGrounded(false);
            _pawn.Animator.SetBool(_pawn.AnimIDJump, true);
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
            _pawn.Animator.SetBool(_pawn.AnimIDJump, false);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            CeilingCheck();
            AddGravity(delta);

            base.MoveWithData(md, delta);

            if (_pawn.VerticalVelocity < 0.0f)
            {
                _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Fall);
            }
        }
    }
}