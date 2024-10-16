using Assets.App.Scripts.Data.Skills;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    /// <summary>
    /// Add's all Skill animation clips in SkillLibrary to Character's Animator Controller
    /// </summary>
    /// <remarks>
    /// By Unity's limitation it isn't possible to copy the base controller over to the new controller and keep references. 
    /// A new GUID is always formed with AssetDatabase.CopyAsset. And EditorUtility.CopySerialized creates a pointer-like file with Animator Controllers.
    /// NOTE: A potential solution would be searching through the used controller and erasing all states that arent in Base controller.
    /// </remarks>
    public class CharacterAnimatorControllerCreator : IPreprocessBuildWithReport
    {
        private static CharacterAnimatorControllerSettings _settings;

        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            CreateAnimatorController();
        }

        private static void GetSettings()
        {
            var settingsGUIDs = AssetDatabase.FindAssets("CharacterAnimatorControllerSettings t:SerializedScriptableObject");

            if (settingsGUIDs.Length == 0)
            {
                Debug.LogError("CharacterAnimatorControllerSettings not found! Asset must be named 'CharacterAnimatorControllerSettings'!");
                return;
            }
            else if (settingsGUIDs.Length > 1)
            {
                Debug.LogError("Multiple CharacterAnimatorControllerSettings found! This is not supported.\n" + settingsGUIDs.ToString());
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(settingsGUIDs[0]);
            _settings = AssetDatabase.LoadAssetAtPath<CharacterAnimatorControllerSettings>(path);
        }

        [MenuItem("Cardium/Utility/Create Character Animator Controller")]
        private static void CreateAnimatorController()
        {
            GetSettings();
            if (_settings == null) return;

            if (_settings.BaseController == null || _settings.PrefabsToUpdate.Count < 1)
            {
                Debug.LogError("CharacterAnimatorControllerSettings not set up correctly! Please assign all fields.");
                return;
            }

            AnimatorController controller = CreateController();

            var rootStateMachine = controller.layers[0].stateMachine;
            // Creates exit transition to layer 0 default state, which should be a Locomotion blend
            var locomotionState = rootStateMachine.defaultState;

            SkillData[] skillData = Resources.LoadAll<SkillData>("");

            int yOffset = 0;
            Vector3 basePosition = rootStateMachine.states[0].position;

            foreach (SkillData skill in skillData)
            {
                if (skill.AnimationClip == null)
                {
                    Debug.LogWarning("Skill " + skill.name + " has no animation clip assigned!");
                    continue;
                }

                var state = rootStateMachine.AddState(skill.AnimationClip.name, new Vector3(1000, yOffset) + basePosition);
                state.motion = skill.AnimationClip;
                state.tag = TagManager.SKILL_ANIMATION_STATE;

                // Add state behavior
                state.AddStateMachineBehaviour<SkillStateBehavior>();

                float exitDuration = _settings.SkillStateExitTransitionDuration;
                if (_settings.SkillStateExitTransitionDurationOverrides.ContainsKey(skill.AnimationClip.name))
                {
                    exitDuration = _settings.SkillStateExitTransitionDurationOverrides[skill.AnimationClip.name];
                }

                float exitTime = (skill.AnimationClip.length - exitDuration) / skill.AnimationClip.length;

                // Add exit transition to Locomotion state
                var exitTransition = state.AddExitTransition();
                exitTransition.destinationState = locomotionState;
                exitTransition.hasExitTime = true;
                exitTransition.exitTime = exitTime;
                // fixed second duration
                exitTransition.hasFixedDuration = true;
                exitTransition.duration = exitDuration;

                yOffset += 50;
            }

            // update references
            foreach (GameObject prefab in _settings.PrefabsToUpdate)
            {
                Animator animator = prefab.GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning("Prefab " + prefab.name + " has no Animator component!");
                    continue;
                }

                animator.runtimeAnimatorController = controller;
                PrefabUtility.SavePrefabAsset(prefab);
            }

            Debug.Log("Character Animator Controller Created!");
        }

        private static AnimatorController CreateController()
        {
            //get the path of the base controller
            string basePath = AssetDatabase.GetAssetPath(_settings.BaseController);
            // find the name of the file for the basePath
            string baseControllerName = System.IO.Path.GetFileName(basePath);
            // replace base name with used name
            string usedPath = basePath.Replace(baseControllerName, _settings.UsedControllerName + ".controller");

            AssetDatabase.CopyAsset(basePath, usedPath);
            AssetDatabase.Refresh();

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(usedPath);

            if (controller == null)
            {
                Debug.LogError("Failed to copy base controller to used controller!");
                return null;
            }

            return controller;
        }
    }
}
