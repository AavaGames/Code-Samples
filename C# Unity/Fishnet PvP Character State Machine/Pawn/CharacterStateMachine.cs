using Assets.App.Scripts.Characters.States;
using Assets.App.Scripts.Characters.States.Abstract;
using FishNet;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    public class CharacterStateMachine : MonoBehaviour, ICharacter
    {
        private CharacterPawn _pawn;

        public bool ShowGizmos = false;

        public enum State { Idle, Run, Jump, Fall, JumpLand, LandLock, Skill, Hit, KnockedDown, Standing }

        private Dictionary<State, CharacterState> _states = new Dictionary<State, CharacterState>();
        public Dictionary<State, CharacterState> States => _states;

        [ShowInInspector]
        private State _currentState = State.Idle;
        public State CurrentState => _currentState;

        [ShowInInspector]
        private State _previousState;
        public State PreviousState => _previousState;

        private CharacterState _currentCharacterState;
        public CharacterState CurrentCharacterState => _currentCharacterState;

        private bool _transitioning = false;
        private State _nextState = State.Idle;

        [Required]
        [SerializeField]
        private Transform _statesContainer;

        /// <summary>
        /// Used in place of Awake. Called by CharacterPawn to stay synchronized
        /// Requires Pawn AnimIDs and Character to be set
        /// </summary>
        public void Initialize(CharacterPawn pawn)
        {
            _pawn = pawn;

            // Add states to dictionary
            _states.Add(State.Idle, _statesContainer.GetComponent<CharacterStateIdle>());
            _states.Add(State.Run, _statesContainer.GetComponent<CharacterStateRun>());
            _states.Add(State.Jump, _statesContainer.GetComponent<CharacterStateJump>());
            _states.Add(State.Fall, _statesContainer.GetComponent<CharacterStateFall>());
            _states.Add(State.Skill, _statesContainer.GetComponent<CharacterStateSkill>());

            _states.Add(State.JumpLand, _statesContainer.GetComponent<CharacterStateJumpLand>());
            _states.Add(State.LandLock, _statesContainer.GetComponent<CharacterStateLandLock>());

            _states.Add(State.Hit, _statesContainer.GetComponent<CharacterStateHit>());
            _states.Add(State.KnockedDown, _statesContainer.GetComponent<CharacterStateKnockedDown>());
            _states.Add(State.Standing, _statesContainer.GetComponent<CharacterStateStanding>());

            foreach (CharacterState state in _states.Values)
            {
                state.Initialize(_pawn);
            }

            EnterState(_currentState);
        }

        public void Death()
        {

        }

        public void Respawn()
        {
            ImmediatelyChangeToState(State.Idle);
        }

        private void EnterState(State newState)
        {
            _previousState = _currentState;
            _currentState = newState;
            _currentCharacterState = _states[_currentState];
            _currentCharacterState.EnterState(_previousState);
        }

        /// <summary>
        /// Immediately enters into the new state after exiting previous one
        /// Should be used by everything outside of States MoveWithData function
        /// </summary>
        /// <param name="newState"></param>
        public void ImmediatelyChangeToState(State newState)
        {
            if (newState == CurrentState) return;

            _currentCharacterState.ExitState(newState);
            EnterState(newState);

            DebugShowState(false, true);
        }

        /// <summary>
        /// Transitions to a new state after finishing their MoveWithData
        /// Will ignore any additional states given if already transitioning
        /// Do NOT use outside of States MoveWithData function
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="immediately">Used when calling this function outside of CharacterStates</param>
        public void TransitionToState(State newState)
        {
            if (newState == CurrentState) return;

            _transitioning = true;
            _nextState = newState;
        }

        /// <summary>
        /// Skips the state and immediately moves on to another one without updating previous state.
        /// Should always be followed by return;
        /// Useful for things like skipping Land state under specific conditions
        /// </summary>
        /// <param name="newState"></param>
        public void SkipState(State newState)
        {
            if (newState == CurrentState) return;

            DebugShowState(true, false);

            _currentState = newState;
            _currentCharacterState = _states[_currentState];
            _currentCharacterState.EnterState(_previousState);
        }

        public void Reconcile(State currentState, State previousState)
        {
            //Debug.Log("State = " + CurrentState + " RECONCILED wants " + currentState);

            _previousState = previousState;
            _currentState = currentState;
            _currentCharacterState = _states[_currentState];
        }

        private void TransitionState()
        {
            if (_transitioning)
            {
                _transitioning = false;
                _currentCharacterState.ExitState(_nextState);
                EnterState(_nextState);

                DebugShowState();
            }
        }

        public void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            if (!md.Active) return;

            _currentCharacterState.MoveWithData(md, delta);
            TransitionState();
        }

        public void EndSkillState()
        {
            if (_currentCharacterState as CharacterStateSkill != null)
            {
                _currentCharacterState.EndState();
            }
        }

        //public void UpdateObservers(State currentState, State previousState)
        //{
        //    // Not a sync var because values change multiple times a frame
        //    _currentState = currentState;
        //    _previousState = previousState;
        //}

        public bool LogStateChanges = false;
        private void DebugShowState(bool skip = false, bool change = false)
        {
            if (!LogStateChanges) return;

            string text = "";

            if (skip)
                text = "// SKIPPED";
            else if (change)
                text = "// CHANGED";

            if (InstanceFinder.PredictionManager.IsReconciling)
                Debug.Log("State = " + _currentState + " // REPLAY" + text);
            else
                Debug.Log("State = " + _currentState + " " + text);
        }
    }
}