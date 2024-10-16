using Assets.App.Scripts.Characters.States.Abstract;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateRun : CharacterStateGroundMove
    {

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);
        }
    }
}