using UnityEngine;

namespace Assets.App.Scripts.Characters.States.Abstract
{
    /// <remarks>
    /// Child states of this class can cast skills! If an exception is necessary add it to CharacterSkillHandler.CanExecuteSkill
    /// </remarks>
    public abstract class CharacterStateGroundMove : CharacterStateMove
    {
        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            SetGrounded(true);

            _pawn.Animator.SetBool(_pawn.AnimIDJump, false);
            _pawn.Animator.SetBool(_pawn.AnimIDFreeFall, false);
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            GroundedCheck(delta);

            if (_pawn.Grounded)
            {
                if (md.Jump)
                {
                    _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Jump);
                }
                else
                {
                    Vector3 moveDirection = new Vector3(md.Move.x, 0.0f, md.Move.y).normalized;
                    // note: Vector3's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                    if (moveDirection == Vector3.zero)
                    {
                        _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Idle);
                    }
                    else
                    {
                        _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Run);
                    }
                }
            }
            else
            {
                _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Fall);
            }

            AddGravity(delta);
            base.MoveWithData(md, delta);
        }
    }
}