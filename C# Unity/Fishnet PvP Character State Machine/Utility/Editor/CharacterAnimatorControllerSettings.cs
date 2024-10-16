using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.App.Scripts.Characters
{
    [CreateAssetMenu(fileName = "CharacterAnimatorControllerSettings", menuName = "Cardium/Settings/Character Animator Controller Settings")]
    public class CharacterAnimatorControllerSettings : SerializedScriptableObject
    {
        [Required]
        public AnimatorController BaseController;
        [Required]
        public string UsedControllerName = "CharacterAnimator";
        [Required]
        [Tooltip("Prefabs with animator that need a reference update")]
        public List<GameObject> PrefabsToUpdate = new List<GameObject>();

        [Title("Transition Settings")]
        [Min(0)]
        [Tooltip("Default fixed transition duration")]
        public float SkillStateExitTransitionDuration = 0.25f;

        [Tooltip("If a certain state needs a different exit duration, put the animation clips name and the exit duration in real seconds.")]
        public Dictionary<string, float> SkillStateExitTransitionDurationOverrides = new Dictionary<string, float>();
    }
}