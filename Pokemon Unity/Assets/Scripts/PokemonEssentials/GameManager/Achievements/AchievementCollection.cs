using System.Collections.Generic;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// ScriptableObject catalogue of all achievements in the game.
    /// Assign one instance to <see cref="AchievementManager"/>.
    /// Create via: PokemonUnity → Achievement Collection
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementCollection",
                     menuName = "PokemonUnity/Achievement Collection", order = 21)]
    public class AchievementCollection : ScriptableObject
    {
        [SerializeField] private AchievementDefinition[] achievements;

        private Dictionary<string, AchievementDefinition> _cache;

        /// <summary>All registered achievements.</summary>
        public IReadOnlyList<AchievementDefinition> All => achievements;

        /// <summary>Look up an achievement by id (O(1) after first call).</summary>
        public AchievementDefinition Get(string id)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, AchievementDefinition>();
                if (achievements != null)
                    foreach (var a in achievements)
                        if (a != null && !string.IsNullOrEmpty(a.id))
                            _cache[a.id] = a;
            }
            _cache.TryGetValue(id, out AchievementDefinition result);
            return result;
        }

        private void OnEnable() => _cache = null; // reset cache when asset reloads
    }
}
