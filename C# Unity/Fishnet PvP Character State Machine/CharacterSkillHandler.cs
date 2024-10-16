using ANU.IngameDebug.Console;
using Assets.App.Data;
using Assets.App.Scripts.Characters.States.Abstract;
using Assets.App.Scripts.Combat;
using Assets.App.Scripts.Data.Skills;
using Assets.App.Scripts.Extensions;
using Assets.App.Scripts.SkillObjects;
using Assets.App.Scripts.Skills;
using Assets.App.Scripts.Structs.Enums;
using Assets.App.Scripts.Utilities;
using FishNet;
using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Assets.App.Scripts.Characters
{
    public struct SkillExecutionData
    {
        public SkillDirection CasterSkillDirection;
        public GameObject CurrentSkillTargetObject;
        /// <summary>
        /// Used for skill object alignment when there is no target.
        /// Always calculated, even when no target.
        /// </summary>
        public Vector3 ManualAimPosition;
    }

    [DebugCommandPrefix("character.skill_handler")]
    public class CharacterSkillHandler : NetworkBehaviour, ICharacter
    {
        private const int SKILL_ANIM_LAYER = 0;

        private Character _character;

        [Required]
        [SerializeField]
        private SkillMap<CharacterSkillHolder> _skillHolders = new SkillMap<CharacterSkillHolder>();
        public SkillMap<CharacterSkillHolder> SkillHolders => _skillHolders;


        [SerializeField]
        private bool _usingSkill = false;
        public bool UsingSkill => _usingSkill;

        [AllowMutableSyncType]
        private SyncVar<SkillData> _syncCurrentSkill = new SyncVar<SkillData>(new SyncTypeSettings()
        {
            SendRate = 0,
            WritePermission = WritePermission.ClientUnsynchronized,
            ReadPermission = ReadPermission.ExcludeOwner, // excluding owner because they lead
        });
        public SkillData CurrentSkill => _syncCurrentSkill.Value;

        /// <summary>
        /// used by server & non-owner clients to make sure they
        /// dont reuse old passed time if they haven't received new data yet
        /// </summary>
        private bool _newStartTick = false;
        private readonly SyncVar<uint> _syncCurrentSkillStartTick = new SyncVar<uint>(new SyncTypeSettings()
        {
            SendRate = 0,
            WritePermission = WritePermission.ServerOnly,
            ReadPermission = ReadPermission.ExcludeOwner, // owner does not care
        });
        private uint CurrentSkillStartTick => _syncCurrentSkillStartTick.Value;
        [ServerRpc(RequireOwnership = true)]
        private void OwnerSetCurrentSkillStartTick(uint tick)
        {
            _newStartTick = true;
            _syncCurrentSkillStartTick.Value = tick;
        }

        private readonly SyncVar<SkillExecutionData> _syncCurrentSkillExecutionData = new SyncVar<SkillExecutionData>(new SyncTypeSettings()
        {
            SendRate = 0,
            WritePermission = WritePermission.ClientUnsynchronized,
            ReadPermission = ReadPermission.Observers,
        });
        [ServerRpc(RequireOwnership = true, RunLocally = true)]
        private void OwnerSetCurrentSkillExecutionData(SkillExecutionData skillExecutionData)
        {
            _syncCurrentSkillExecutionData.Value = skillExecutionData;
        }
        public SkillExecutionData CurrentSkillExecutionData => _syncCurrentSkillExecutionData.Value;

        void Awake()
        {
            InstanceFinder.TimeManager.OnUpdate += TimeManager_OnUpdate;
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;

            _character = GetComponent<Character>();

            foreach (var pair in _skillHolders)
            {
                pair.Value.Initialize(_character, pair.Direction);
            }
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnUpdate -= TimeManager_OnUpdate;
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            _character.Stats.OnHitLaunch += ServerForceDespawnUnactiveSkillObjects;
        }

        private void TimeManager_OnUpdate() { }
        private void TimeManager_OnTick() { }

        public void Reconcile(bool usingSkill)
        {
            _usingSkill = usingSkill;
        }

        public void Death()
        {
            ClearSkill();
        }

        public void Respawn()
        {

        }

        #region Managing Skills

        [Server]
        public void ServerUpdateSkill(SkillDirection skillDirection, SkillData skillData)
        {
            if (CombatGlobals.SkillLibrary.HasSkill(skillData))
                _skillHolders[skillDirection].Skill.InsertSkillData(skillData);
        }

        [Server]
        public void ServerUpdateSkill(SkillDirection skillDirection, uint skillUniqueID)
        {
            SkillData skillData = CombatGlobals.SkillLibrary.GetSkill(skillUniqueID);
            if (skillData == null)
                return;
            _skillHolders[skillDirection].Skill.InsertSkillData(skillData);
        }

        [Server]
        private void ServerUpdateSkill(SkillDirection skillDirection, int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= CombatGlobals.SkillLibrary.Skills.Count)
                return;
            SkillData skillData = CombatGlobals.SkillLibrary.Skills[skillIndex];
            if (skillData == null)
                return;

            _skillHolders[skillDirection].Skill.InsertSkillData(skillData);
        }

        [Server]
        private void ServerUpdateSkill(SkillDirection skillDirection, string skillName)
        {
            SkillData skillData = CombatGlobals.SkillLibrary.GetSkill(skillName);
            if (skillData == null)
                return;
            _skillHolders[skillDirection].Skill.InsertSkillData(skillData);
        }

        [Server]
        public void ServerRemoveSkill(SkillDirection skillDirection)
        {
            _skillHolders[skillDirection].Skill.RemoveSkill();
        }

        [Server]
        public void ServerEraseSkill(SkillDirection skillDirection)
        {
            _skillHolders[skillDirection].Skill.EraseSkill();
        }


#if DEBUG // debug console commands 

        [DebugCommand("skill_index")]
        [ServerRpc(RequireOwnership = true)]
        private void DebugOwnerUpdateSkill(SkillDirection skillDirection, int skillIndex)
        {
            ServerUpdateSkill(skillDirection, skillIndex);
        }

        [DebugCommand("skill")]
        [ServerRpc(RequireOwnership = true)]
        private void DebugOwnerUpdateSkill(SkillDirection skillDirection, string skillName)
        {
            ServerUpdateSkill(skillDirection, skillName);
        }

        [DebugCommand("erase_skill")]
        [ServerRpc(RequireOwnership = true)]
        private void DebugOwnerEraseSkill(SkillDirection skillDirection)
        {
            ServerEraseSkill(skillDirection);
        }
#endif

        #endregion

        #region Skill Execution

        public void CheckSkillQueue(CharacterPawn.MoveData md)
        {
            if (!md.SkillQueued.End)
            {
                if (md.SkillQueued.North)
                    TryExecuteSkill(SkillDirection.North);
                if (md.SkillQueued.East)
                    TryExecuteSkill(SkillDirection.East);
                if (md.SkillQueued.South)
                    TryExecuteSkill(SkillDirection.South);
                if (md.SkillQueued.West)
                    TryExecuteSkill(SkillDirection.West);
            }
        }

        private void TryExecuteSkill(SkillDirection skillDirection)
        {
            // Never predict skill execution
            // Confirming whether this every occurs, should probably be in CheckSkillQueue()
            ReplicateState state = _character.Pawn.CurrentReplicateState;
            if (state == ReplicateState.CurrentFuture || state == ReplicateState.ReplayedFuture)
            //|| state == ReplicateState.CurrentPredicted || state == ReplicateState.ReplayedPredicted) // not currently used by fishnet
            {
                Debug.LogWarning("Client was attempting to predict into skill");
                return;
            }

            if (CanExecuteSkill(skillDirection))
            {
                Skill skill = _skillHolders[skillDirection].Skill;

                if (IsServerInitialized)
                {
                    // TODO rework skill container to work with run local RPCs
                    // so that player gets instant UI feedback on casting
                    // PROBLEM: might take mana away during reconciliation
                    skill.Casted();
                }

                ExecuteSkill(skill.SkillData, skillDirection);
            }
        }

        [SerializeField]
        [BoxGroup("Debug")]
        private bool _verboseSkillLogging = false;

        private bool CanExecuteSkill(SkillDirection skillDirection)
        {
            if (_verboseSkillLogging && !InstanceFinder.PredictionManager.IsReconciling)
                Debug.Log($"---- Skill {skillDirection} ----");

            if (_usingSkill)
            {
                if (_verboseSkillLogging && !InstanceFinder.PredictionManager.IsReconciling)
                    Debug.Log("Still using a skill.");
                return false;
            }

            Skill skill = _skillHolders[skillDirection].Skill;

            if (skill.SkillData == null)
            {
                if (_verboseSkillLogging && !InstanceFinder.PredictionManager.IsReconciling)
                    Debug.Log("No skill to use in " + skillDirection + " direction.");

                return false;
            }

            if (_character.Pawn.CurrentMoveData.Mana < skill.Cost.GetCost(_character.Stats.MaxMana))
            {
                if (_verboseSkillLogging && !InstanceFinder.PredictionManager.IsReconciling)
                    Debug.Log($"Not enough mana ({_character.Pawn.CurrentMoveData.Mana}/{skill.Cost.GetCost(_character.Stats.MaxMana)}) to use {skill.Name}.");

                return false;
            }

            bool isGroundState = _character.Pawn.CurrentCharacterState as CharacterStateGroundMove != null;
            bool isAirState = _character.Pawn.CurrentCharacterState as CharacterStateAir != null;

            if (isAirState && !skill.SkillData.CanUseInAir)
            {
                if (_verboseSkillLogging && !InstanceFinder.PredictionManager.IsReconciling)
                    Debug.Log($"Cannot use {skill.Name} in air state {_character.Pawn.CurrentState}.");

                return false;
            }

            if (!isGroundState && !isAirState)
            {
                if (_verboseSkillLogging && !InstanceFinder.PredictionManager.IsReconciling)
                    Debug.Log($"Cannot use {skill.Name} in non-move state {_character.Pawn.CurrentState}.");

                return false;
            }

#if DEBUG
            if (!_character.Pawn.NetworkAnimator.HasState(skill.SkillData.AnimationClip.name, 0)) // sanity check
            {
                Debug.LogError($"This animator has no animation called {skill.SkillData.AnimationClip.name}!");
                return false;
            }
#endif

            // TODO investigate further interactions between casting on client but not server
            // Issue: https://github.com/SwordSlappers/ProjectCardium/issues/31
            if (IsServerInitialized && !IsOwner && !_newStartTick)
            {
                Debug.LogError("Server is trying to execute a skill without client having sent execution data first!");
                return false;
            }

            return true;
        }

        private void ExecuteSkill(SkillData skill, SkillDirection skillDirection)
        {
            _usingSkill = true;
            _syncCurrentSkill.Value = skill;

            // TODO choose skill state depending on skill type melee, projectile, etc
            _character.Pawn.StateMachine.ImmediatelyChangeToState(CharacterStateMachine.State.Skill);

            // owner cannot enter when reconciling
            // clients can
            // server will break things if they are hosting and predicting
            if (InstanceFinder.PredictionManager.IsReconciling)
                return;

            string time = $"{System.DateTime.Now.Second}s {System.DateTime.Now.Millisecond}ms";

            if (IsOwner)
            {
                if (!InstanceFinder.PredictionManager.IsReconciling)
                {
                    //Debug.Log("Skill Started Tick: " + TimeManager.Tick + " Real Time: " + time);
                    OwnerSendSkillExecutionData(skillDirection);
                }
            }

            if (IsServerInitialized)
            {
                //Debug.Log($"SERVER: {CurrentSkill.name} executed by {gameObject.name} @ {transform.position}");

                // Must spawn objects before playing animations as they may activate immediately
                _character.Pawn.Caster.SpawnSkillObjects();
            }

            if (IsOwner)
            {
                _character.Pawn.Caster.SetupSkillObjectList();

                PlaySkillAnimation(skill);
            }
            else
            {
                // TODO need to confirm if this syncvar is passed properly to clients

                if (!_newStartTick) // TODO remove once we confirm this is all working
                    Debug.LogError("Has not received new execution data yet, defaulting passed time to 0");

                // must get tick sent from owner character
                uint startTick = _newStartTick ? CurrentSkillStartTick : 0;
                _newStartTick = false; // flag so we dont use it again, cant set 0 because of syncvar

                float passedTime = (float)TimeManager.TimePassed(startTick, false);
                // Cap it to a maximum ping, so high ping players don't ruin the game
                passedTime = Mathf.Min(GameplaySettingsData.MAX_PING_MS, passedTime);

                //Debug.Log($"Non-Owner Skill Started Tick: {InstanceFinder.TimeManager.Tick} Owner Tick: {startTick}" + "Real Time: " + time
                //    + $"\nPassed Time: {passedTime}s True Time: {(float)TimeManager.TimePassed(startTick, false)}s");

                PlaySkillAnimation(skill, passedTime);
            }
        }

        private void OwnerSendSkillExecutionData(SkillDirection skillDirection)
        {
            GameObject currentTarget = null;
            if (_character.Pawn.CurrentMoveData.Targeting)
                currentTarget = _character.Pawn.Targeter.CurrentTarget.gameObject;

            // Start tick needs to be sent to server + other clients
            OwnerSetCurrentSkillStartTick(TimeManager.Tick);

            // Only server needs this info
            OwnerSetCurrentSkillExecutionData(new SkillExecutionData()
            {
                CasterSkillDirection = skillDirection,
                CurrentSkillTargetObject = currentTarget,
                ManualAimPosition = FindManualAimPosition(),
            });
        }

        private Vector3 FindManualAimPosition()
        {
            Vector3 manualAimPos;
            GameplaySettingsData settings = CombatGlobals.GameplaySettings;

            Ray ray = _character.Pawn.Camera.MainCamera.ScreenPointToRay
                (_character.Pawn.UI.Reticle.ManualReticlePosition);

            if (gameObject.scene.GetPhysicsScene().Raycast(ray.origin, ray.direction,
                out RaycastHit hitInfo, settings.SkillRange.Long.Maximum, settings.ReticleRayLayers))
            {
                manualAimPos = hitInfo.point;
            }
            else
            {
                manualAimPos = ray.GetPoint(settings.SkillRange.Long.Maximum);
            }

            // Add a minimum distance so it doesn't fly directly up if the character is hugging a wall
            if (Vector3.Distance(_character.Pawn.transform.position, manualAimPos) <= settings.ReticleRayDistanceMinimum)
                manualAimPos += ray.direction * settings.ReticleRayDistanceMinimum;

            //Debug.DrawLine(ray.origin, freeTargetPos, Color.magenta, 5f);

            return manualAimPos;
        }


        public bool DebugServerIgnorePassedTime = false;

        /// <summary>
        /// Plays the Skill Animation unless reconciling
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="passedTimeFromClient"></param>
        private void PlaySkillAnimation(SkillData skill, float passedTimeFromClient = 0)
        {
            NetworkAnimator networkAnimator = _character.Pawn.NetworkAnimator;

            if (InstanceFinder.PredictionManager.IsReconciling) // server can reconcile while client-host
                return;

            if (!IsServerInitialized)
            {
                // don't restart animation if we're already playing the animation
                if (networkAnimator.Animator.GetCurrentAnimatorStateInfo(SKILL_ANIM_LAYER).IsName(skill.AnimationClip.name)
                   || networkAnimator.Animator.GetNextAnimatorStateInfo(SKILL_ANIM_LAYER).IsName(skill.AnimationClip.name))
                {
                    if (IsOwner)
                    {
                        Debug.Log("Stopping Owner from retriggering Skill Animation again!");
                    }
                    else
                    {
                        Debug.Log("Stopping Non-Owner Client from retriggering Skill Animation again!");
                    }
                    return;
                }
            }

            if (!IsOwner && passedTimeFromClient != 0 && !DebugServerIgnorePassedTime)
            {
                // Start playing at passed time
                networkAnimator.CrossFadeInFixedTime(skill.AnimationClip.name,
                    _character.Data.SkillFixedTransitionTime, SKILL_ANIM_LAYER, passedTimeFromClient);

                // Check if we missed any events
                foreach (var animEvent in skill.AnimationClip.events)
                {
                    // NOTE: Animation events are ONLY sent to pawn, anim events should not be for its children
                    if (animEvent.time < passedTimeFromClient)
                        _character.Pawn.SendMessage(animEvent.functionName, animEvent, animEvent.messageOptions);
                }

                return;
            }

            networkAnimator.CrossFadeInFixedTime(skill.AnimationClip.name,
                    _character.Data.SkillFixedTransitionTime, SKILL_ANIM_LAYER);
        }

        #endregion

        public void ClearSkill()
        {
            //Debug.Log("Skill cleared!");

            _usingSkill = false;
            _syncCurrentSkill.Value = null;
        }

        /// <summary>
        /// Used when character's skill is canceled (e.g hit launched) and the skill object isn't active.
        /// </summary>
        [Server]
        private void ServerForceDespawnUnactiveSkillObjects()
        {
            if (_usingSkill)
            {
                foreach (SkillObject skillObj in _character.Pawn.Caster.CurrentSkillObjects)
                {
                    if (!skillObj.Active)
                    {
                        Despawn(skillObj.gameObject);
                    }
                }
            }
        }
    }
}