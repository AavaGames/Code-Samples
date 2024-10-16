using ANU.IngameDebug.Console;
using Assets.App.Scripts.Combat;
using Assets.App.Scripts.Data.Skills;
using Assets.App.Scripts.Structs;
using DG.Tweening;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace Assets.App.Scripts.Characters
{
    [DebugCommandPrefix("character.stats")]
    public class CharacterStats : NetworkBehaviour, ICharacter
    {
        private Character _character;
        private CharacterData.StatsData CharacterDataStats => _character.Data.Stats;
        public CharacterBuffs Buffs { get; private set; }

        private Sequence _blinkingSequence;
        public float ImmunityBlinkingSpeed = 0.2f;

        private readonly SyncVar<bool> _syncImmune = new SyncVar<bool>();
        public bool Immune => _syncImmune.Value;

        // Stats
        private readonly SyncVar<int> _syncHealth = new SyncVar<int>();
        public int Health => _syncHealth.Value;
        private readonly SyncVar<int> _syncMaxHealth = new SyncVar<int>();
        public int MaxHealth => _syncMaxHealth.Value;

        private readonly SyncVar<int> _syncMana = new SyncVar<int>();
        public int Mana => _syncMana.Value;
        private readonly SyncVar<int> _syncMaxMana = new SyncVar<int>();
        public int MaxMana => _syncMaxMana.Value;

        // Server Only Timers
        private float _healthRegenTimer = 0.0f;
        private float _manaRegenTimer = 0.0f;

        private Coroutine _visualBlinkingCoroutine;

        // Actions / Events

        /// <summary>
        /// Action triggered when player is successfully launched and loses control
        /// </summary>
        public Action OnHitLaunch;

        [Tooltip("int Health")]
        public Action<int> OnHealthChange;
        [Tooltip("int Mana")]
        public Action<int> OnManaChange;

        public void Awake()
        {
            _character = GetComponent<Character>();
            Buffs = GetComponent<CharacterBuffs>();

            _syncHealth.OnChange += HealthChanged;
            _syncMana.OnChange += ManaChanged;

            _blinkingSequence = DOTween.Sequence();
            _blinkingSequence.AppendCallback(() => _character.Pawn.Mesh.SetActive(false))
                    .AppendInterval(ImmunityBlinkingSpeed)
                    .AppendCallback(() => _character.Pawn.Mesh.SetActive(true))
                    .AppendInterval(ImmunityBlinkingSpeed)
                    .SetLoops(-1); // Loop the sequence indefinitely
            _blinkingSequence.Pause();
        }

        private void OnDestroy()
        {
            _blinkingSequence.Kill();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;

            _syncMaxHealth.Value = _character.Data.Stats.StartingHealth;
            _syncHealth.Value = MaxHealth;

            _syncMaxMana.Value = _character.Data.Stats.StartingMana;
            _syncMana.Value = MaxMana;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
        }

        [Server]
        private void TimeManager_OnTick()
        {
            if (!_character.IsDead)
            {
                float delta = (float)InstanceFinder.TimeManager.TickDelta;

                if (Mana < MaxMana)
                {
                    _manaRegenTimer += delta * CharacterDataStats.ManaRegeneration * Buffs.ManaRegeneration;
                    if (_manaRegenTimer > 1.0f)
                    {
                        RestoreMana(1);
                        _manaRegenTimer -= 1.0f;
                    }
                }
                else
                    _manaRegenTimer = 0.0f;

                if (Health < MaxHealth)
                {
                    _healthRegenTimer += delta * CharacterDataStats.HealthRegeneration * Buffs.HealthRegeneration;
                    if (_healthRegenTimer > 1.0f)
                    {
                        Heal(1);
                        _healthRegenTimer -= 1.0f;
                    }
                }
                else
                    _healthRegenTimer = 0.0f;
            }
        }

        public void Death()
        {
            if (IsClientInitialized)
            {
                if (_visualBlinkingCoroutine != null)
                {
                    StopCoroutine(_visualBlinkingCoroutine);
                    EndBlinking();
                }
            }
        }

        public void Respawn()
        {
            if (IsServerInitialized)
            {
                _syncHealth.Value = _character.Data.Stats.RespawnHealth;
            }
        }

        [Server]
        public void Damage(HitInfo hitInfo)
        {
            _syncHealth.Value = Mathf.Clamp(_syncHealth.Value - hitInfo.Damage, 0, _syncMaxHealth.Value);

            if (_syncHealth.Value == 0)
            {
                // always launch player on death
                OwnerHitLaunch(Owner, hitInfo.HitPosition);
                _character.ServerDeath(hitInfo);
            }
        }

        [Server]
        public void Heal(int amount)
        {
            _syncHealth.Value = Mathf.Clamp(_syncHealth.Value + amount, 0, _syncMaxHealth.Value);
        }


        [Server]
        public void UseMana(int amount)
        {
            _syncMana.Value = Mathf.Clamp(_syncMana.Value - amount, 0, _syncMaxMana.Value);
        }

        [Server]
        public void RestoreMana(int amount)
        {
            _syncMana.Value = Mathf.Clamp(_syncMana.Value + amount, 0, _syncMaxMana.Value);
        }

        public bool HasMana(int amount)
        {
            return amount <= _syncMana.Value;
        }

        public void HealthChanged(int oldValue, int newValue, bool asServer)
        {
            OnHealthChange?.Invoke(_syncHealth.Value);
        }

        private void ManaChanged(int oldValue, int newValue, bool asServer)
        {
            OnManaChange?.Invoke(_syncMana.Value);
        }

        private bool CanHit(HitInfo hitInfo)
        {
            if (_syncImmune.Value)
                return false;

            // only check this if hit is coming from a skill
            if (hitInfo.SkillData != null)
            {
                if (_character.Pawn.Downed && !hitInfo.SkillHitData.CanHitDowned)
                    return false;
            }

            return true;
        }

        [Server]
        public void Hit(HitInfo hitInfo)
        {
            if (CanHit(hitInfo))
            {
                Damage(hitInfo);

                // we've already been launched
                if (_character.IsDead) return;

                // can't fly away if we're already downed
                if (_character.Pawn.Downed) return;

                if (hitInfo.Damage >= _character.Data.HitLaunchDamageThreshold)
                {
                    OwnerHitLaunch(Owner, hitInfo.HitPosition);
                }
                else
                {
                    OwnerSmallHitAnimation(Owner);
                }
            }
        }

        [TargetRpc(RunLocally = true)]
        public void OwnerHitLaunch(NetworkConnection owner, Vector3 hitPosition)
        {
            OnHitLaunch?.Invoke();

            _character.Pawn.LookAt(hitPosition);
            _character.Pawn.StateMachine.ImmediatelyChangeToState(CharacterStateMachine.State.Hit);
        }

        [TargetRpc(RunLocally = true)]
        public void OwnerSmallHitAnimation(NetworkConnection owner)
        {
            _character.Pawn.NetworkAnimator.Play("HitFlinch", 1);
        }

        [Server]
        public void EnterImmunity(float duration)
        {
            StartCoroutine(ImmunityToggle(duration));

            ObserverImmunity(duration);
        }

        [Server]
        private IEnumerator ImmunityToggle(float duration)
        {
            _syncImmune.Value = true;
            yield return new WaitForSeconds(duration);
            _syncImmune.Value = false;
        }

        [ObserversRpc]
        public void ObserverImmunity(float duration)
        {
            _visualBlinkingCoroutine = StartCoroutine(ImmunityBlinking(duration));
        }

        [Client]
        private IEnumerator ImmunityBlinking(float duration)
        {
            _blinkingSequence.Restart();

            yield return new WaitForSeconds(duration);

            EndBlinking();
        }

        [Client]
        private void EndBlinking()
        {
            _blinkingSequence.Pause();
            _character.Pawn.Mesh.SetActive(true);
        }

        #region Debug Console Commands

        [DebugCommand("max_mana")]
        private void DebugMaxMana()
        {
            if (Owner == LocalConnection)
            {
                DebugSetMana(99);
            }
        }

        [DebugCommand("set_mana")]
        private void DebugSetMana(int amount)
        {
            if (Owner == LocalConnection)
            {
                DebugServerSetMana(amount);
            }
        }

        [ServerRpc]
        private void DebugServerSetMana(int amount)
        {
            amount = Math.Clamp(amount, 0, 99);

            _syncMaxMana.Value = amount;
            _syncMana.Value = amount;
        }

        [ServerRpc]
        [DebugCommand("hit_small")]
        public void DebugHit()
        {
            Vector3 hitPosition = transform.position + (transform.forward * UnityEngine.Random.Range(-1f, 1f)) +
                (transform.right * UnityEngine.Random.Range(-1f, 1f));
            Vector3 hitNormal = transform.forward * -1;

            ProjectileSkillData skillData = CombatGlobals.SkillLibrary.GetSkill("Necro Orb") as ProjectileSkillData;

            HitInfo hitInfo = new HitInfo("Environment", _character,
                skillData, skillData.BaseHitData,
                hitPosition, hitPosition, hitNormal);

            Debug.Log(hitInfo.ToString());

            Hit(hitInfo);
        }

        [ServerRpc]
        [DebugCommand("hit")]
        public void DebugHitBig()
        {
            Vector3 hitPosition = transform.position + (transform.forward * UnityEngine.Random.Range(-1f, 1f)) +
                (transform.right * UnityEngine.Random.Range(-1f, 1f));
            Vector3 hitNormal = transform.forward * -1;

            ProjectileSkillData skillData = CombatGlobals.SkillLibrary.GetSkill("Magic Bullet") as ProjectileSkillData;

            HitInfo hitInfo = new HitInfo("Environment", _character,
                skillData, skillData.BaseHitData,
                hitPosition, hitPosition, hitNormal);

            Debug.Log(hitInfo.ToString());

            Hit(hitInfo);
        }
        #endregion

    }
}
