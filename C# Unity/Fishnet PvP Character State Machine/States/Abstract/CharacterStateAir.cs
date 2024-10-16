using System.Collections;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States.Abstract
{
    /// <remarks>
    /// Child states of this class can cast skills! If an exception is necessary add it to CharacterSkillHandler.CanExecuteSkill
    /// </remarks>
    public abstract class CharacterStateAir : CharacterStateMove
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