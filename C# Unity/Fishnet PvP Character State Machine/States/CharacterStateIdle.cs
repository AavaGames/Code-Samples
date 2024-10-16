using Assets.App.Scripts.Characters.States.Abstract;
using System.Collections;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateIdle : CharacterStateGroundMove
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