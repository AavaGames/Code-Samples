using ANU.IngameDebug.Console;
using Assets.App.Scripts.Combat;
using Assets.App.Scripts.Extensions;
using FishNet;
using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Assets.App.Scripts.Characters.Pawn
{
    public class CharacterTargeter : NetworkBehaviour, ICharacter
    {
        private CharacterPawn _pawn;
        private CharacterCamera _camera;

        public enum TargetSortingType { List, Distance }
        [OnValueChanged("SetTargetingTypeThroughInspector")]
        public TargetSortingType TargetSorting = TargetSortingType.Distance;

        /// <summary>
        /// Do not directly set this value, use CanTarget instead
        /// </summary>
        [SerializeField]
        [OnValueChanged("SetCanTarget")]
        private bool _canTarget = true;
        private void SetCanTarget(bool value)
        {
            // if we're targeting and we can't target anymore
            if (_isTargeting && !value)
                SetTargeting(false); // must call before setting _canTarget or it fails

            _canTarget = value;
        }

        [SerializeField]
        private bool _isTargeting = false;
        public bool IsTargeting => _isTargeting;

        private float _targetDistance = 0f;
        public float TargetDistance => _targetDistance;

        private List<Target> _validTargets = new List<Target>();
        private int _targetIndex = 0;

        [SerializeField]
        private Target _currentTarget;
        public Target CurrentTarget => _currentTarget;

        private Target _previousTarget; // used for camera transition and retargeting previous target

        private Action<InputAction.CallbackContext> _targetAction;
        private Action<InputAction.CallbackContext> _nextTargetAction;
        private Action<InputAction.CallbackContext> _previousTargetAction;

        [Tooltip("When Targeter has begun targeting. Called before OnNewTarget.")]
        public Action OnStartTargeting;
        [Tooltip("When Targeter has stopped targeting. Called after OnLeavingTarget.")]
        public Action OnStopTargeting;

        [Tooltip("When a new target has be acquired.\nTarget currentTarget")]
        public Action<Target> OnNewTarget;
        [Tooltip("When a target has stopped being targeted. NOTE: if CurrentTarget is destroyed, this is not called.\n" +
            "Target previousTarget, bool isTargeting")]
        public Action<Target, bool> OnLeavingTarget;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                InstanceFinder.TimeManager.OnUpdate += TimeManager_OnUpdate;
                InstanceFinder.TimeManager.OnLateUpdate += TimeManager_OnLateUpdate;

                _pawn = GetComponent<CharacterPawn>();
                _camera = GetComponent<CharacterCamera>();

                // Setup Targeting
                SetTargetingType(TargetSorting);

                _targetAction = ctx => ToggleTargeting();
                _nextTargetAction = ctx => FindTarget(true);
                _previousTargetAction = ctx => FindTarget(false);

                _pawn.Character.Controller.Input.Actions["Target"].performed += _targetAction;
                _pawn.Character.Controller.Input.Actions["NextTarget"].performed += _nextTargetAction;
                _pawn.Character.Controller.Input.Actions["PreviousTarget"].performed += _previousTargetAction;

            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnUpdate -= TimeManager_OnUpdate;
                InstanceFinder.TimeManager.OnLateUpdate -= TimeManager_OnLateUpdate;
            }

            if (_pawn != null)
            {
                _pawn.Character.Controller.Input.Actions["Target"].performed -= _targetAction;
                _pawn.Character.Controller.Input.Actions["NextTarget"].performed -= _nextTargetAction;
                _pawn.Character.Controller.Input.Actions["PreviousTarget"].performed -= _previousTargetAction;
            }

            // Clear events just in case
            OnStartTargeting = null;
            OnStopTargeting = null;
            OnNewTarget = null;
            OnLeavingTarget = null;
        }

        public void Death()
        {
            SetCanTarget(false); // stops targeting too
        }

        public void Respawn()
        {
            SetCanTarget(true);
        }

        private void ToggleTargeting()
        {
            SetTargeting(!_isTargeting);
        }

        [DebugCommand("cycle_target_sorting")]
        private void CycleTargetSorting()
        {
            if (TargetSorting == TargetSortingType.Distance)
                SetTargetingType(TargetSortingType.List);
            else
                SetTargetingType(TargetSortingType.Distance);

            Debug.Log("Set Target Cycle to " + TargetSorting.ToString());
        }

        private void TimeManager_OnUpdate()
        {
            if (_isTargeting)
            {
                if (!CanTarget(CurrentTarget))
                {
                    SetTargeting(false);

                    // if we want to cycle target when we kill someone
                    // remove else and let it flow through
                    //FindTarget(true);
                    //if (!_targeting) // failed to find target
                    //    return;
                }
            }
        }

        private void TimeManager_OnLateUpdate()
        {
            if (_isTargeting)
            {
                _targetDistance = Vector3.Distance(transform.position, CurrentTarget.transform.position);
            }
        }

        public void SetTargetingType(TargetSortingType type)
        {
            SetTargeting(false);

            TargetSorting = type;
        }

        private void SetTargetingTypeThroughInspector() { SetTargetingType(TargetSorting); }

        public bool CanTarget(Target target)
        {
            if (!_canTarget) return false;

            if (target == null) return false;

            return target.CanTarget(_pawn.Target);
        }

        private void SetTargeting(bool targeting)
        {
            if (!_canTarget) return;
            if (_isTargeting == targeting) return;

            _isTargeting = targeting;

            if (_isTargeting)
            {
                // TODO Rework to be based on who you're looking at and whos closest to the your reticle
                // Loop characters and WorldToScreenPoint

                Target validTarget;

                if (TargetSorting == TargetSortingType.List && CanTarget(_previousTarget))
                {
                    validTarget = _previousTarget;
                }
                else
                {
                    validTarget = FindValidTarget(true);
                }

                if (validTarget != null)
                {
                    OnStartTargeting?.Invoke();
                    SetTarget(validTarget);
                }
                else // do nothing if we failed to find a valid target
                {
                    _isTargeting = false;
                }
            }
            else
            {
                SetTarget(null);

                // this will only be called if we were 
                OnStopTargeting?.Invoke();
            }
        }

        /// <summary>
        /// Sets the target and invokes events. Target should have passed CanTarget before entering here.
        /// </summary>
        private void SetTarget(Target target)
        {
            _previousTarget = CurrentTarget;

            // target could have become untargetable
            // can be null if last target was destroyed
            if (_previousTarget != null)
                OnLeavingTarget?.Invoke(_previousTarget, _isTargeting);

            // Can be null
            _currentTarget = target;

            if (CurrentTarget != null)
                OnNewTarget?.Invoke(CurrentTarget);

            if (IsOwner)
                ServerUpdateTarget(target);
        }

        [ServerRpc(RequireOwnership = true)]
        private void ServerUpdateTarget(Target target)
        {
            if (!IsOwner) // skip client host
                SetTarget(target);

            ObserverUpdateTarget(target);
        }

        [ObserversRpc(ExcludeOwner = true, BufferLast = true)]
        private void ObserverUpdateTarget(Target target)
        {
            SetTarget(target);
        }

        public void FindTarget(bool forward)
        {
            if (!_canTarget) return;
            if (!_isTargeting) return;

            Target validTarget = FindValidTarget(forward);

            if (CurrentTarget == validTarget)
            {
                // already targeting only viable target
                return;
            }
            else
            {
                SetTarget(validTarget);
            }
        }

        private Target FindValidTarget(bool forward)
        {
            if (!_canTarget) return null;
            if (!_isTargeting) return null;

            if (_pawn.Target.TargetListChanged)
                _validTargets = _pawn.Target.GetTargets(false);

            if (_validTargets.Count < 1)
            {
                return null;
            }
            else if (_validTargets.Count == 1)
            {
                _targetIndex = 0;

                if (CanTarget(_validTargets[_targetIndex]))
                {
                    return _validTargets[_targetIndex];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (TargetSorting == TargetSortingType.List)
                {
                    return FindValidTargetByList(forward);
                }
                else if (TargetSorting == TargetSortingType.Distance)
                {
                    return FindValidTargetByDistance(forward);
                }

                return null;
            }
        }

        public Target FindValidTargetByList(bool forward)
        {
            if (CurrentTarget == null)
                _targetIndex = 0;
            else
                _targetIndex = IntExtension.CycleIndex(_targetIndex, _validTargets.Count, forward ? 1 : -1);

            // cycle through the list until we find a valid one or fail
            for (int i = 0; i < _validTargets.Count; i++)
            {
                Target target = _validTargets[_targetIndex];
                if (CanTarget(target))
                    return target;

                _targetIndex = IntExtension.CycleIndex(_targetIndex, _validTargets.Count, forward ? 1 : -1);
            }

            return null;
        }

        public Target FindValidTargetByDistance(bool forward)
        {
            List<float> distances = new List<float>(_validTargets.Count);
            Vector3 pos = transform.position; // speeds up loop
            float currentTargetDistance = 0;

            // Gather all distances
            for (int i = 0; i < _validTargets.Count; i++)
            {
                // Using sqr magnitude is faster than magnitude and results are the same
                float distance = (_validTargets[i].transform.position - pos).sqrMagnitude;
                distances.Add(distance);

                if (i == _targetIndex)
                    currentTargetDistance = distance;
            }

            // Save original list so we can find index later
            List<float> originalDistances = new List<float>(distances);
            // Sort our distances
            distances.Sort();

            // if we're targeting from nothing, start at the beginning
            int index = 0;

            if (CurrentTarget != null)
            {
                // Find index of current target in sorted distances
                index = distances.IndexOf(currentTargetDistance);
                index = IntExtension.CycleIndex(index, distances.Count, forward ? 1 : -1);
            }

            // loop through distances to find a valid target
            for (int i = 0; i < distances.Count; i++)
            {
                _targetIndex = originalDistances.IndexOf(distances[index]);

                Target target = _validTargets[_targetIndex];
                if (CanTarget(target))
                    return target;

                index = IntExtension.CycleIndex(index, distances.Count, forward ? 1 : -1);
            }

            return null;
        }
    }
}