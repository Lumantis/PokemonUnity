using System.Collections.Generic;
using UnityEngine;
using PokemonUnity.Combat;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// ScriptableObject catalogue of all move VFX data.
    /// Assign one globally and reference it from <see cref="BattleVFXManager"/>.
    /// Create via: PokemonUnity → VFX → Move VFX Library
    /// </summary>
    [CreateAssetMenu(fileName = "MoveVFXLibrary", menuName = "PokemonUnity/VFX/Move VFX Library", order = 11)]
    public class MoveVFXLibrary : ScriptableObject
    {
        [SerializeField] private MoveVFXData[] allVFX;

        private Dictionary<Moves, MoveVFXData> _cache;

        /// <summary>Build the fast-lookup cache. Call once on startup.</summary>
        public void Initialize()
        {
            _cache = new Dictionary<Moves, MoveVFXData>();
            if (allVFX == null) return;
            foreach (var vfx in allVFX)
                if (vfx != null)
                    _cache[vfx.moveId] = vfx;
        }

        /// <summary>Returns the VFX data for <paramref name="move"/>, or null if not registered.</summary>
        public MoveVFXData GetVFX(Moves move)
        {
            if (_cache == null) Initialize();
            _cache.TryGetValue(move, out MoveVFXData data);
            return data;
        }

        // ── Type-colour fallback ──────────────────────────────────────────────
        public static readonly Dictionary<Types, Color> TypeColors =
            new Dictionary<Types, Color>
        {
            { Types.NORMAL,   new Color(0.66f, 0.65f, 0.60f) },
            { Types.FIRE,     new Color(1.00f, 0.40f, 0.10f) },
            { Types.WATER,    new Color(0.25f, 0.55f, 1.00f) },
            { Types.ELECTRIC, new Color(1.00f, 0.84f, 0.00f) },
            { Types.GRASS,    new Color(0.25f, 0.80f, 0.25f) },
            { Types.ICE,      new Color(0.60f, 0.90f, 1.00f) },
            { Types.FIGHTING, new Color(0.75f, 0.10f, 0.10f) },
            { Types.POISON,   new Color(0.65f, 0.15f, 0.65f) },
            { Types.GROUND,   new Color(0.88f, 0.75f, 0.30f) },
            { Types.FLYING,   new Color(0.65f, 0.65f, 1.00f) },
            { Types.PSYCHIC,  new Color(1.00f, 0.25f, 0.50f) },
            { Types.BUG,      new Color(0.65f, 0.80f, 0.10f) },
            { Types.ROCK,     new Color(0.70f, 0.65f, 0.25f) },
            { Types.GHOST,    new Color(0.45f, 0.35f, 0.55f) },
            { Types.DRAGON,   new Color(0.40f, 0.20f, 1.00f) },
            { Types.DARK,     new Color(0.35f, 0.25f, 0.20f) },
            { Types.STEEL,    new Color(0.70f, 0.70f, 0.80f) },
            { Types.FAIRY,    new Color(1.00f, 0.65f, 0.80f) },
        };

        public static Color GetTypeColor(Types type)
            => TypeColors.TryGetValue(type, out Color c) ? c : Color.white;
    }
}
