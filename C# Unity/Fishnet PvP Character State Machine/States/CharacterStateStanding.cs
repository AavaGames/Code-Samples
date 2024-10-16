using Assets.App.Scripts.Characters.States.Abstract;
using FishNet;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateStanding : CharacterState
    {

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            _pawn.Animator.SetFloat(_pawn.AnimIDGetUpLerp, 0f);

            // Used for lerping standing
            _pawn.StateTimer = 0;

            if (InstanceFinder.IsServerStarted)
            {
                _pawn.Character.Stats.EnterImmunity(_pawn.Character.Data.StandingDuration +
                    _pawn.Character.Data.AfterStandingImmunityDuration);
            }
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);

            _pawn.StateTimer += delta / _pawn.Character.Data.StandingDuration;
            _pawn.Animator.SetFloat(_pawn.AnimIDGetUpLerp, _pawn.StateTimer);

            if (_pawn.StateTimer >= 1.0f)
                _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.Idle);
        }
    }
}