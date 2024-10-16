using ANU.IngameDebug.Console;
using Assets.App.Scripts.Characters.Pawn;
using Assets.App.Scripts.Characters.States.Abstract;
using Assets.App.Scripts.Combat;
using Assets.App.Scripts.UI.Characters;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    public class CharacterPawn : NetworkBehaviour, ICharacter
    {
        #region Components
        public Character Character { get; private set; }
        public CharacterCaster Caster { get; private set; }
        public CharacterStateMachine StateMachine { get; private set; }
        public CharacterCamera Camera { get; private set; }
        public CharacterTargeter Targeter { get; private set; }
        public CharacterInteractor CharacterInteractor { get; private set; }
        public CharacterUI UI { get; private set; }
        public Target Target { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public Animator Animator { get; private set; }
        public NetworkAnimator NetworkAnimator { get; private set; }
        public CharacterHurtbox[] Hurtboxes { get; private set; }

        [Required]
        public GameObject Mesh;
        [Required, Tooltip("The core of the mesh. (e.g. humanoid chest)")]
        public Transform Core;

        #endregion

        // removing Active causes rotation to not be updated on start for some reason
        public bool Active { get; private set; }
        [ReadOnly] public float VerticalVelocity = 0.0f;

        /// <summary>
        /// The forward / backward velocity added to Character movement.
        /// </summary>
        /// <remarks>
        /// Can transition to Vector3 for physics based interactions later on.
        /// </remarks>
        [ReadOnly] public float HorizontalVelocity = 0.0f;

        [ReadOnly] public float GoalRotation = 0.0f;
        [ReadOnly] public bool Grounded = false;
        [Tooltip("Used by states to do various time based things like lockouts or lerps. (e.g. landing lockout, standing lerp)")]
        [ReadOnly] public float StateTimer = 0.0f;

        private readonly SyncVar<bool> _syncDowned = new SyncVar<bool>();
        public bool Downed => _syncDowned.Value;

        public CharacterStateMachine.State CurrentState => StateMachine.CurrentState;
        public CharacterState CurrentCharacterState => StateMachine.CurrentCharacterState;

        [Header("Animation")]
        [Tooltip("Rate at which speed animation changes, the proportion of source remaining after one second")]
        public float AnimSpeedDampSmoothing = 0.001f;
        [ReadOnly] public float AnimSpeedBlend = 0.0f;
        // Animator Variable IDs
        public int AnimIDSpeed { get; private set; }
        public int AnimIDGrounded { get; private set; }
        public int AnimIDJump { get; private set; }
        public int AnimIDFreeFall { get; private set; }
        public int AnimIDMotionSpeed { get; private set; }
        public int AnimIDLanded { get; private set; }
        public int AnimIDKnockedDown { get; private set; }
        public int AnimIDGetUpLerp { get; private set; }


        // The owners latest move data, used for smoothing movement in OnUpdate
        private MoveData _ownerMoveData;
        // The current move data being used. Includes replays
        public MoveData CurrentMoveData { get; private set; }
        public ReplicateState CurrentReplicateState { get; private set; }

        #region Input Queue, MoveData and ReconcileData

        public SkillQueue SkillQueued = new SkillQueue(); // must be public changed in caster

        public struct SkillQueue
        {
            public bool North;
            public bool East;
            public bool South;
            public bool West;
            [Tooltip("The skill (animation) has ended")]
            public bool End;

            public bool AnyQueued()
            {
                return North || East || South || West;
            }
        }

        public struct MoveData : IReplicateData
        {
            public bool Active;

            public Vector2 Move;
            public bool Jump;

            public Vector3 CameraEulers; // NOTE: No used for Z axis, maybe make it to variables

            public bool Targeting;
            public Vector3 TargetPosition;

            public int Mana;

            public SkillQueue SkillQueued;

            public void Dispose() { }
            private uint _tick;
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;

            public override string ToString()
            {
                return string.Format("Move: {0}, Jump: {1}, CameraEulers: {2}, Targeting: {3}, TargetPosition: {4}, Mana: {5}, SkillQueued: {6}, CurrentSkill: {7}",
                                       Move, Jump, CameraEulers, Targeting, TargetPosition, Mana, SkillQueued);
            }
        }

        //ReconcileData for Reconciliation
        public struct ReconcileData : IReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public float VerticalVelocity;
            public float HorizontalVelocity;
            public bool Grounded;
            public float GoalRotation;
            public float StateTimer;

            public CharacterStateMachine.State CurrentState;
            public CharacterStateMachine.State PreviousState;

            public bool UsingSkill;

            public override string ToString()
            {
                return string.Format("Position: {0}, Rotation: {1}, VerticalVelocity: {2}, HorizontalVelocity: {3}, Grounded: {4}, GoalRotation: {5}, StateTimer: {6}, CurrentState: {7}, PreviousState: {8}, UsingSkill: {9}",
                    Position, Rotation, VerticalVelocity, HorizontalVelocity, Grounded, GoalRotation, StateTimer, CurrentState, PreviousState, UsingSkill);
            }

            public ReconcileData(Vector3 position, Quaternion rotation, float verticalVelocity, float horizontalVelocity,
                bool grounded, CharacterStateMachine.State currentState, CharacterStateMachine.State previousState,
                float goalRotation, float stateTimer, bool usingSkill)
            {
                Position = position;
                Rotation = rotation;
                VerticalVelocity = verticalVelocity;
                HorizontalVelocity = horizontalVelocity;

                Grounded = grounded;
                CurrentState = currentState;
                PreviousState = previousState;
                GoalRotation = goalRotation;
                StateTimer = stateTimer;

                UsingSkill = usingSkill;

                _tick = 0;
            }

            public void Dispose() { }
            private uint _tick;
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        #endregion

        private void Awake()
        {
            UI = GetComponentInChildren<CharacterUI>();
            Caster = GetComponent<CharacterCaster>();
            Camera = GetComponent<CharacterCamera>();
            Targeter = GetComponent<CharacterTargeter>();
            CharacterInteractor = GetComponent<CharacterInteractor>();
            StateMachine = GetComponent<CharacterStateMachine>();
            Animator = GetComponent<Animator>();
            NetworkAnimator = GetComponent<NetworkAnimator>();
            CharacterController = GetComponent<CharacterController>();
            Hurtboxes = GetComponentsInChildren<CharacterHurtbox>();

            Target = GetComponentInChildren<Target>();
            if (Target == null)
                Debug.LogError("Pawn has no Target!");

            AssignAnimationIDs();

#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            foreach (NetworkObject nob in GetComponentsInChildren<NetworkObject>()) // sanity check
            {
                if (nob.GetInitializeOrder() < NetworkObject.GetInitializeOrder())
                    Debug.LogError("PAWN: Nested NetworkObject must have the same or higher Initialize Order as Pawn");
            }
#endif
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            Character = GetComponentInParent<Character>();
            gameObject.name = $"CharacterPawn ({OwnerId})";

            CharacterController.enabled = true;

            // StateMachine requires animation ids to be assigned first
            StateMachine.Initialize(this);

            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
            InstanceFinder.TimeManager.OnUpdate += TimeManager_OnUpdate;

            Activate();

            // Wait till spawned to set default rotation
            Rotate(transform.rotation.eulerAngles.y);
        }


        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
                InstanceFinder.TimeManager.OnUpdate -= TimeManager_OnUpdate;
            }
        }

        #region Update and Reconciliation

        private void TimeManager_OnUpdate()
        {
            if (IsOwner)
            {
                // Input Queue for next tick
                if (Character.Controller.Input.BaseMap.FindAction("SkillNorth").WasPressedThisFrame())
                    SkillQueued.North = true;
                if (Character.Controller.Input.BaseMap.FindAction("SkillEast").WasPressedThisFrame())
                    SkillQueued.East = true;
                if (Character.Controller.Input.BaseMap.FindAction("SkillSouth").WasPressedThisFrame())
                    SkillQueued.South = true;
                if (Character.Controller.Input.BaseMap.FindAction("SkillWest").WasPressedThisFrame())
                    SkillQueued.West = true;

                // Smoothly updates the movement with the latest clients move data received on the last server tick
                //UpdateWithData(_ownerMoveData, Time.deltaTime);
            }
        }

        private void TimeManager_OnTick()
        {
            Move(BuildMoveData());
        }

        // Called after a tick occurs. Reconcile here if other forces influence movement
        private void TimeManager_OnPostTick()
        {
            if (IsServerInitialized)
            {
                CreateReconcile();
            }
        }

        public override void CreateReconcile()
        {
            base.CreateReconcile();

            ReconcileData rd = new ReconcileData(
                transform.position, transform.rotation,
                VerticalVelocity, HorizontalVelocity,
                Grounded, CurrentState, StateMachine.PreviousState,
                GoalRotation, StateTimer,
                Character.SkillHandler.UsingSkill
            );
            Reconciliation(rd); // Sends a reconcile off to the player
        }

        private void UpdateWithData(MoveData md, ReplicateState state, float delta)
        {
            CurrentMoveData = md;

            if (md.SkillQueued.End)
            {
                StateMachine.EndSkillState();
                md.SkillQueued.End = false; // needed if updating in OnUpdate
            }
            Character.SkillHandler.CheckSkillQueue(md);

            StateMachine.MoveWithData(md, delta);
        }

        // Reconciles any variables that may affect movement
        [Reconcile]
        private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            VerticalVelocity = rd.VerticalVelocity;
            HorizontalVelocity = rd.HorizontalVelocity;

            // States
            Grounded = rd.Grounded;
            GoalRotation = rd.GoalRotation;
            StateTimer = rd.StateTimer;

            StateMachine.Reconcile(rd.CurrentState, rd.PreviousState);

            // Skills
            Character.SkillHandler.Reconcile(rd.UsingSkill);
        }

        [Replicate]
        private void Move(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            CurrentMoveData = md;
            CurrentReplicateState = state;

            // Used to smooth move in update
            //if (IsOwner && state == ReplicateState.CurrentCreated)
            //{
            //    _ownerMoveData = md;
            //    return;
            //}

            UpdateWithData(md, state, (float)base.TimeManager.TickDelta);
        }

        private MoveData BuildMoveData()
        {
            if (!IsOwner)
                return default;

            bool targeting = Targeter.CanTarget(Targeter.CurrentTarget);

            // Checks queued inputs and saves them
            MoveData md = new MoveData()
            {
                Move = Character.Controller.Input.Move,
                Jump = Character.Controller.Input.Actions["Jump"].IsPressed(),
                //MovementAbility = Character.Controller.Input.Actions["MovementAbility"].IsPressed
                // this is required to know which direction we are moving in
                CameraEulers = Camera.MainCamera.transform.eulerAngles,
                // this is required to know which direction to face
                Targeting = targeting,
                // sets to Vector3.zero if it needs to be ignored, Vector3 cannot be null
                TargetPosition = targeting ? Targeter.CurrentTarget.transform.position : Vector3.zero,
                Mana = Character.Stats.Mana,
                SkillQueued = SkillQueued,
            };
            md.Active = Active;

            // Reset queued input
            SkillQueued = new SkillQueue();

            return md;
        }

        #endregion

        public void Death()
        {
            if (IsServerInitialized)
            {
                Target.SetTargetableBy(Target.TargetableByEnum.None);
            }

            Camera.Death();
            Caster.Death();
            StateMachine.Death();
        }

        public void Respawn()
        {
            if (IsServerInitialized)
            {
                ServerTeleportToSpawn();
                Target.SetTargetableBy(Target.TargetableByEnum.All);
            }

            NetworkAnimator.Play("Locomotion");

            Camera.Respawn();
            Caster.Respawn();
            StateMachine.Respawn();
        }

        // should only be used on owner and server
        public void Activate()
        {
            Active = true;
            StateMachine.enabled = Active;
        }

        public void Deactivate()
        {
            Active = false;
            StateMachine.enabled = Active;
        }

        private void AssignAnimationIDs()
        {
            AnimIDSpeed = Animator.StringToHash("Speed");
            AnimIDGrounded = Animator.StringToHash("Grounded");
            AnimIDJump = Animator.StringToHash("Jump");
            AnimIDFreeFall = Animator.StringToHash("FreeFall");
            AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            AnimIDLanded = Animator.StringToHash("Landed");
            AnimIDKnockedDown = Animator.StringToHash("KnockedDown");
            AnimIDGetUpLerp = Animator.StringToHash("GetUpLerp");
        }

        /// <summary>
        /// Sets vertical velocity to 0
        /// </summary>
        public void ResetVerticalVelocity()
        {
            VerticalVelocity = 0.0f;
        }

        /// <summary>
        /// Sets horizontal velocity to 0
        /// </summary>
        public void ResetHorizontalVelocity()
        {
            HorizontalVelocity = 0.0f;
        }

        /// <summary>
        /// Sets both vertical and horizontal velocity to 0
        /// </summary>
        public void ResetVelocity()
        {
            ResetVerticalVelocity();
            ResetHorizontalVelocity();
        }

        /// <summary>
        /// Rotates the character to face the y rotation given.
        /// </summary>
        /// <param name="yRotation"></param>
        public void Rotate(float yRotation)
        {
            GoalRotation = yRotation;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        public void LookAt(Vector3 targetPosition)
        {
            Vector3 directionToTarget = targetPosition - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);
            float rotation = rotationToTarget.eulerAngles.y;
            Rotate(rotation);
        }

        [Server]
        public void SetDowned(bool isDowned)
        {
            _syncDowned.Value = isDowned;
        }

        [DebugCommand("stuck")]
        [Client]
        private void CharacterStuck()
        {
            if (IsOwner)
            {
                ServerDebugStuck();
            }
        }

        // TODO remove later, for debug purposes. This can be abused by hackers
        [ServerRpc]
        private void ServerDebugStuck()
        {
            ServerTeleportToSpawn();
        }

        /// <summary>
        /// Should only be called inside Move()
        /// </summary>
        [TargetRpc(RunLocally = true)]
        private void OwnerTeleport(NetworkConnection owner, Vector3 position, float yRotation)
        {
            ClearReplicateCache();

            CharacterController.enabled = false;

            transform.position = position;
            Rotate(yRotation);

            CharacterController.enabled = true;

            //reset camera
            Camera.ResetCameraRotation(true);
        }

        [Server]
        public void ServerTeleportToSpawn()
        {
            // Our parent (Character) is our spawn point since they do not move
            OwnerTeleport(Owner, transform.parent.position, transform.parent.rotation.eulerAngles.y);
        }

        [Server]
        public void ServerTeleport(Vector3 position, float yRotation)
        {
            OwnerTeleport(Owner, position, yRotation);
        }
    }
}
