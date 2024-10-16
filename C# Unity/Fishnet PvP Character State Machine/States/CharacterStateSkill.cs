using Assets.App.Scripts.Characters.States.Abstract;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateSkill : CharacterState
    {
        private const float STUCK_TIME = 0.5f;

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            _pawn.ResetVelocity();

            // Face target if there is one
            if (_pawn.CurrentMoveData.Targeting)
                _pawn.LookAt(_pawn.CurrentMoveData.TargetPosition);
            else
                _pawn.Rotate(_pawn.CurrentMoveData.CameraEulers.y);

            // Used as backup in case Character gets stuck in skill
            _pawn.StateTimer = 0;

            // TODO incorporate some sort of wait till turn to execute skill?
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);

            _pawn.Character.SkillHandler.ClearSkill();
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);

            bool endState = false;
            if (!_pawn.Character.SkillHandler.UsingSkill)
            {
                Debug.LogError($"{_pawn.name} isn't using a skill anymore! Ending state.");
                endState = true;
            }
            else
            {
                var stateInfo = _pawn.Animator.GetCurrentAnimatorStateInfo(0);
                var nextStateInfo = _pawn.Animator.GetNextAnimatorStateInfo(0);

                // A backup skill kick, very very rarely does the client host (not sure about clients)
                // gets stuck here

                if (_pawn.Character.SkillHandler.CurrentSkill != null)
                {
                    string stateName = _pawn.Character.SkillHandler.CurrentSkill.AnimationClip.name;
                    if (!stateInfo.IsName(stateName) && !nextStateInfo.IsName(stateName))
                    {
                        _pawn.StateTimer += delta;
                        if (_pawn.StateTimer > STUCK_TIME)
                        {
                            Debug.LogError($"{_pawn.name} was stuck in Skill state! Ending state.");
                            endState = true;
                        }
                    }
                }
            }

            if (endState)
            {
                Debug.LogError($"{_pawn.name} ending skill state.");
                _pawn.SkillQueued.End = true;
            }
        }

        public override void EndState()
        {
            base.EndState();

            GroundedCheck(0f);

            if (_pawn.Grounded)
                _pawn.StateMachine.ImmediatelyChangeToState(CharacterStateMachine.State.Idle);
            else
                _pawn.StateMachine.ImmediatelyChangeToState(CharacterStateMachine.State.Fall);
        }
    }
}