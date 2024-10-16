using Assets.App.Scripts.Characters.Pawn;
using Assets.App.Scripts.Data.Skills;
using Assets.App.Scripts.Extensions;
using Assets.App.Scripts.SkillObjects;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    public class CharacterCaster : NetworkBehaviour, ICharacter
    {
        private CharacterPawn _pawn;
        private CharacterBoneFollowerManager _boneFollowerManager;

        private SkillData CurrentSkill => _pawn.Character.SkillHandler.CurrentSkill;

        /// <summary>
        /// The current skill's objects
        /// Updated by owned SkillObject
        /// </summary>
        [SerializeField]
        [ReadOnly]
        private SkillObject[] _currentSkillObjects;
        public SkillObject[] CurrentSkillObjects => _currentSkillObjects;

        private void Awake()
        {
            _pawn = GetComponent<CharacterPawn>();
            _boneFollowerManager = GetComponent<CharacterBoneFollowerManager>();
        }

        public void Death()
        {

        }

        public void Respawn()
        {

        }

        #region Skill Objects

        /// <summary>
        /// Server spawns skill objects and sends them out deactivated
        /// </summary>
        [Server]
        public void SpawnSkillObjects()
        {
            SetupSkillObjectList();

            // iterate through all skill objects and spawn them depending on their settings
            for (int i = 0; i < CurrentSkill.SkillObjects.Count; i++)
            {
                SpawnSkillObject(i);
            }
        }

        /// <summary>
        /// Sets up the transform of the object container's created object
        /// </summary>
        /// <param name="currentObj">Instantiated game object from ObjectContainer Prefab</param>
        /// <param name="objectContainer"></param>
        /// <returns>Transform of the parent of the object</returns>
        private Transform SetupObjectContainerTransform(GameObject currentObj, SkillData.ObjectContainer objectContainer)
        {
            Transform parent = _pawn.transform; // PawnTransform.Root

            // set default pos + rot to Root
            currentObj.transform.SetParent(parent, false);
            currentObj.transform.ResetLocal();

            if (objectContainer.Parent == SkillData.ObjectContainer.PawnTransform.Mesh)
            {
                parent = _pawn.Mesh.transform;
            }
            else if (objectContainer.Parent == SkillData.ObjectContainer.PawnTransform.Bone)
            {
                Transform bone = _boneFollowerManager.FindBoneFollower(objectContainer.BoneName);
                if (bone == null)
                {
                    Debug.LogError(string.Format("Skill {0} cannot find bone '{1}' for its object ({2}).",
                    CurrentSkill.name, objectContainer.BoneName, objectContainer.Prefab));
                }
                else
                {
                    parent = bone;
                }
            }

            if (objectContainer.Parent != SkillData.ObjectContainer.PawnTransform.Root)
            {
                if (objectContainer.InWorldSpace)
                {
                    // only cares about position
                    currentObj.transform.position = parent.position;
                }
                else
                {
                    // fully parent and reset
                    currentObj.transform.SetParent(parent, false);
                    currentObj.transform.ResetLocal();
                }
            }

            // Detach from parent
            if (objectContainer.InWorldSpace)
                currentObj.transform.parent = null;

            return parent;
        }

        private void SpawnSkillObject(int index)
        {
            // checks here to confirm we can do this

            SkillData.ObjectContainer objectContainer = CurrentSkill.SkillObjects[index];

            GameObject currentObj = Instantiate(objectContainer.Prefab);
            currentObj.name = objectContainer.Prefab.name + "(Networked)";

            Transform parent = SetupObjectContainerTransform(currentObj, objectContainer);

            SkillObject skillObject = currentObj.GetComponent<SkillObject>();

            skillObject.Initialize(_pawn.Character, CurrentSkill,
                _pawn.Character.SkillHandler.CurrentSkillExecutionData, parent, index);

            if (skillObject.ServerInitialized)
            {
                Spawn(currentObj, Owner);
            }
            else
            {
                Debug.LogError($"Failed to initialize Skill Object ({skillObject.name})");
            }
        }

        public void SetupSkillObjectList()
        {
            if (_currentSkillObjects.Length != CurrentSkill.SkillObjects.Count)
                _currentSkillObjects = new SkillObject[CurrentSkill.SkillObjects.Count];
        }

        public void AddSkillObject(int index, SkillObject skillObject)
        {
            _currentSkillObjects[index] = skillObject;
        }

        [Server]
        private void ServerActivateSkillObject(int index)
        {
            try
            {
                _currentSkillObjects[index]?.ServerActivate();
            }
            catch
            {
                Debug.LogError($"Server: Skill Object at index {index} for {CurrentSkill.name} is null!");
            }
        }

        #region Animation Events

        /// <summary>
        /// Called by animation events.
        /// Requires an int parameter for index of Skill.SkillObject
        /// </summary>
        public void ActivateSkillObject(AnimationEvent animationEvent)
        {
            if (IsServerInitialized)
            {
                ServerActivateSkillObject(animationEvent.intParameter);
            }
        }

        /// <summary>
        /// Called by animation state exit event
        /// </summary>
        public void SkillAnimationStateEnd()
        {
            // if skill anim ends and it immedately goes into a new skill then it may be cleared

            SkillEnd();
        }

        /// <summary>
        /// Releases character from skill
        /// </summary>
        public void SkillEnd()
        {
            if (IsServerInitialized)
            {
                CurrentSkill.InvokeEvent(SkillData.SkillEvent.OnEnd, _pawn.Character);
            }

            if (IsOwner)
            {
                QueueEndSkill();
            }
            else if (IsServerInitialized) // skip client host
            {
                // Kicks character out of skill, basically always occurs
                // necessary in case they fail to reach the end of the animation on their side
                if (_pawn.StateMachine.CurrentState == CharacterStateMachine.State.Skill)
                {
                    _pawn.StateMachine.CurrentCharacterState.EndState();
                }
            }
        }

        public void QueueEndSkill()
        {
            _pawn.SkillQueued.End = true;
        }

        #endregion
        #endregion

        #region Visual Objects

        // TODO rework this to use the same system as skill objects
        private void CreateVisualObject(int index)
        {
            if (!HasVisualObject(index)) return;

            SkillData.ObjectContainer objectContainer = CurrentSkill.VisualObjects[index];

            GameObject prefab = objectContainer.Prefab;
            GameObject currentObj = Instantiate(prefab, transform, false);

            currentObj.name = objectContainer.Prefab.name + " (Local)";

            SetupObjectContainerTransform(currentObj, objectContainer);
        }

        private bool HasVisualObject(int index)
        {
            if (CurrentSkill == null)
            {
                Debug.LogError($"{_pawn.name} doesn't have the current skill to create visual objects!");
                return false;
            }

            if (index >= CurrentSkill.VisualObjects.Count)
            {
                Debug.LogError($"{CurrentSkill.name} is missing a visual object at index {index}! Is the index wrong or is the item missing?");
                return false;
            }

            if (CurrentSkill.VisualObjects[index].Prefab == null)
            {
                Debug.LogError($"{CurrentSkill.name} is missing a prefab for visual object at index {index}!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called by animation events.
        /// Requires an int parameter for index of location in VisualObject list
        /// </summary>
        public void CreateVisualObject(AnimationEvent animationEvent)
        {
            CreateVisualObject(animationEvent.intParameter);
        }

        #endregion
    }
}