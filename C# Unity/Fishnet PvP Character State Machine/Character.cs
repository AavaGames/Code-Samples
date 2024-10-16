using Assets.App.Scripts.Structs;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    public class Character : NetworkBehaviour, ICharacter
    {
        [Required]
        public CharacterData Data;
        private CharacterManager _levelManager;

        public Controller Controller { get; private set; }

        private readonly SyncVar<CharacterPawn> _syncPawn =
            new SyncVar<CharacterPawn>(new SyncTypeSettings(WritePermission.ClientUnsynchronized));
        public CharacterPawn Pawn => _syncPawn.Value;

        public CharacterSkillHandler SkillHandler { get; private set; }
        public CharacterStats Stats { get; private set; }

        private readonly SyncVar<bool> _syncIsDead = new SyncVar<bool>(true,
            new SyncTypeSettings(WritePermission.ClientUnsynchronized));
        public bool IsDead => _syncIsDead.Value;

        [Tooltip("SERVER ONLY: Character deadCharacter, HitInfo")]
        public Action<Character, HitInfo> OnServerDeath;

        [Tooltip("SERVER ONLY: Character character")]
        public Action<Character> OnServerRespawn;

        private void Awake()
        {
            Controller = GetComponentInParent<Controller>();

            SkillHandler = GetComponent<CharacterSkillHandler>();
            Stats = GetComponent<CharacterStats>();

            _levelManager = FindObjectOfType<CharacterManager>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            gameObject.name = $"{gameObject.name} ({OwnerId})";

            if (IsServerInitialized)
            {
                _levelManager.CheckIn(this);
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (_levelManager != null && _levelManager.isActiveAndEnabled)
                _levelManager.CheckOut(this);
        }

        [Server]
        public void ReceivePawn(CharacterPawn pawn)
        {
            _syncPawn.Value = pawn;

            _syncIsDead.Value = false;

            // Owner isn't initialized yet so we can skip as they'll receive up to date info on spawn
            if (!IsClientInitialized)
                return;

            // skip client host
            if (IsOwner && IsServerInitialized)
                return;

            OwnerReceivePawn(Owner, pawn);
        }

        [TargetRpc]
        private void OwnerReceivePawn(NetworkConnection owner, CharacterPawn pawn)
        {
            _syncPawn.Value = pawn;
            _syncIsDead.Value = false;
        }

        [Server]
        public void RemovePawn()
        {
            _syncPawn.Value = null;
            _syncIsDead.Value = true;
        }

        [Server]
        public void ServerDeath(HitInfo hitInfo)
        {
            _syncIsDead.Value = true;

            OnServerDeath?.Invoke(this, hitInfo);
            if (OnServerDeath == null)
                Debug.LogError($"{gameObject.name} has no OnDeath callback!");

            OwnerDeath(Owner);
        }

        [Server]
        public void ServerRespawn()
        {
            _syncIsDead.Value = false;

            OnServerRespawn?.Invoke(this);

            OwnerRespawn(Owner);
            Stats.EnterImmunity(Data.RespawnImmunityDuration);
        }

        [TargetRpc(RunLocally = true)]
        private void OwnerDeath(NetworkConnection owner) { Death(); }

        [TargetRpc(RunLocally = true)]
        private void OwnerRespawn(NetworkConnection owner) { Respawn(); }

        public void Death()
        {
            SkillHandler.Death();
            Stats.Death();
            Pawn.Death();
        }

        public void Respawn()
        {
            SkillHandler.Respawn();
            Stats.Respawn();
            Pawn.Respawn();
        }
    }
}