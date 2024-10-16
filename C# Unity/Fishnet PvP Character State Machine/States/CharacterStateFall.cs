using Assets.App.Scripts.Characters.States.Abstract;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateFall : CharacterStateAir
    {
        private float _startHeight;
        private bool _wasJumping;

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            // Skip was jumping check if last was skill to not overwrite whether we jumped before the skill or not
            if (_pawn.StateMachine.PreviousState != CharacterStateMachine.State.Skill)
                _wasJumping = _pawn.StateMachine.PreviousState == CharacterStateMachine.State.Jump;

            SetGrounded(false);
            _startHeight = _pawn.transform.position.y;

            // used to flip fall animator bool to show animation
            _pawn.StateTimer = _pawn.Character.Data.WaitTillFallAnimTime;
            _pawn.Animator.SetBool(_pawn.AnimIDFreeFall, false);
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
            _pawn.Animator.SetBool(_pawn.AnimIDFreeFall, false);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            UpdateTimer(delta);

            GroundedCheck(delta);

            if (_pawn.Grounded)
            {
                float fallDistance = _startHeight - _pawn.transform.position.y;
                float fallDistanceForLanding = _pawn.Character.Data.FallDistanceForLandingLock;

                if (fallDistance >= fallDistanceForLanding)
                    _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.LandLock);
                else
                {
                    if (_pawn.Character.Data.LandingSlowIfJumped && _wasJumping)
                        _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.JumpLand);
                    else
                        _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Idle);
                }
            }
            AddGravity(delta);

            base.MoveWithData(md, delta);
        }

        protected override void TimerFinished()
        {
            base.TimerFinished();

            _pawn.Animator.SetBool(_pawn.AnimIDFreeFall, true);
        }
    }
}