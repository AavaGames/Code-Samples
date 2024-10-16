using ANU.IngameDebug.Console;
using Assets.App.Scripts.Extensions;
using Assets.App.Scripts.Structs;
using FishNet;
using FishNet.CodeGenerating;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    public class Buff
    {
        public readonly SkillBuffData BuffData;

        public Buff(SkillBuffData buffData)
        {
            BuffData = buffData;
        }
    }

    public class TimedBuff : Buff
    {
        public float Duration;

        public TimedBuff(SkillBuffData buffData) : base(buffData)
        {
            Duration = buffData.Duration;
        }

        public TimedBuff(SkillBuffData buffData, uint startedTick) : base(buffData)
        {
            Duration = buffData.Duration - (float)InstanceFinder.TimeManager.TimePassed(startedTick, false);

            //Debug.Log("Timed Buff: " + buffData.Duration + " - " + (float)InstanceFinder.TimeManager.TimePassed(startedTick, false) + " = " + Duration);
        }
    }

    [DebugCommandPrefix("character.stats.buffs")]
    public class CharacterBuffs : NetworkBehaviour
    {
        private Dictionary<uint, Buff> _permanentBuffs = new Dictionary<uint, Buff>();
        private Dictionary<uint, TimedBuff> _timedBuffs = new Dictionary<uint, TimedBuff>();

        #region Buff SyncVars

        private static SyncTypeSettings SyncTypeSettings => new SyncTypeSettings()
        {
            WritePermission = WritePermission.ServerOnly,
            ReadPermission = ReadPermission.Observers,
            SendRate = 0,
            Channel = Channel.Reliable,
        };

        [AllowMutableSyncType]
        private SyncVar<float> _syncHealthRegeneration = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float HealthRegeneration => _syncHealthRegeneration.Value;
        [AllowMutableSyncType]
        private SyncVar<float> _syncManaRegeneration = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float ManaRegeneration => _syncManaRegeneration.Value;

        [AllowMutableSyncType]
        private SyncVar<float> _syncMoveSpeed = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float MoveSpeed => _syncMoveSpeed.Value;
        [AllowMutableSyncType] private SyncVar<float> _syncJumpHeight = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float JumpHeight => _syncJumpHeight.Value;

        [AllowMutableSyncType] private SyncVar<float> _syncHomingAccuracy = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float HomingAccuracy => _syncHomingAccuracy.Value;
        [AllowMutableSyncType] private SyncVar<float> _syncProjectileSpeed = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float ProjectileSpeed => _syncProjectileSpeed.Value;

        [AllowMutableSyncType]
        private SyncVar<int> _syncDamageFlat = new SyncVar<int>(0, SyncTypeSettings);
        public int DamageFlat => _syncDamageFlat.Value;
        [AllowMutableSyncType]
        [Tooltip("Damage is rounded to closest integer")]
        private SyncVar<float> _syncDamageMultiplier = new SyncVar<float>(1.0f, SyncTypeSettings);
        public float DamageMultiplier => _syncDamageMultiplier.Value;

        #endregion

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            if (IsServerInitialized)
            {
                TimeManager.OnTick += TimeManager_ServerOnTick;
            }
            else
            {
                TimeManager.OnTick += TimeManager_ClientOnlyOnTick;
            }
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            if (TimeManager != null)
            {
                TimeManager.OnTick -= TimeManager_ServerOnTick;
                TimeManager.OnTick -= TimeManager_ClientOnlyOnTick;
            }
        }

        [Client]
        private void TimeManager_ClientOnlyOnTick()
        {
            if (_timedBuffs.Count > 0)
            {
                foreach (var pair in _timedBuffs)
                {
                    if (pair.Value.Duration > 0)
                    {
                        pair.Value.Duration -= (float)TimeManager.TickDelta;
                        if (pair.Value.Duration < 0)
                        {
                            pair.Value.Duration = 0;
                        }
                    }
                }
            }
        }

        [Server]
        private void TimeManager_ServerOnTick()
        {
            if (_timedBuffs.Count > 0)
            {
                List<uint> buffsToRemove = new List<uint>();

                foreach (var pair in _timedBuffs)
                {
                    pair.Value.Duration -= (float)TimeManager.TickDelta;

                    if (pair.Value.Duration <= 0)
                        buffsToRemove.Add(pair.Key);
                }

                if (buffsToRemove.Count > 0)
                {
                    foreach (uint id in buffsToRemove)
                        ServerRemoveBuff(id);

                    buffsToRemove.Clear();
                }
            }
        }

        // create a separate character class that manages status effects with timers, etc
        // void Reset()

        [Server]
        private void ApplySkillBuffData(SkillBuffData buffData, bool negate = false)
        {
            float multiplier = negate ? -1.0f : 1.0f;

            _syncManaRegeneration.Value += buffData.ManaRegeneration * multiplier;

            _syncMoveSpeed.Value += buffData.MoveSpeed * multiplier;
            _syncJumpHeight.Value += buffData.JumpHeight * multiplier;

            _syncDamageFlat.Value += (int)(buffData.DamageFlat * multiplier);
        }

        private uint FindNewKey()
        {
            List<int> intKeys = new List<int>();
            foreach (uint key in _permanentBuffs.Keys)
            {
                intKeys.Add((int)key);
            }
            foreach (uint key in _timedBuffs.Keys)
            {
                intKeys.Add((int)key);
            }
            return (uint)IntExtension.FindNewKey(intKeys);
        }

        [Server]
        public void ServerAddBuff(SkillBuffData buffData)
        {
            uint id = FindNewKey();

            if (buffData.Type == SkillBuffData.BuffType.Timed)
            {
                _timedBuffs.Add(id, new TimedBuff(buffData));

                ObserverAddBuff(id, buffData, TimeManager.Tick);
            }
            else
            {
                _permanentBuffs.Add(id, new Buff(buffData));

                ObserverAddBuff(id, buffData);
            }

            ApplySkillBuffData(buffData);
        }

        // Updates new clients who join but doesn't update duration of buffs
        [ObserversRpc(BufferLast = true, ExcludeServer = true)]
        private void ObserverAddBuff(uint id, SkillBuffData buffData, uint startedTick = 0)
        {
            if (buffData.Type == SkillBuffData.BuffType.Timed)
            {
                if (_timedBuffs.ContainsKey(id))
                {
                    Debug.LogError("Client: Tried to add a timed buff that already exists!");
                    return;
                }

                _timedBuffs.Add(id, new TimedBuff(buffData, startedTick));
            }
            else
            {
                if (_permanentBuffs.ContainsKey(id))
                {
                    Debug.LogError("Client: Tried to add a permanent buff that already exists!");
                    return;
                }

                _permanentBuffs.Add(id, new Buff(buffData));
            }
        }

        [Server]
        public void ServerRemoveBuff(uint id)
        {
            SkillBuffData buffData;

            if (_timedBuffs.ContainsKey(id))
            {
                buffData = _timedBuffs[id].BuffData;

                _timedBuffs.Remove(id);
            }
            else if (_permanentBuffs.ContainsKey(id))
            {
                buffData = _permanentBuffs[id].BuffData;

                _permanentBuffs.Remove(id);
            }
            else
            {
                Debug.LogError($"Server: Tried to remove a buff id {id} that doesn't exist!");
                return;
            }

            ApplySkillBuffData(buffData, true);

            ObserverRemoveBuff(id);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserverRemoveBuff(uint id)
        {
            if (_timedBuffs.ContainsKey(id))
            {
                _timedBuffs.Remove(id);
            }
            else if (_permanentBuffs.ContainsKey(id))
            {
                _permanentBuffs.Remove(id);
            }
            else
            {
                Debug.LogError($"Client: Tried to remove a buff id {id} that doesn't exist!");
                return;
            }
        }

        #region debug

        [DebugCommand("set_ms")]
        private void DebugSetMS(float amount)
        {
            if (Owner == LocalConnection)
            {
                DebugServerSetMS(amount);
            }
        }

        [ServerRpc(RunLocally = true, RequireOwnership = true)]
        private void DebugServerSetMS(float amount)
        {
            _syncMoveSpeed.Value = amount;
        }

        #endregion
    }
}