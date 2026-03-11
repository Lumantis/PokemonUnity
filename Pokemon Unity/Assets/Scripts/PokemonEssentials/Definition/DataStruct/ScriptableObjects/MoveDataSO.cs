using UnityEngine;
using PokemonUnity.Combat;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Damage category for a move.
    /// </summary>
    public enum DamageCategory { Physical, Special, Status }

    /// <summary>
    /// ScriptableObject storing static data for a single move.
    /// Create via menu: PokemonUnity → Move Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewMoveData", menuName = "PokemonUnity/Move Data", order = 2)]
    public class MoveDataSO : ScriptableObject
    {
        [Header("Identity")]
        public Moves          id;
        public string         displayName;
        [TextArea(2, 4)]
        public string         description;

        [Header("Type & Category")]
        public Types          type;
        public DamageCategory category;

        [Header("Stats")]
        [Range(0, 250)] public int basePower;   // 0 = variable / status
        [Range(0, 101)] public int accuracy;    // 0 = never misses
        [Range(1,  64)] public int pp = 10;
        [Range(-7,  7)] public int priority;

        [Header("Flags")]
        public bool makesContact;
        public bool snatchable;
        public bool mirrorMovable = true;
        public bool soundBased;
        public bool punchMove;
        public bool bitingMove;
        public bool ballisticMove;

        [Header("Effect")]
        [Range(0, 100)] public int effectChance; // % chance of secondary effect (0 = always/never)
        [TextArea(1, 2)]
        public string effectDescription;
    }
}
