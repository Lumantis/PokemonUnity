using UnityEngine;
using PokemonUnity.Combat;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// ScriptableObject defining the visual effect for a single move.
    /// Create via: PokemonUnity → VFX → Move VFX Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewMoveVFX", menuName = "PokemonUnity/VFX/Move VFX Data", order = 10)]
    public class MoveVFXData : ScriptableObject
    {
        [Header("Identity")]
        public Moves moveId;
        public Types moveType;

        [Header("Particle Effect")]
        [Tooltip("If null, a colour-based fallback particle is used.")]
        public ParticleSystem particlePrefab;

        [Header("Colours (used for fallback particle)")]
        public Color primaryColor   = Color.white;
        public Color secondaryColor = Color.white;

        [Header("Timing & Scale")]
        [Min(0.1f)] public float duration = 1f;
        [Min(0.1f)] public float scale    = 1f;

        [Header("Placement")]
        [Tooltip("Play the effect at the attacker's position.")]
        public bool  playOnAttacker = true;
        [Tooltip("Play the effect at the defender's position.")]
        public bool  playOnDefender = true;
        public Vector3 attackerOffset = Vector3.zero;
        public Vector3 defenderOffset = Vector3.zero;

        [Header("Fallback")]
        [Tooltip("Use a coloured particle when no prefab is assigned.")]
        public bool useTypeColorFallback = true;
    }
}
