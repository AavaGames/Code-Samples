using Assets.App.Scripts.Characters.States.Abstract;
using Assets.App.Scripts.Structs;
using FishNet;
using UnityEngine;

namespace Assets.App.Scripts.Characters.States
{
    public class CharacterStateHit : CharacterState
    {
        private float _startHeight;

        public override void EnterState(CharacterStateMachine.State previousState)
        {
            base.EnterState(previousState);

            _pawn.NetworkAnimator.Play("Hit");
            _pawn.Animator.SetBool(_pawn.AnimIDKnockedDown, true);

            SetGrounded(false);
            _startHeight = _pawn.transform.position.y;

            // the square root of H * -2 * G = how much velocity needed to reach desired height
            _pawn.VerticalVelocity = Mathf.Sqrt(_pawn.Character.Data.HitLaunchHeight * -2f * _pawn.Character.Data.Gravity);

            // Used to lerp horizontal velocity
            _pawn.StateTimer = 0;

            if (InstanceFinder.IsServerStarted)
                _pawn.SetDowned(true);
        }

        public override void ExitState(CharacterStateMachine.State nextState)
        {
            base.ExitState(nextState);
            _pawn.HorizontalVelocity = 0;
        }

        public override void MoveWithData(CharacterPawn.MoveData md, float delta)
        {
            base.MoveWithData(md, delta);

            CeilingCheck();
            AddGravity(delta);

            if (_pawn.StateTimer <= 1.0f)
            {
                _pawn.StateTimer += delta / _pawn.Character.Data.HitLaunchHorizontalVelocityDuration;
                _pawn.HorizontalVelocity = _pawn.Character.Data.HitLaunchHorizontalVelocity.Evaluate(_pawn.StateTimer) * -1;
            }

            if (_pawn.VerticalVelocity < 0.0f)
            {
                GroundedCheck(delta);

                if (_pawn.Grounded || HasStoppedMoving(delta))
                {
                    FallDamage();
                    _pawn.StateMachine.TransitionToState(CharacterStateMachine.State.KnockedDown);
                }
            }

            Vector3 verticalVelocity = new Vector3(0.0f, _pawn.VerticalVelocity, 0.0f) * delta;
            Vector3 horizontalVelocity = transform.forward * _pawn.HorizontalVelocity * delta;

            _pawn.CharacterController.Move(horizontalVelocity + verticalVelocity);
        }

        private void FallDamage()
        {
            float fallDistance = _startHeight - _pawn.transform.position.y;
            int damage = 0;
            foreach (var pair in _pawn.Character.Data.HitFallFallDistanceForDamage)
            {
                if (fallDistance > pair.Key)
                {
                    damage = pair.Value;
                }
            }

            if (damage > 0)
            {
                HitInfo hitInfo = new HitInfo("Falling", damage);
                _pawn.Character.Stats.Hit(hitInfo);
            }
        }
    }
}