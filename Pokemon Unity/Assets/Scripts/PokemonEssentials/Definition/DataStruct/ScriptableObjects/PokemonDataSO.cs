using UnityEngine;
using PokemonUnity;
using PokemonUnity.Monster;
using PokemonUnity.Combat;
using PokemonUnity.Inventory;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// ScriptableObject storing static data for a single Pokémon species.
    /// Create via menu: PokemonUnity → Pokemon Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewPokemonData", menuName = "PokemonUnity/Pokemon Data", order = 1)]
    public class PokemonDataSO : ScriptableObject
    {
        [Header("Identity")]
        public Pokemons id;
        public string   displayName;
        [TextArea(2, 4)]
        public string   pokedexEntry;

        [Header("Types")]
        public Types primaryType;
        public Types secondaryType = Types.NONE;

        [Header("Base Stats")]
        [Range(1, 255)] public int baseHp;
        [Range(1, 255)] public int baseAttack;
        [Range(1, 255)] public int baseDefense;
        [Range(1, 255)] public int baseSpAtk;
        [Range(1, 255)] public int baseSpDef;
        [Range(1, 255)] public int baseSpeed;

        /// <summary>Computed Base Stat Total (read-only).</summary>
        public int BST => baseHp + baseAttack + baseDefense + baseSpAtk + baseSpDef + baseSpeed;

        [Header("Catch & Breeding")]
        [Range(3, 255)]  public int   catchRate  = 45;
        [Range(0, 8)]    public int   eggCycles  = 20;
        [Range(-1, 254)] public float genderRatio = 127.5f; // 255 = genderless
        public GrowthRate growthRate = GrowthRate.MediumFast;
        public int        baseExp    = 64;
        public int        baseHappiness = 70;

        [Header("Sprites & Sounds")]
        public Sprite   frontSprite;
        public Sprite   backSprite;
        public Sprite   iconSprite;
        public AudioClip cry;

        [Header("Evolution")]
        public PokemonDataSO[] evolutions;
        [Tooltip("Level at which evolution occurs (0 = item/trade evolution)")]
        public int[]           evolutionLevels;
        public Items[]         evolutionItems;

        [Header("Learnset")]
        [Tooltip("Index = level - 1. Moves.NONE means no move at that level.")]
        public Moves[] learnset;
        public Moves[] tmHmMoves;
        public Moves[] eggMoves;
    }
}
