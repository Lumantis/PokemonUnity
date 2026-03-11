using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>Category grouping for achievements.</summary>
    public enum AchievementCategory
    {
        Story,
        Collection,
        Battle,
        Exploration,
        Special
    }

    /// <summary>
    /// ScriptableObject defining a single achievement.
    /// Create via: PokemonUnity → Achievement
    /// </summary>
    [CreateAssetMenu(fileName = "NewAchievement", menuName = "PokemonUnity/Achievement", order = 20)]
    public class AchievementDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable unique key. Never change after release.")]
        public string id;
        public string title;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Category & Visibility")]
        public AchievementCategory category = AchievementCategory.Story;
        [Tooltip("Hidden achievements show ??? until unlocked.")]
        public bool isHidden;

        [Header("Progress")]
        [Tooltip("If true, tracks a counter (e.g., catch 100 Pokémon).")]
        public bool isProgressive;
        [Tooltip("Target count for progressive achievements.")]
        [Min(1)] public int progressGoal = 1;

        [Header("Reward")]
        [Tooltip("Optional flavour text shown on unlock.")]
        [TextArea(1, 2)]
        public string rewardDescription;
    }
}
